using System.ComponentModel;

using cc.isr.ONC.RPC.MSTest.Codecs;

namespace cc.isr.ONC.RPC.MSTest.Tcp;

[TestClass]
public class OncRpcTcpTests
{

    /// <summary>   Initializes the fixture. </summary>
    /// <param name="context">  The context. </param>
    [ClassInitialize]
    public static void InitializeFixture( TestContext context )
    {
        try
        {
            System.Diagnostics.Debug.WriteLine( $"{context.FullyQualifiedTestClassName}.{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name} Tester" );
            _classTestContext = context;
            System.Diagnostics.Debug.WriteLine( $"{_classTestContext.FullyQualifiedTestClassName}.{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name} Tester" );
            _server = new() {
                Listening = false
            };
            _server.PropertyChanged += OnServerPropertyChanged;
            _ = Task.Factory.StartNew( () => {
                Console.WriteLine( "starting the server task; this takes ~6 seconds..." );
                _server.Run();
            } );

            Console.WriteLine( $"{nameof( OncRpcTcpServer )} waiting listening {DateTime.Now:ss.fff}" );
            // wait till the server is running.
            do
            {
                System.Threading.Thread.Sleep( 500 );
            }
            while ( !_server.Listening );
            Console.WriteLine( $"{nameof( OncRpcTcpServer )} is {(_server.Listening ? "running" : "idle")}  {DateTime.Now:ss.fff}" );
        }
        catch ( Exception ex )
        {
            Console.WriteLine( $"Failed initializing fixture: \n{ex} " );
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
            if ( _server.Listening )
            {
                _server.StopRpcProcessing();
            }
            _server = null;
        }
    }

    private static OncRpcTcpServer? _server;

    private static void OnServerPropertyChanged( object? sender, PropertyChangedEventArgs args )
    {
        switch ( args.PropertyName )
        {
            case nameof( OncRpcTcpServer.ReadMessage ):
                Console.WriteLine( _server?.ReadMessage );
                break;
            case nameof( OncRpcTcpServer.WriteMessage ):
                Console.WriteLine( _server?.WriteMessage );
                break;
            case nameof( OncRpcTcpServer.PortNumber ):
                Console.WriteLine( $"{args.PropertyName} set to {_server?.PortNumber}" );
                break;
            case nameof( OncRpcTcpServer.IPv4Address ):
                Console.WriteLine( $"{args.PropertyName} set to {_server?.IPv4Address}" );
                break;
            case nameof( OncRpcTcpServer.Listening ):
                Console.WriteLine( $"{args.PropertyName} set to {_server?.Listening}" );
                break;
        }
    }

    /// <summary>   (Unit Test Method) server should be listening. </summary>
    [TestMethod]
    public void ServerShouldBeListening()
    {
        Assert.IsTrue( _server?.Listening );
    }

    /// <summary>   Assert client should connect. </summary>
    /// <param name="client">   The client. </param>
    /// <param name="host">     The host. </param>
    /// <param name="version">  The version. </param>
    private static void AssertClientShouldConnect( OncRpcTcpTestClient client, IPAddress host, int version )
    {
        Console.Write( "Connecting... " );
        client.Connect( host, version );
        Console.WriteLine( "okay" );
    }

    /// <summary>   Assert client should close. </summary>
    /// <param name="client">   The client. </param>
    private static void AssertClientShouldClose( OncRpcTcpTestClient client )
    {
        Console.Write( "Closing... " );
        client.Close();
        Console.WriteLine( "okay" );
    }

    /// <summary>   (Unit Test Method) client should connect. </summary>
    [TestMethod]
    public void ClientShouldConnect()
    {
        OncRpcTcpTestClient client = new();
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
        OncRpcTcpTestClient client = new();
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
        Console.Write( "About to ping: " );
        client.CallRemoteProcedureNull();
        Console.WriteLine( "okay" );
    }

    /// <summary>   Assert client should ping. </summary>
    [TestMethod]
    public void ClientShouldPing()
    {
        OncRpcTcpTestClient client = new();
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
        OncRpcTcpTestClient client = new();
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
            Console.WriteLine( "okay" );
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
        OncRpcTcpTestClient client = new();
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
            Console.WriteLine( "okay" );
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

        OncRpcTcpTestClient client = new();
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
            System.Console.WriteLine( $"Okay: echoed {echoed}" );
        }
    }

    /// <summary>   (Unit Test Method) client should echo messages. </summary>
    [TestMethod]
    public void ClientShouldEchoMessages()
    {
        OncRpcTcpTestClient client = new();
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
        Console.Write( "About to echo: " );
        string expected = "Hello, Remote Tea!";
        string actual = client.CallRemoteProcedureEcho( expected );
        Assert.AreEqual( expected, actual );
        Console.WriteLine( $"Okay. Echo: '{actual}'" );
    }

    /// <summary>   Assert client should concatenate. </summary>
    /// <param name="client">   The client. </param>
    private static void AssertClientShouldConcatenate( OncRpcTcpTestClient client )
    {
        Console.Write( "About to concatenate: " );
        StringVectorCodec strings = new();
        strings.SetValues( new StringCodec[] { new StringCodec( "Hello, " ), new StringCodec( "Remote " ), new StringCodec( "Tea!" ) } );
        string expected = "Hello, Remote Tea!";
        string actual = client.CallRemoteProcedureConcatenateInputParameters( strings );
        Assert.AreEqual( expected, actual );
        Console.WriteLine( $"Okay. Echo: '{actual}'" );
    }

    /// <summary>   Assert client should concatenate exactly. </summary>
    /// <param name="client">   The client. </param>
    private static void AssertClientShouldConcatenateExactly( OncRpcTcpTestClient client )
    {
        Console.Write( "About to concatenating exactly three strings: " );
        string expected = "(arg1:Hello )(arg2:Remote )(arg3:Tea!)";
        string actual = client.CallRemoteProcedureConcatenatedThreeItems( "(arg1:Hello )", "(arg2:Remote )", "(arg3:Tea!)" );
        Assert.AreEqual( expected, actual );
        Console.WriteLine( $"Okay. Echo: '{actual}'" );
    }

    /// <summary>   Assert client should check for foo. </summary>
    /// <param name="client">   The client. </param>
    private static void AssertClientShouldCheckForFoo( OncRpcTcpTestClient client )
    {
        Console.Write( "About to check for foo: " );
        if ( client.CallRemoteProcedureCompareInputToFoo( ( int ) EnumFoo.BAR ) )
        {
            Console.WriteLine( "oops: but a bar is not a foo!" );
            return;
        }
        Console.Write( "not bar: " );
        if ( !client.CallRemoteProcedureCompareInputToFoo( ( int ) EnumFoo.FOO ) )
        {
            Console.WriteLine( "oops: a foo should be a foo!" );
            return;
        }
        Console.WriteLine( "but a foo. ok." );
    }

    /// <summary>   Assert client should get foo. </summary>
    /// <param name="client">   The client. </param>
    private static void AssertClientShouldGetFoo( OncRpcTcpTestClient client )
    {
        Console.Write( "About to get a foo: " );
        if ( client.CallRemoteProcedureReturnEnumFooValue() != ( int ) EnumFoo.FOO )
        {
            Console.WriteLine( "oops: got a bar instead of a foo!" );
            return;
        }
        Console.WriteLine( "Okay." );
    }

    /// <summary>   Assert client should get numbered foo. </summary>
    /// <param name="client">   The client. </param>
    private static void AssertClientShouldGetNumberedFoo( OncRpcTcpTestClient client )
    {
        Console.Write( "About to get a numbered foo string: " );
        string echo = client.CallRemoteProcedureReturnYouAreFooValue( 42 );
        Console.WriteLine( $"Okay. Echo: '{echo}'" );
    }

    /// <summary>   Assert client should echo linked list. </summary>
    /// <param name="client">   The client. </param>
    private static void AssertClientShouldEchoLinkedList( OncRpcTcpTestClient client )
    {
        Console.Write( "Linked List test: " );
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
        LinkedListCodec? list = client.CallRemoteProcedureBuildLinkedList( node1 );
        Console.Write( "Okay. Echo: " );
        while ( list is not null )
        {
            Console.Write( list.Foo + ", " );
            list = list.Next;
        }
        Console.WriteLine();
    }

    /// <summary>   Assert client should link linked list. </summary>
    /// <param name="client">   The client. </param>
    private static void AssertClientShouldLinkLinkedList( OncRpcTcpTestClient client )
    {
        Console.Write( "Linking Linked Lists test: " );
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
        Console.Write( "Okay. Echo: " );
        while ( list is not null )
        {
            Console.Write( list.Foo + ", " );
            list = list.Next;
        }
        Console.WriteLine();
    }

    /// <summary>   (Unit Test Method) client should call remote procedures. </summary>
    /// <remarks>   2023-01-12. </remarks>
    [TestMethod]
    public void ClientShouldCallRemoteProcedures()
    {
        OncRpcTcpTestClient client = new();
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
            AssertClientShouldEchoLinkedList( client );
            AssertClientShouldLinkLinkedList( client );
            AssertClientShouldClose( client );
            Console.WriteLine( "All tests passed." );
        }
        catch { throw; }
        finally
        {
            AssertClientShouldClose( client );
        }

    }

}
