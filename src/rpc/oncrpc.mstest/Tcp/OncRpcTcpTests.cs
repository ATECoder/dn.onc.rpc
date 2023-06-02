using System.ComponentModel;
using System.Diagnostics;
using System.Text;

using cc.isr.ONC.RPC.MSTest.Codecs;
using cc.isr.ONC.RPC.Portmap;
using cc.isr.ONC.RPC.Server;

namespace cc.isr.ONC.RPC.MSTest.Tcp;

[TestClass]
public class OncRpcTcpTests
{

    #region " construction and cleanup "

    /// <summary>   Gets or sets the server start time typical. </summary>
    /// <value> The server start time typical. </value>
    public static int ServerStartTimeTypical { get; set; } = 3500;

    /// <summary>   Gets or sets the server start loop delay. </summary>
    /// <value> The server start loop delay. </value>
    public static int ServerStartLoopDelay { get; set; } = 100;


    /// <summary> Initializes the test class before running the first test. </summary>
    /// <param name="testContext"> Gets or sets the test context which provides information about
    /// and functionality for the current test run. </param>
    /// <remarks>Use ClassInitialize to run code before running the first test in the class</remarks>
    [ClassInitialize()]
    public static void InitializeTestClass( TestContext testContext )
    {
        try
        {
            _classTestContext = testContext;
            string methodFullName = $"{testContext.FullyQualifiedTestClassName}.{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}";
            if ( Logger is null )
                Console.WriteLine( methodFullName );
            else
                Logger?.LogMemberInfo( methodFullName );

            _server = new();

            _server.PropertyChanged += OnServerPropertyChanged;
            _server.ThreadExceptionOccurred -= OnThreadException;
            _ = Task.Factory.StartNew( () => {

                Logger?.LogInformation( "starting the Portmap service; this takes ~3.5 seconds..." );
                using OncRpcEmbeddedPortmapServiceStub epm = OncRpcEmbeddedPortmapServiceStub.StartEmbeddedPortmapService();
                epm.EmbeddedPortmapService!.ThreadExceptionOccurred += OnThreadException;

                Logger?.LogInformation( "starting the server task; this takes ~2.4 seconds..." );
                _server.Run();
            } );

            Logger?.LogInformation( $"{nameof( OncRpcTcpServer )} waiting listening {DateTime.Now:ss.fff}" );

            // because the initializing task is not awaited, we need to wait for the server to start here.

            if ( !_server.ServerStarted( 2 * OncRpcTcpTests.ServerStartTimeTypical, OncRpcTcpTests.ServerStartLoopDelay ) )
                throw new InvalidOperationException( "failed starting the ONC/RPC server." );

            Logger?.LogInformation( $"{nameof( OncRpcTcpServer )} is {(_server.Running ? "running" : "idle")}  {DateTime.Now:ss.fff}" );
        }
        catch ( Exception ex )
        {
            if ( Logger is null )
                Console.WriteLine( $"Failed initializing the test class: {ex}" );
            else
                Logger.LogMemberError( "Failed initializing the test class:", ex );

            // cleanup to meet strong guarantees

            try
            {
                CleanupTestClass();
            }
            finally
            {
            }
        }
    }

    private static TestContext? _classTestContext;

    /// <summary> Cleans up the test class after all tests in the class have run. </summary>
    /// <remarks> Use <see cref="CleanupTestClass"/> to run code after all tests in the class have run. </remarks>
    [ClassCleanup()]
    public static void CleanupTestClass()
    {
        OncRpcTcpServer? server = _server;
        if ( server is not null )
        {
            try
            {
                //_server.Shutdown( 2000, 25 );

                bool running = server.Running;
                OncRpcServerStubBase.ShutdownTimeout = 2000;
                Stopwatch sw = Stopwatch.StartNew();
                server.Dispose();

                // it takes 35 ms to dispose the server with 25 ms loop delay and 4 ms with 5 ms loop delay.

                Logger?.LogInformation( $"Running {running}; server disposed in {sw.Elapsed.TotalMilliseconds:0.0} ms" );
                running = server.Running;

                server.PropertyChanged -= OnServerPropertyChanged;
                server.ThreadExceptionOccurred -= OnThreadException;
            }
            catch ( Exception ex )
            {
                Logger?.LogError( "exception cleanup up text fixture", ex );
            }
            finally
            {
                _server = null;
                _classTestContext = null;
            }
        }

    }

    private IDisposable? _loggerScope;

    private LoggerTraceListener<OncRpcTcpTests>? _traceListener;

    /// <summary> Initializes the test class instance before each test runs. </summary>
    [TestInitialize()]
    public void InitializeBeforeEachTest()
    {
        if ( Logger is not null )
        {
            this._loggerScope = Logger.BeginScope( this.TestContext?.TestName ?? string.Empty );
            this._traceListener = new LoggerTraceListener<OncRpcTcpTests>( Logger );
            _ = Trace.Listeners.Add( this._traceListener );
        }
    }

    /// <summary> Cleans up the test class instance after each test has run. </summary>
    [TestCleanup()]
    public void CleanupAfterEachTest()
    {
        Assert.IsFalse( this._traceListener?.Any( TraceEventType.Error ),
            $"{nameof( this._traceListener )} should have no {TraceEventType.Error} messages" );
        this._loggerScope?.Dispose();
        this._traceListener?.Dispose();
        Trace.Listeners.Clear();
    }

    /// <summary>
    /// Gets or sets the test context which provides information about and functionality for the
    /// current test run.
    /// </summary>
    /// <value> The test context. </value>
    public TestContext? TestContext { get; set; }

    /// <summary>   Gets a logger instance for this category. </summary>
    /// <value> The logger. </value>
    public static ILogger<OncRpcTcpTests>? Logger { get; } = LoggerProvider.InitLogger<OncRpcTcpTests>();

    private static OncRpcTcpServer? _server;

    #endregion

    #region " initialization tests "

    /// <summary>   (Unit Test Method) 00 logger should be enabled. </summary>
    /// <remarks>   2023-05-31. </remarks>
    [TestMethod]
    public void A00LoggerShouldBeEnabled()
    {
        Assert.IsNotNull( Logger, $"{nameof( Logger )} should initialize" );
        Assert.IsTrue( Logger.IsEnabled( LogLevel.Information ),
            $"{nameof( Logger )} should be enabled for the {LogLevel.Information} {nameof( LogLevel )}" );
    }

    /// <summary>   (Unit Test Method) 01 logger trace listener should have messages. </summary>
    /// <remarks>   2023-06-01. </remarks>
    [TestMethod]
    public void A01LoggerTraceListenerShouldHaveMessages()
    {
        Assert.IsNotNull( this._traceListener, $"{nameof( this._traceListener )} should initialize" );
        Assert.IsTrue( Trace.Listeners.Count > 0, $"{nameof( Trace )} should have non-zero {nameof( Trace.Listeners )}" );
        Trace.TraceError( "Testing tracing an error" ); Trace.Flush();
        Assert.IsTrue( this._traceListener?.Any( TraceEventType.Error ), $"{nameof( this._traceListener )} should have {TraceEventType.Error} messages" );

        // no need to report errors for this test.

        this._traceListener?.Clear();
    }

    #endregion

    private static void OnServerPropertyChanged( object? sender, PropertyChangedEventArgs e )
    {
        if ( sender is not OncRpcTcpServer ) return;
        switch ( e.PropertyName )
        {
            case nameof( OncRpcTcpServer.ReadMessage ):
                Logger?.LogInformation( (( OncRpcTcpServer ) sender).ReadMessage );
                break;
            case nameof( OncRpcTcpServer.WriteMessage ):
                Logger?.LogInformation( (( OncRpcTcpServer ) sender).WriteMessage );
                break;
            case nameof( OncRpcTcpServer.PortNumber ):
                Logger?.LogInformation( $"{e.PropertyName} set to {(( OncRpcTcpServer ) sender).PortNumber}" );
                break;
            case nameof( OncRpcTcpServer.IPv4Address ):
                Logger?.LogInformation( $"{e.PropertyName} set to {(( OncRpcTcpServer ) sender).IPv4Address}" );
                break;
            case nameof( OncRpcTcpServer.Running ):
                Logger?.LogInformation( $"{e.PropertyName} set to {(( OncRpcTcpServer ) sender).Running}" );
                break;
        }
    }

    private static void OnThreadException( object? sender, ThreadExceptionEventArgs e )
    {
        string name = "unknown";
        if ( sender is OncRpcTcpServer ) name = nameof( OncRpcTcpServer );
        if ( sender is OncRpcServerStubBase ) name = nameof( OncRpcServerStubBase );

        Logger?.LogError( $"{name}  encountered an exception during an asynchronous operation", e.Exception );
    }

    /// <summary>   (Unit Test Method) server should be listening. </summary>
    /// <remarks>
    /// <code>
    /// Standard Output: 
    /// 2023-02-04 19:28:24.828,Running set to False
    /// 2023-02-04 19:28:24.841, Running True; server disposed in 15.7 ms
    /// </code>
    /// </remarks>
    [TestMethod]
    public void ServerShouldBeListening()
    {
        Assert.IsTrue( _server?.Running );
    }

    /// <summary>   Assert client should connect. </summary>
    /// <param name="client">   The client. </param>
    /// <param name="host">     The host. </param>
    /// <param name="version">  The version. </param>
    private static void AssertClientShouldConnect( OncRpcTcpTestClient client, IPAddress host, int version )
    {
        Logger?.LogInformation( "Connecting... " );
        client.Connect( host, version );
        Assert.IsTrue( client.Connected, "should be connected" );
        Logger?.LogInformation( $"{client.Host} connected." );
    }

    /// <summary>   (Unit Test Method) client should connect. </summary>
    /// <remarks>
    /// <code>
    /// Standard Output: 
    /// 2023-02-04 19:28:24.815,Connecting...
    /// 2023-02-04 19:28:24.815,127.0.0.1 connected.
    /// </code>
    /// </remarks>
    [TestMethod]
    public void ClientShouldConnect()
    {
        using OncRpcTcpTestClient client = new();
        AssertClientShouldConnect( client, IPAddress.Loopback, RpcProgramConstants.Version1 );
    }

    /// <summary>   (Unit Test Method) client should connect version 2. </summary>
    /// <remarks>
    /// <code>
    /// Standard Output: 
    /// 2023-02-04 19:28:24.816,Connecting...
    /// 2023-02-04 19:28:24.816,127.0.0.1 connected.
    /// </code>
    /// </remarks>
    [TestMethod]
    public void ClientShouldConnectVersion2()
    {
        using OncRpcTcpTestClient client = new();
        AssertClientShouldConnect( client, IPAddress.Loopback, RpcProgramConstants.Version2 );
    }

    /// <summary>   Assert client should ping. </summary>
    /// <param name="client">   The client. </param>
    private static void AssertClientShouldPing( OncRpcTcpTestClient client )
    {
        Logger?.LogInformation( "Pinging..." );
        client.CallRemoteProcedureNull();
        Logger?.LogInformation( $"{client.Host} pinged." );
    }

    /// <summary>   Assert client should ping. </summary>
    /// <remarks>
    /// <code>
    /// Standard Output: 
    /// 2023-02-04 19:28:24.819,Connecting...
    /// 2023-02-04 19:28:24.820,127.0.0.1 connected.
    /// 2023-02-04 19:28:24.820,Pinging...
    /// 2023-02-04 19:28:24.820,127.0.0.1 pinged.
    /// </code>
    /// </remarks>
    [TestMethod]
    public void ClientShouldPing()
    {
        using OncRpcTcpTestClient client = new();
        AssertClientShouldConnect( client, IPAddress.Loopback, RpcProgramConstants.Version1 );
        AssertClientShouldPing( client );
    }

    /// <summary>   (Unit Test Method) client should ping version 2. </summary>
    /// <remarks>
    /// <code>
    /// Standard Output: 
    /// 2023-02-04 19:28:24.820,Connecting...
    /// 2023-02-04 19:28:24.821,127.0.0.1 connected.
    /// 2023-02-04 19:28:24.821,Pinging...
    /// 2023-02-04 19:28:24.821,127.0.0.1 pinged.
    /// </code>
    /// </remarks>
    [TestMethod]
    public void ClientShouldPingVersion2()
    {
        using OncRpcTcpTestClient client = new();
        AssertClientShouldConnect( client, IPAddress.Loopback, RpcProgramConstants.Version2 );
        AssertClientShouldPing( client );
    }

    /// <summary>   Assert client should fail authentication. </summary>
    /// <param name="client">   The client. </param>
    private static void AssertClientShouldFailAuthentication( OncRpcTcpTestClient client )
    {
        int userId = 0;
        int groupId = 0;
        string machineName = AuthenticationConstants.MachineName;
        Logger?.LogInformation( $"{client.Host} checking  {nameof( OncRpcAuthType.OncRpcAuthTypeUnix )} on machine: {machineName} with invalid credentials user id: {userId} & Group ID = {groupId}: " );
        try
        {
            client.CallAuthenticate( AuthenticationConstants.MachineName, userId, groupId );
        }
        catch ( OncRpcAuthException ae )
        {
            if ( ae.AuthStatus != OncRpcAuthStatus.OncRpcAuthBadCredential )
            {
                Assert.Fail( $"received {nameof( OncRpcAuthException )} with a incorrect status of {ae.AuthStatus}" );
            }
            Logger?.LogInformation( $"{client.Host} expected exception thrown." );
        }
        catch ( OncRpcException e )
        {
            Assert.Fail( $"{client.Host} received {nameof( OncRpcException )}: {e}" );
        }
        catch ( Exception ex )
        {
            Assert.Fail( $"{client.Host} received exception: {ex}" );
        }
    }

    /// <summary>   (Unit Test Method) client should fail authentication. </summary>
    /// <remarks>
    /// <code>
    /// Standard Output: 
    /// 2023-02-04 19:28:24.818,Connecting...
    /// 2023-02-04 19:28:24.819,127.0.0.1 connected.
    /// 2023-02-04 19:28:24.819,127.0.0.1 checking OncRpcAuthTypeUnix on machine: LIMEDEVB with invalid credentials user id: 0 & Group ID = 0:
    /// </code>
    /// </remarks>
    [TestMethod]
    public void ClientShouldFailAuthentication()
    {
        using OncRpcTcpTestClient client = new();
        AssertClientShouldConnect( client, IPAddress.Loopback, RpcProgramConstants.Version1 );
        AssertClientShouldFailAuthentication( client );
    }

    /// <summary>   Assert client should authenticate. </summary>
    /// <param name="client">   The client. </param>
    private static void AssertClientShouldAuthenticate( OncRpcTcpTestClient client )
    {
        int userId = AuthenticationConstants.UserIdentity;
        int groupId = AuthenticationConstants.GroupIdentity;
        string machineName = AuthenticationConstants.MachineName;
        Logger?.LogInformation( $"{client.Host} checking {nameof( OncRpcAuthType.OncRpcAuthTypeUnix )} on machine: {machineName} with valid credentials user id: {userId} & Group ID = {groupId}: " );
        try
        {
            client.CallAuthenticate( AuthenticationConstants.MachineName, userId, groupId );
            Logger?.LogInformation( $"{client.Host} valid credentials authenticated" );
        }
        catch ( OncRpcAuthException ae )
        {
            Assert.Fail( $"{client.Host} received {nameof( OncRpcAuthException )} with a status of {ae.AuthStatus}: {ae}" );
        }
    }

    /// <summary>   (Unit Test Method) client should authenticate. </summary>
    /// <remarks>
    /// <code>
    /// Standard Output: 
    /// 2023-02-04 19:28:17.695,cc.isr.ONC.RPC.MSTest.Tcp.OncRpcTcpTests.OncRpcTcpTests
    /// 2023-02-04 19:28:17.701,OncRpcTcpServer waiting listening 17.701
    /// 2023-02-04 19:28:17.701,starting the Portmap service; this takes ~3.5 seconds...
    /// 2023-02-04 19:28:17.702,Checking for Portmap service
    /// 2023-02-04 19:28:17.720,No Portmap service available.
    /// 2023-02-04 19:28:17.720,Creating embedded Portmap instance
    /// 2023-02-04 19:28:17.931,Portmap service started; checked 18.1 ms.
    /// 2023-02-04 19:28:17.931,starting the server task; this takes ~2.4 seconds...
    /// 2023-02-04 19:28:17.938,Running set to True
    /// 2023-02-04 19:28:24.779, OncRpcTcpServer is running  24.779
    /// 2023-02-04 19:28:24.784,Connecting...
    /// 2023-02-04 19:28:24.787,127.0.0.1 connected.
    /// 2023-02-04 19:28:24.787,127.0.0.1 checking OncRpcAuthTypeUnix on machine: LIMEDEVB with valid credentials user id: 42 & Group ID = 815:
    /// 2023-02-04 19:28:24.790,127.0.0.1 valid credentials authenticated
    /// </code>
    /// </remarks>
    [TestMethod]
    public void ClientShouldAuthenticate()
    {
        // this seems to be required to allow the sequence to work.
        // this.ClientShouldConnect();

        using OncRpcTcpTestClient client = new();
        AssertClientShouldConnect( client, IPAddress.Loopback, RpcProgramConstants.Version1 );
        AssertClientShouldAuthenticate( client );
    }

    /// <summary>   Assert client should echo messages. </summary>
    /// <param name="client">   The client. </param>
    /// <param name="messages"> The messages. </param>
    private static void AssertClientShouldEchoMessages( OncRpcTcpTestClient client, string[] messages )
    {
        foreach ( string message in messages )
        {
            Logger?.LogInformation( $"{client.Host} checking echo of {message}: " );
            string echoed = client.CallRemoteProcedureEcho( message );
            Assert.AreEqual( message, echoed, $"{client.Host} answer '{echoed}' does not match '{message}' call" );
            Logger?.LogInformation( $"{client.Host} echoed {echoed}" );
        }
    }

    /// <summary>   (Unit Test Method) client should echo messages. </summary>
    /// <remarks>
    /// <code>
    ///  Standard Output: 
    /// 2023-02-04 19:28:24.816,Connecting...
    /// 2023-02-04 19:28:24.817,127.0.0.1 connected.
    /// 2023-02-04 19:28:24.817,127.0.0.1 checking OncRpcAuthTypeUnix on machine: LIMEDEVB with valid credentials user id: 42 & Group ID = 815:
    /// 2023-02-04 19:28:24.817,127.0.0.1 valid credentials authenticated
    /// 2023-02-04 19:28:24.817,127.0.0.1 checking echo of UNIX:
    /// 2023-02-04 19:28:24.817,127.0.0.1 echoed UNIX
    /// 2023-02-04 19:28:24.817,127.0.0.1 checking echo of AUTH:
    /// 2023-02-04 19:28:24.817,127.0.0.1 echoed AUTH
    /// 2023-02-04 19:28:24.817,127.0.0.1 checking echo of is like:
    /// 2023-02-04 19:28:24.818,127.0.0.1 echoed is like
    /// 2023-02-04 19:28:24.818,127.0.0.1 checking echo of* NO* authentication:
    /// 2023-02-04 19:28:24.818,127.0.0.1 echoed* NO* authentication
    /// 2023-02-04 19:28:24.818,127.0.0.1 checking echo of --:
    /// 2023-02-04 19:28:24.818,127.0.0.1 echoed --
    /// 2023-02-04 19:28:24.818,127.0.0.1 checking echo of it:
    /// 2023-02-04 19:28:24.818,127.0.0.1 echoed it
    /// 2023-02-04 19:28:24.818,127.0.0.1 checking echo of uses:
    /// 2023-02-04 19:28:24.818,127.0.0.1 echoed uses
    /// 2023-02-04 19:28:24.818,127.0.0.1 checking echo of* NO CRYPTOGRAPHY*:
    /// 2023-02-04 19:28:24.818,127.0.0.1 echoed* NO CRYPTOGRAPHY*
    /// 2023-02-04 19:28:24.818,127.0.0.1 checking echo of for securing:
    /// 2023-02-04 19:28:24.818,127.0.0.1 echoed for securing
    /// 2023-02-04 19:28:24.818,127.0.0.1 checking echo of ONC/RPC messages:
    /// 2023-02-04 19:28:24.818,127.0.0.1 echoed ONC/RPC messages
    /// </code>
    /// </remarks>
    [TestMethod]
    public void ClientShouldEchoMessages()
    {
        using OncRpcTcpTestClient client = new();
        AssertClientShouldConnect( client, IPAddress.Loopback, RpcProgramConstants.Version1 );
        AssertClientShouldAuthenticate( client );
        string[] messages = new string[] { "UNIX", "AUTH", "is like", "*NO* authentication", "--", "it", "uses", "*NO CRYPTOGRAPHY*", "for securing", "ONC/RPC messages" };
        AssertClientShouldEchoMessages( client, messages );
    }

    /// <summary>   Assert client should echo. </summary>
    /// <param name="client">   The client. </param>
    private static void AssertClientShouldEcho( OncRpcTcpTestClient client )
    {
        Logger?.LogInformation( $"{client.Host} About to echo: " );
        string expected = "Hello, Remote Tea!";
        string actual = client.CallRemoteProcedureEcho( expected );
        Assert.AreEqual( expected, actual );
        Logger?.LogInformation( $"{client.Host} echoed '{actual}'" );
    }

    /// <summary>   Assert client should concatenate. </summary>
    /// <param name="client">   The client. </param>
    private static void AssertClientShouldConcatenate( OncRpcTcpTestClient client )
    {
        Logger?.LogInformation( "About to concatenate: " );
        StringVectorCodec strings = new();
        strings.SetValues( new StringCodec[] { new StringCodec( "Hello, " ), new StringCodec( "Remote " ), new StringCodec( "Tea!" ) } );
        string expected = "Hello, Remote Tea!";
        string actual = client.CallRemoteProcedureConcatenateInputParameters( strings );
        Assert.AreEqual( expected, actual );
        Logger?.LogInformation( $"{client.Host} concatenated '{actual}'" );
    }

    /// <summary>   Assert client should concatenate exactly. </summary>
    /// <param name="client">   The client. </param>
    private static void AssertClientShouldConcatenateExactly( OncRpcTcpTestClient client )
    {
        Logger?.LogInformation( "About to concatenating exactly three strings: " );
        string expected = "(1:Hello )(2:Remote )(3:Tea!)";
        string actual = client.CallRemoteProcedureConcatenatedThreeItems( "(1:Hello )", "(2:Remote )", "(3:Tea!)" );
        Assert.AreEqual( expected, actual );
        Logger?.LogInformation( $"{client.Host} concatenated '{actual}'" );
    }

    /// <summary>   Assert client should check for foo. </summary>
    /// <param name="client">   The client. </param>
    private static void AssertClientShouldCheckForFoo( OncRpcTcpTestClient client )
    {
        Assert.IsFalse( client.CallRemoteProcedureCompareInputToFoo( EnumFoo.BAR ), $"oops: but a {EnumFoo.BAR} is not a {EnumFoo.FOO}!" );
        Assert.IsTrue( client.CallRemoteProcedureCompareInputToFoo( EnumFoo.FOO ), $"oops: a {EnumFoo.FOO} should be a {EnumFoo.FOO}!" );
    }

    /// <summary>   Assert client should get foo. </summary>
    /// <param name="client">   The client. </param>
    private static void AssertClientShouldGetFoo( OncRpcTcpTestClient client )
    {
        Logger?.LogInformation( $"{client.Host} About to get a foo: " );
        Assert.AreEqual( client.CallRemoteProcedureReturnEnumFooValue(), ( int ) EnumFoo.FOO, $"oops: got a {EnumFoo.BAR} instead of a {EnumFoo.FOO}!" );
    }

    /// <summary>   Assert client should get numbered foo. </summary>
    /// <param name="client">   The client. </param>
    private static void AssertClientShouldGetNumberedFoo( OncRpcTcpTestClient client )
    {
        Logger?.LogInformation( $"{client.Host} About to get a numbered foo string: " );
        EnumFoo expectedValue = EnumFoo.FOO;
        string expected = OncRpcTcpServer.ReturnYouAreFooValue( ( int ) expectedValue );
        string echo = client.CallRemoteProcedureReturnYouAreFooValue( expectedValue );
        Assert.AreEqual( expected, echo, $"{client.Host} oops: should echo '{expected}')" );
    }

    /// <summary>   Assert client should prepend linked list. </summary>
    /// <param name="client">   The client. </param>
    private static void AssertClientShouldPrependLinkedList( OncRpcTcpTestClient client )
    {
        Logger?.LogInformation( $"{client.Host} Linked List test: " );
        LinkedListCodec node1 = new() {
            Foo = 0
        };
        LinkedListCodec node2 = new() {
            Foo = 8
        };
        node1.Next = node2;
        LinkedListCodec node3 = new() {
            Foo = 15
        };
        node2.Next = node3;
        LinkedListCodec? list = client.CallRemoteProcedurePrependLinkedList( node1 );
        LinkedListCodec? expected = OncRpcTcpServer.PrependLinkedList( node1 );
        Assert.IsNotNull( list, "list should get built" );
        LinkedListCodec? actual = list;
        int i = 0;
        StringBuilder builder = new();
        while ( expected != null )
        {
            i++;
            Assert.IsNotNull( actual, $"node{i} actual list should have the same number of nodes as expected" ); ;
            Assert.AreEqual( expected.Foo, actual.Foo, $"nodes {i} should have the same value" );
            _ = builder.Append( actual.Foo + ", " );
            actual = actual.Next;
            expected = expected.Next;
        }
        Logger?.LogInformation( $"built list {builder}" );
    }

    /// <summary>   Assert client should link linked list. </summary>
    /// <param name="client">   The client. </param>
    private static void AssertClientShouldLinkLinkedList( OncRpcTcpTestClient client )
    {
        Logger?.LogInformation( $"{client.Host} Linking Linked Lists test: " );
        LinkedListCodec node1 = new() {
            Foo = 0
        };
        LinkedListCodec node2 = new() {
            Foo = 8
        };
        LinkedListCodec node3 = new() {
            Foo = 15
        };
        node2.Next = node3;
        LinkedListCodec? list = client.CallRemoteProcedureLinkListItems( node2, node1 );
        // link the lists as expected by the server
        node2.Next = node1;
        LinkedListCodec? expected = node2;
        Assert.IsNotNull( list, "list should get built" );
        LinkedListCodec? actual = list;
        int i = 0;
        StringBuilder builder = new();
        while ( expected != null )
        {
            i++;
            Assert.IsNotNull( actual, $"node{i} actual list should have the same number of nodes as expected" ); ;
            Assert.AreEqual( expected.Foo, actual.Foo, $"nodes {i} should have the same value" );
            _ = builder.Append( actual.Foo + ", " );
            actual = actual.Next;
            expected = expected.Next;
        }
        Logger?.LogInformation( $"built list {builder}" );
    }

    /// <summary>   (Unit Test Method) client should call remote procedures. </summary>
    /// <remarks>
    /// <code>
    /// Standard Output: 
    /// 2023-02-04 19:28:24.805,Connecting...
    /// 2023-02-04 19:28:24.806,127.0.0.1 connected.
    /// 2023-02-04 19:28:24.806,Pinging...
    /// 2023-02-04 19:28:24.806,127.0.0.1 pinged.
    /// 2023-02-04 19:28:24.806,127.0.0.1 About to echo:
    /// 2023-02-04 19:28:24.808,127.0.0.1 echoed 'Hello, Remote Tea!'
    /// 2023-02-04 19:28:24.808,About to concatenate:
    /// 2023-02-04 19:28:24.809,127.0.0.1 concatenated 'Hello, Remote Tea!'
    /// 2023-02-04 19:28:24.809,About to concatenating exactly three strings:
    /// 2023-02-04 19:28:24.810,127.0.0.1 concatenated '(1:Hello )(2:Remote )(3:Tea!)'
    /// 2023-02-04 19:28:24.811,127.0.0.1 About to get a foo:
    /// 2023-02-04 19:28:24.812,127.0.0.1 About to get a numbered foo string:
    /// 2023-02-04 19:28:24.812,127.0.0.1 Linked List test:
    /// 2023-02-04 19:28:24.813,built list 42, 0, 8, 15,
    /// 2023-02-04 19:28:24.813,127.0.0.1 Linking Linked Lists test:
    /// 2023-02-04 19:28:24.814, built list 8, 0,
    /// 2023-02-04 19:28:24.814,All tests passed.
    /// </code>
    /// </remarks>    
    [TestMethod]
    public void ClientShouldCallRemoteProcedures()
    {
        using OncRpcTcpTestClient client = new();
        AssertClientShouldConnect( client, IPAddress.Loopback, RpcProgramConstants.Version1 );
        AssertClientShouldPing( client );
        AssertClientShouldEcho( client );
        AssertClientShouldConcatenate( client );
        AssertClientShouldConcatenateExactly( client );
        AssertClientShouldCheckForFoo( client );
        AssertClientShouldGetFoo( client );
        AssertClientShouldGetNumberedFoo( client );
        AssertClientShouldPrependLinkedList( client );
        AssertClientShouldLinkLinkedList( client );
        Logger?.LogInformation( "All tests passed." );
    }

}
