using System.ComponentModel;
using System.Diagnostics;
using System.Text;

using cc.isr.ONC.RPC.Logging;
using cc.isr.ONC.RPC.MSTest.Codecs;
using cc.isr.ONC.RPC.Portmap;
using cc.isr.ONC.RPC.Server;

namespace cc.isr.ONC.RPC.MSTest.Tcp;

[TestClass]
public class OncRpcTcpTests
{

    /// <summary>   Gets or sets the server start time typical. </summary>
    /// <value> The server start time typical. </value>
    public static int ServerStartTimeTypical { get; set; } = 3500;

    /// <summary>   Gets or sets the server start loop delay. </summary>
    /// <value> The server start loop delay. </value>
    public static int ServerStartLoopDelay { get; set; } = 100;


    #region " fixture construction and cleanup "

    /// <summary>   Initializes the fixture. </summary>
    /// <param name="context">  The context. </param>
    [ClassInitialize]
    public static void InitializeFixture( TestContext context )
    {
        try
        {
            _classTestContext = context;
            Logger.Writer.LogInformation( $"{_classTestContext.FullyQualifiedTestClassName}.{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}" );

            _server = new();

            _server.PropertyChanged += OnServerPropertyChanged;
            _server.ThreadExceptionOccurred -= OnThreadException;
            _ = Task.Factory.StartNew( () => {

                Logger.Writer.LogInformation( "starting the portmap service; this takes ~3.5 seconds..." );
                using OncRpcEmbeddedPortmapServiceStub epm = OncRpcEmbeddedPortmapServiceStub.StartEmbeddedPortmapService();
                epm.EmbeddedPortmapService!.ThreadExceptionOccurred += OnThreadException;

                Logger.Writer.LogInformation( "starting the server task; this takes ~2.4 seconds..." );
                _server.Run();
            } );

            Logger.Writer.LogInformation( $"{nameof( OncRpcTcpServer )} waiting listening {DateTime.Now:ss.fff}" );

            // because the initializing task is not awaited, we need to wait for the server to start here.

            if ( !_server.ServerStarted( 2 * OncRpcTcpTests.ServerStartTimeTypical, OncRpcTcpTests.ServerStartLoopDelay ) )
                throw new InvalidOperationException( "failed starting the ONC/RPC server." );

            Logger.Writer.LogInformation( $"{nameof( OncRpcTcpServer )} is {(_server.Running ? "running" : "idle")}  {DateTime.Now:ss.fff}" );
        }
        catch ( Exception ex )
        {
            Logger.Writer.LogMemberError( $"Failed initializing fixture:", ex );
            CleanupFixture();
        }
    }

    public TestContext? TestContext { get; set; }

    private static TestContext? _classTestContext;

    /// <summary>   Cleanup fixture. </summary>
    [ClassCleanup]
    public static void CleanupFixture()
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
                Logging.Logger.Writer.LogInformation( $"Running {running}; server disposed in {sw.Elapsed.TotalMilliseconds:0.0} ms" );
                running = server.Running;

                server.PropertyChanged -= OnServerPropertyChanged;
                server.ThreadExceptionOccurred -= OnThreadException;
            }
            catch ( Exception ex )
            {
                Logger.Writer.LogError( "exception cleanup up text fixture", ex );
            }
            finally
            {
                _server = null;
                _classTestContext = null;
            }
        }

    }


    private static OncRpcTcpServer? _server;

    #endregion

    private static void OnServerPropertyChanged( object? sender, PropertyChangedEventArgs e )
    {
        if ( sender is not OncRpcTcpServer ) return;
        switch ( e.PropertyName )
        {
            case nameof( OncRpcTcpServer.ReadMessage ):
                Logger.Writer.LogInformation( ( ( OncRpcTcpServer ) sender).ReadMessage );
                break;
            case nameof( OncRpcTcpServer.WriteMessage ):
                Logger.Writer.LogInformation( (( OncRpcTcpServer ) sender).WriteMessage );
                break;
            case nameof( OncRpcTcpServer.PortNumber ):
                Logger.Writer.LogInformation( $"{e.PropertyName} set to {(( OncRpcTcpServer ) sender).PortNumber}" );
                break;
            case nameof( OncRpcTcpServer.IPv4Address ):
                Logger.Writer.LogInformation( $"{e.PropertyName} set to {(( OncRpcTcpServer ) sender).IPv4Address}" );
                break;
            case nameof( OncRpcTcpServer.Running ):
                Logger.Writer.LogInformation( $"{e.PropertyName} set to {(( OncRpcTcpServer ) sender).Running}" );
                break;
        }
    }

    private static void OnThreadException( object? sender, ThreadExceptionEventArgs e )
    {
        string name = "unknown";
        if ( sender is OncRpcTcpServer ) name = nameof( OncRpcTcpServer );
        if ( sender is OncRpcServerStubBase ) name = nameof( OncRpcServerStubBase );

        Logger.Writer.LogError( $"{name}  encountered an exception during an asynchronous operation", e.Exception );
    }

    /// <summary>   (Unit Test Method) server should be listening. </summary>
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
        Logger.Writer.LogInformation( "Connecting... " );
        client.Connect( host, version );
        Assert.IsTrue( client.Connected, "should be connected" );
        Logger.Writer.LogInformation( $"{client.Host} connected." );
    }

    /// <summary>   (Unit Test Method) client should connect. </summary>
    /// <remarks>
    /// <code>
    /// Standard Output: 
    ///   2023-02-02 20:00:24.750,Connecting...
    ///   2023-02-02 20:00:24.751,connected.
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
    ///   2023-02-02 20:00:24.751,Connecting...
    ///   2023-02-02 20:00:24.751,connected.
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
        Logger.Writer.LogInformation( "Pinging..." );
        client.CallRemoteProcedureNull();
        Logger.Writer.LogInformation( $"{client.Host} pinged." );
    }

    /// <summary>   Assert client should ping. </summary>
    /// <remarks>
    /// <code>
    /// Standard Output: 
    ///   2023-02-02 20:00:24.755,Connecting...
    ///   2023-02-02 20:00:24.756,connected.
    ///   2023-02-02 20:00:24.756,Pinging...
    ///   2023-02-02 20:00:24.756,pinged.
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
    ///   2023-02-02 20:00:24.756,Connecting...
    ///   2023-02-02 20:00:24.757,connected.
    ///   2023-02-02 20:00:24.757,Pinging...
    ///   2023-02-02 20:00:24.757,pined.
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
        Logger.Writer.LogInformation( $"{client.Host} checking  {nameof( OncRpcAuthType.OncRpcAuthTypeUnix )} on machine: {machineName} with invalid credentials user id: {userId} & Group ID = {groupId}: " );
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
            Logger.Writer.LogInformation( $"{client.Host} expected exception thrown." );
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
    ///   2023-02-02 20:00:24.754,Connecting...
    ///   2023-02-02 20:00:24.754,connected.
    ///   checking OncRpcAuthTypeUnix on machine: limedevb with invalid credentials user id: 0 & Group ID = 0:
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
        Logger.Writer.LogInformation( $"{client.Host} checking {nameof( OncRpcAuthType.OncRpcAuthTypeUnix )} on machine: {machineName} with valid credentials user id: {userId} & Group ID = {groupId}: " );
        try
        {
            client.CallAuthenticate( AuthenticationConstants.MachineName, userId, groupId );
            Logger.Writer.LogInformation( $"{client.Host} valid credentials authenticated" );
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
    ///   2023-02-02 20:00:17.657,cc.isr.ONC.RPC.MSTest.Tcp.OncRpcTcpTests.OncRpcTcpTests
    ///   2023-02-02 20:00:17.658,OncRpcTcpServer waiting listening 17.658
    ///   2023-02-02 20:00:17.658,starting the portmap service; this takes ~3.5 seconds...
    ///   2023-02-02 20:00:17.658,Checking for portmap service
    ///   2023-02-02 20:00:17.763, No portmap service available.
    ///   2023-02-02 20:00:17.763,Creating embedded portmap instance
    ///   2023-02-02 20:00:17.977, Portmap service started; checked 105.2 ms.
    ///   2023-02-02 20:00:17.978,starting the server task; this takes ~2.4 seconds...
    ///   2023-02-02 20:00:17.980,Running set to True
    ///   2023-02-02 20:00:24.736, OncRpcTcpServer is running  24.736
    ///   2023-02-02 20:00:24.737,Connecting...
    ///   2023-02-02 20:00:24.738,connected.
    ///   checking OncRpcAuthTypeUnix on machine: limedevb with valid credentials user id: 42 & Group ID = 815: 2023-02-02 20:00:24.741, valid credentials authenticated
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
            Logger.Writer.LogInformation( $"{client.Host} checking echo of {message}: " );
            string echoed = client.CallRemoteProcedureEcho( message );
            Assert.AreEqual( message, echoed, $"{client.Host} answer '{echoed}' does not match '{message}' call" );
            Logger.Writer.LogInformation( $"{client.Host} echoed {echoed}" );
        }
    }

    /// <summary>   (Unit Test Method) client should echo messages. </summary>
    /// <remarks>
    /// <code>
    /// Standard Output: 
    ///   2023-02-02 20:00:24.752,Connecting...
    ///   2023-02-02 20:00:24.752,connected.
    ///   checking OncRpcAuthTypeUnix on machine: limedevb with valid credentials user id: 42 & Group ID = 815: 2023-02-02 20:00:24.752, valid credentials authenticated
    ///   checking echo of UNIX: 2023-02-02 20:00:24.752, echoed UNIX
    ///   checking echo of AUTH: 2023-02-02 20:00:24.753,echoed AUTH
    ///   checking echo of is like: 2023-02-02 20:00:24.753,echoed is like
    ///   checking echo of *NO* authentication: 2023-02-02 20:00:24.753, echoed *NO* authentication
    ///   checking echo of --: 2023-02-02 20:00:24.753,echoed --
    ///   checking echo of it: 2023-02-02 20:00:24.753, echoed it
    ///   checking echo of uses: 2023-02-02 20:00:24.753,echoed uses
    ///   checking echo of* NO CRYPTOGRAPHY*: 2023-02-02 20:00:24.753,echoed* NO CRYPTOGRAPHY*
    ///   checking echo of for securing: 2023-02-02 20:00:24.753,echoed for securing
    ///   checking echo of ONC/RPC messages: 2023-02-02 20:00:24.754, echoed ONC/RPC messages
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
        Logger.Writer.LogInformation( $"{client.Host} About to echo: " );
        string expected = "Hello, Remote Tea!";
        string actual = client.CallRemoteProcedureEcho( expected );
        Assert.AreEqual( expected, actual );
        Logger.Writer.LogInformation( $"{client.Host} echoed '{actual}'" );
    }

    /// <summary>   Assert client should concatenate. </summary>
    /// <param name="client">   The client. </param>
    private static void AssertClientShouldConcatenate( OncRpcTcpTestClient client )
    {
        Logger.Writer.LogInformation( "About to concatenate: " );
        StringVectorCodec strings = new();
        strings.SetValues( new StringCodec[] { new StringCodec( "Hello, " ), new StringCodec( "Remote " ), new StringCodec( "Tea!" ) } );
        string expected = "Hello, Remote Tea!";
        string actual = client.CallRemoteProcedureConcatenateInputParameters( strings );
        Assert.AreEqual( expected, actual );
        Logger.Writer.LogInformation( $"{client.Host} concatenated '{actual}'" );
    }

    /// <summary>   Assert client should concatenate exactly. </summary>
    /// <param name="client">   The client. </param>
    private static void AssertClientShouldConcatenateExactly( OncRpcTcpTestClient client )
    {
        Logger.Writer.LogInformation( "About to concatenating exactly three strings: " );
        string expected = "(1:Hello )(2:Remote )(3:Tea!)";
        string actual = client.CallRemoteProcedureConcatenatedThreeItems( "(1:Hello )", "(2:Remote )", "(3:Tea!)" );
        Assert.AreEqual( expected, actual );
        Logger.Writer.LogInformation( $"{client.Host} concatenated '{actual}'" );
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
        Logger.Writer.LogInformation( $"{client.Host} About to get a foo: " );
        Assert.AreEqual( client.CallRemoteProcedureReturnEnumFooValue(), ( int ) EnumFoo.FOO, $"oops: got a {EnumFoo.BAR} instead of a {EnumFoo.FOO}!" );
    }

    /// <summary>   Assert client should get numbered foo. </summary>
    /// <param name="client">   The client. </param>
    private static void AssertClientShouldGetNumberedFoo( OncRpcTcpTestClient client )
    {
        Logger.Writer.LogInformation( $"{client.Host} About to get a numbered foo string: " );
        EnumFoo expectedValue = EnumFoo.FOO;
        string expected = OncRpcTcpServer.ReturnYouAreFooValue( ( int ) expectedValue );
        string echo = client.CallRemoteProcedureReturnYouAreFooValue( expectedValue );
        Assert.AreEqual( expected, echo, $"{client.Host} oops: should echo '{expected}')" );
    }

    /// <summary>   Assert client should prepend linked list. </summary>
    /// <param name="client">   The client. </param>
    private static void AssertClientShouldPrependLinkedList( OncRpcTcpTestClient client )
    {
        Logger.Writer.LogInformation( $"{client.Host} Linked List test: " );
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
        Logger.Writer.LogInformation( $"built list {builder}" );
    }

    /// <summary>   Assert client should link linked list. </summary>
    /// <param name="client">   The client. </param>
    private static void AssertClientShouldLinkLinkedList( OncRpcTcpTestClient client )
    {
        Logger.Writer.LogInformation( $"{client.Host} Linking Linked Lists test: " );
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
        Logger.Writer.LogInformation( $"built list {builder}" );
    }

    /// <summary>   (Unit Test Method) client should call remote procedures. </summary>
    /// <remarks>
    /// <code>
    /// Standard Output: 
    ///   2023-02-02 20:00:24.743,Connecting...
    ///   2023-02-02 20:00:24.744,connected.
    ///   2023-02-02 20:00:24.744,Pinging...
    ///   2023-02-02 20:00:24.744,pined.
    ///   2023-02-02 20:00:24.744,About to echo:
    ///   2023-02-02 20:00:24.745,Echoed 'Hello, Remote Tea!'
    ///   2023-02-02 20:00:24.745,About to concatenate:
    ///   2023-02-02 20:00:24.746,concatenated 'Hello, Remote Tea!'
    ///   2023-02-02 20:00:24.746,About to concatenating exactly three strings:
    ///   2023-02-02 20:00:24.746, concatenated '(1:Hello )(2:Remote )(3:Tea!)'
    ///   2023-02-02 20:00:24.747, About to get a foo:
    ///   2023-02-02 20:00:24.748, About to get a numbered foo string:
    ///   2023-02-02 20:00:24.748,Linked List test:
    ///   2023-02-02 20:00:24.749,built list 42, 0, 8, 15,
    ///   2023-02-02 20:00:24.749, Linking Linked Lists test:
    ///   2023-02-02 20:00:24.750,built list 8, 0,
    ///   2023-02-02 20:00:24.750, All tests passed.
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
        Logger.Writer.LogInformation( "All tests passed." );
    }

}
