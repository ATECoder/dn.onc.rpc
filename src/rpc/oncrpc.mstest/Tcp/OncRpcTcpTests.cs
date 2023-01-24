using System.ComponentModel;
using System.Text;

using cc.isr.ONC.RPC.Logging;
using cc.isr.ONC.RPC.MSTest.Codecs;
using cc.isr.ONC.RPC.Portmap;

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
            _ = Task.Factory.StartNew( () => {
                Logger.Writer.LogInformation( "starting the server task; this takes ~6 seconds..." );
                _server.Run();
            } );

            Logger.Writer.LogInformation( $"{nameof( OncRpcTcpServer )} waiting listening {DateTime.Now:ss.fff}" );

            // wait till the server is running.

            _ = _server.ServerStarted( 2 * OncRpcTcpTests.ServerStartTimeTypical, OncRpcTcpTests.ServerStartLoopDelay );

            Logger.Writer.LogInformation( $"{nameof( OncRpcTcpServer )} is {(_server.Running ? "running" : "idle")}  {DateTime.Now:ss.fff}" );
            _portMapService = _server!.EmbeddedPortmapService!.EmbeddedPortmapService;
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
        if ( _server is not null )
        {
            if ( _server.Running )
            {
                _server.StopRpcProcessing();
            }
            _server.Dispose();
            _server = null;
        }
        // this does not solve the test abortion issue on testing the broadcast.
        // _portMapService?.Dispose();
    }

    private static OncRpcPortMapService? _portMapService;
    private static OncRpcTcpServer? _server;


    #endregion

    private static void OnServerPropertyChanged( object? sender, PropertyChangedEventArgs e )
    {
        if ( _server is null ) return;
        switch ( e.PropertyName )
        {
            case nameof( OncRpcTcpServer.ReadMessage ):
                Logger.Writer.LogInformation( _server.ReadMessage );
                break;
            case nameof( OncRpcTcpServer.WriteMessage ):
                Logger.Writer.LogInformation( _server.WriteMessage );
                break;
            case nameof( OncRpcTcpServer.PortNumber ):
                Logger.Writer.LogInformation( $"{e.PropertyName} set to {_server?.PortNumber}" );
                break;
            case nameof( OncRpcTcpServer.IPv4Address ):
                Logger.Writer.LogInformation( $"{e.PropertyName} set to {_server?.IPv4Address}" );
                break;
            case nameof( OncRpcTcpServer.Running ):
                Logger.Writer.LogInformation( $"{e.PropertyName} set to {_server?.Running}" );
                break;
        }
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
        Logger.Writer.LogInformation( "    okay" );
    }

    /// <summary>   Assert client should close. </summary>
    /// <param name="client">   The client. </param>
    private static void AssertClientShouldClose( OncRpcTcpTestClient client )
    {
        Logger.Writer.LogInformation( "Closing... " );
        client.Close();
        Assert.IsFalse( client.Connected, "should be disconnected" );
        Logger.Writer.LogInformation( "    okay" );
    }

    /// <summary>   (Unit Test Method) client should connect. </summary>
    [TestMethod]
    public void ClientShouldConnect()
    {
        using OncRpcTcpTestClient client = new();
        try
        {
            AssertClientShouldConnect( client, IPAddress.Loopback, RpcProgramConstants.Version1 );
        }
        catch { throw; }
        finally
        {
            AssertClientShouldClose( client );
        }
    }

    /// <summary>   (Unit Test Method) client should connect version 2. </summary>
    [TestMethod]
    public void ClientShouldConnectVersion2()
    {
        using OncRpcTcpTestClient client = new();
        try
        {
            AssertClientShouldConnect( client, IPAddress.Loopback, RpcProgramConstants.Version2 );
        }
        catch { throw; }
        finally
        {
            AssertClientShouldClose( client );
        }
    }

    /// <summary>   Assert client should ping. </summary>
    /// <param name="client">   The client. </param>
    private static void AssertClientShouldPing( OncRpcTcpTestClient client )
    {
        Logger.Writer.LogInformation( "About to ping: " );
        client.CallRemoteProcedureNull();
        Logger.Writer.LogInformation( "    okay" );
    }

    /// <summary>   Assert client should ping. </summary>
    [TestMethod]
    public void ClientShouldPing()
    {
        using OncRpcTcpTestClient client = new();
        try
        {
            AssertClientShouldConnect( client, IPAddress.Loopback, RpcProgramConstants.Version1 );
            AssertClientShouldPing( client );
        }
        catch { throw; }
        finally
        {
            AssertClientShouldClose( client );
        }
    }

    /// <summary>   (Unit Test Method) client should ping version 2. </summary>
    [TestMethod]
    public void ClientShouldPingVersion2()
    {
        using OncRpcTcpTestClient client = new();
        try
        {
            AssertClientShouldConnect( client, IPAddress.Loopback, RpcProgramConstants.Version2 );
            AssertClientShouldPing( client );
        }
        catch { throw; }
        finally
        {
            AssertClientShouldClose( client );
        }
    }

    /// <summary>   Assert client should fail authentication. </summary>
    /// <param name="client">   The client. </param>
    private static void AssertClientShouldFailAuthentication( OncRpcTcpTestClient client )
    {
        int userId = 0;
        int groupId = 0;
        string machineName = AuthenticationConstants.MachineName;
        System.Console.Out.Write( $"checking  {nameof( OncRpcAuthType.OncRpcAuthTypeUnix )} on machine: {machineName} with invalid credentials user id: {userId} & Group ID = {groupId}: " );
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
            Logger.Writer.LogInformation( "    okay" );
        }
        catch ( OncRpcException e )
        {
            Assert.Fail( $"received {nameof( OncRpcException )}: {e}" );
        }
        catch ( Exception ex )
        {
            Assert.Fail( $"received exception: {ex}" );
        }
    }

    /// <summary>   (Unit Test Method) client should fail authentication. </summary>
    [TestMethod]
    public void ClientShouldFailAuthentication()
    {
        using OncRpcTcpTestClient client = new();
        try
        {
            AssertClientShouldConnect( client, IPAddress.Loopback, RpcProgramConstants.Version1 );
            AssertClientShouldFailAuthentication( client );
        }
        catch { throw; }
        finally
        {
            AssertClientShouldClose( client );
        }
    }

    /// <summary>   Assert client should authenticate. </summary>
    /// <param name="client">   The client. </param>
    private static void AssertClientShouldAuthenticate( OncRpcTcpTestClient client )
    {
        int userId = AuthenticationConstants.UserIdentity;
        int groupId = AuthenticationConstants.GroupIdentity;
        string machineName = AuthenticationConstants.MachineName;
        System.Console.Out.Write( $"checking {nameof( OncRpcAuthType.OncRpcAuthTypeUnix )} on machine: {machineName} with valid credentials user id: {userId} & Group ID = {groupId}: " );
        try
        {
            client.CallAuthenticate( AuthenticationConstants.MachineName, userId, groupId );
            Logger.Writer.LogInformation( "    okay" );
        }
        catch ( OncRpcAuthException ae )
        {
            Assert.Fail( $"received {nameof( OncRpcAuthException )} with a status of {ae.AuthStatus}: {ae}" );
        }
    }

    /// <summary>   (Unit Test Method) client should authenticate. </summary>
    [TestMethod]
    public void ClientShouldAuthenticate()
    {
        // this seems to be required to allow the sequence to work.
        this.ClientShouldConnect();

        using OncRpcTcpTestClient client = new();
        try
        {
            AssertClientShouldConnect( client, IPAddress.Loopback, RpcProgramConstants.Version1 );
            AssertClientShouldAuthenticate( client );
        }
        catch { throw; }
        finally
        {
            AssertClientShouldClose( client );
        }
    }

    /// <summary>   Assert client should echo messages. </summary>
    /// <param name="client">   The client. </param>
    /// <param name="messages"> The messages. </param>
    private static void AssertClientShouldEchoMessages( OncRpcTcpTestClient client, string[] messages )
    {
        foreach ( string message in messages )
        {
            System.Console.Out.Write( $"checking echo of {message}: " );
            string echoed = client.CallRemoteProcedureEcho( message );
            Assert.AreEqual( message, echoed, $"answer '{echoed}' does not match '{message}' call" );
            Logger.Writer.LogInformation( $"    Okay: echoed {echoed}" );
        }
    }

    /// <summary>   (Unit Test Method) client should echo messages. </summary>
    [TestMethod]
    public void ClientShouldEchoMessages()
    {
        using OncRpcTcpTestClient client = new();
        try
        {
            AssertClientShouldConnect( client, IPAddress.Loopback, RpcProgramConstants.Version1 );
            AssertClientShouldAuthenticate( client );
            string[] messages = new string[] { "UNIX", "AUTH", "is like", "*NO* authentication", "--", "it", "uses", "*NO CRYPTOGRAPHY*", "for securing", "ONC/RPC messages" };
            AssertClientShouldEchoMessages( client, messages );
        }
        catch { throw; }
        finally
        {
            AssertClientShouldClose( client );
        }
    }

    /// <summary>   Assert client should echo. </summary>
    /// <param name="client">   The client. </param>
    private static void AssertClientShouldEcho( OncRpcTcpTestClient client )
    {
        Logger.Writer.LogInformation( "About to echo: " );
        string expected = "Hello, Remote Tea!";
        string actual = client.CallRemoteProcedureEcho( expected );
        Assert.AreEqual( expected, actual );
        Logger.Writer.LogInformation( $"    Okay. Echo: '{actual}'" );
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
        Logger.Writer.LogInformation( $"    Okay. Echo: '{actual}'" );
    }

    /// <summary>   Assert client should concatenate exactly. </summary>
    /// <param name="client">   The client. </param>
    private static void AssertClientShouldConcatenateExactly( OncRpcTcpTestClient client )
    {
        Logger.Writer.LogInformation( "About to concatenating exactly three strings: " );
        string expected = "(arg1:Hello )(arg2:Remote )(arg3:Tea!)";
        string actual = client.CallRemoteProcedureConcatenatedThreeItems( "(arg1:Hello )", "(arg2:Remote )", "(arg3:Tea!)" );
        Assert.AreEqual( expected, actual );
        Logger.Writer.LogInformation( $"    Okay. Echo: '{actual}'" );
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
        Logger.Writer.LogInformation( "About to get a foo: " );
        Assert.AreEqual( client.CallRemoteProcedureReturnEnumFooValue(), ( int ) EnumFoo.FOO, $"oops: got a {EnumFoo.BAR} instead of a {EnumFoo.FOO}!" );
    }

    /// <summary>   Assert client should get numbered foo. </summary>
    /// <param name="client">   The client. </param>
    private static void AssertClientShouldGetNumberedFoo( OncRpcTcpTestClient client )
    {
        Logger.Writer.LogInformation( "About to get a numbered foo string: " );
        EnumFoo expectedValue = EnumFoo.FOO;
        string expected = OncRpcTcpServer.ReturnYouAreFooValue( ( int ) expectedValue );
        string echo = client.CallRemoteProcedureReturnYouAreFooValue( expectedValue );
        Assert.AreEqual( expected, echo, $"oops: should echo '{expected}')" );
    }

    /// <summary>   Assert client should prepend linked list. </summary>
    /// <param name="client">   The client. </param>
    private static void AssertClientShouldPrependLinkedList( OncRpcTcpTestClient client )
    {
        Logger.Writer.LogInformation( "Linked List test: " );
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
        Logger.Writer.LogInformation( "Linking Linked Lists test: " );
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
    [TestMethod]
    public void ClientShouldCallRemoteProcedures()
    {
        using OncRpcTcpTestClient client = new();
        try
        {
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
            AssertClientShouldClose( client );
            Logger.Writer.LogInformation( "All tests passed." );
        }
        catch { throw; }
        finally
        {
            AssertClientShouldClose( client );
        }

    }

}
