using System.ComponentModel;

using cc.isr.ONC.RPC.Logging;
using cc.isr.ONC.RPC.MSTest.Tcp;
using cc.isr.ONC.RPC.Portmap;
using cc.isr.ONC.RPC.Server;

namespace cc.isr.ONC.RPC.MSTest.Udp;

[TestClass]
[TestCategory( "broadcast" )]
public class LocalHostBroadcastTest
{

    /// <summary>   Gets or sets the server start time typical. </summary>
    /// <value> The server start time typical. </value>
    public static int ServerStartTimeTypical { get; set; } = 3500;

    /// <summary>   Gets or sets the server start loop delay. </summary>
    /// <value> The server start loop delay. </value>
    public static int ServerStartLoopDelay { get; set; } = 100;


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

            // _server.PropertyChanged += OnServerPropertyChanged;
            _server.ThreadExceptionOccurred += OnThreadException;

            _ = Task.Factory.StartNew( () => {
                Logger.Writer.LogInformation( "starting the embedded port map service; this takes ~3.5 seconds..." );
                using OncRpcEmbeddedPortmapServiceStub epm = OncRpcEmbeddedPortmapServiceStub.StartEmbeddedPortmapService();
                epm.EmbeddedPortmapService!.ThreadExceptionOccurred += OnThreadException;

                Logger.Writer.LogInformation( "starting the server task; this takes ~2.5 seconds..." );
                _server.Run();
            } );

            Logger.Writer.LogInformation( $"{nameof( OncRpcTcpServer )} waiting listening {DateTime.Now:ss.fff}" );

            // because the initializing task is not awaited, we need to wait for the server to start here.

            if ( !_server.ServerStarted( 2 * LocalHostBroadcastTest.ServerStartTimeTypical, LocalHostBroadcastTest.ServerStartLoopDelay ) )
                throw new InvalidOperationException( "failed starting the ONC/RPC server." );

            Logger.Writer.LogInformation( $"{nameof( OncRpcTcpServer )} is {(_server.Running ? "running" : "idle")}  {DateTime.Now:ss.fff}" );
        }
        catch ( Exception ex )
        {
            Logger.Writer.LogMemberError( "Failed initializing fixture:", ex );
            CleanupFixture();
        }
    }

    /// <summary>   Gets or sets a context for the test. </summary>
    /// <value> The test context. </value>
    public TestContext? TestContext { get; set; }

    private static TestContext? _classTestContext;

    /// <summary>   Cleanup fixture. </summary>
    [ClassCleanup]
    public static void CleanupFixture()
    {
        OncRpcUdpServer? server = _server;
        if ( server is not null )
        {
            try
            {
                server.Dispose();
                server.PropertyChanged -= OnServerPropertyChanged;
                server.ThreadExceptionOccurred -= OnThreadException;
            }
            catch ( Exception ex )
            {
                Logger.Writer.LogError( "Exception cleaning up fixture", ex );
            }
            finally
            {
                _server = null;
                _classTestContext = null;
            }
        }
    }

    private static OncRpcUdpServer? _server;

    private static void OnThreadException( object? sender, ThreadExceptionEventArgs e )
    {
        string name = "unknown";
        if ( sender is OncRpcUdpServer ) name = nameof( OncRpcUdpServer );
        if ( sender is OncRpcServerStubBase ) name = nameof( OncRpcServerStubBase );

        Logger.Writer.LogError( $"{name} encountered an exception during an asynchronous operation", e.Exception );
    }

    private static void OnServerPropertyChanged( object? sender, PropertyChangedEventArgs e )
    {
        if ( sender is not OncRpcUdpServer ) return;
        switch ( e.PropertyName )
        {
            case nameof( OncRpcTcpServer.ReadMessage ):
                Logger.Writer.LogInformation( ((OncRpcUdpServer ) sender).ReadMessage );
                break;
            case nameof( OncRpcTcpServer.WriteMessage ):
                Logger.Writer.LogInformation( (( OncRpcUdpServer ) sender).WriteMessage );
                break;
            case nameof( OncRpcTcpServer.PortNumber ):
                Logger.Writer.LogInformation( $"{e.PropertyName} set to {(( OncRpcUdpServer ) sender).PortNumber}" );
                break;
            case nameof( OncRpcTcpServer.IPv4Address ):
                Logger.Writer.LogInformation( $"{e.PropertyName} set to {(( OncRpcUdpServer ) sender).IPv4Address}" );
                break;
            case nameof( OncRpcTcpServer.Running ):
                Logger.Writer.LogInformation( $"{e.PropertyName} set to {(( OncRpcUdpServer ) sender).Running}" );
                break;
        }
    }

    private readonly List<IPEndPoint> _portmappers = new();

    /// <summary>   (Unit Test Method) server should listen. </summary>
    [TestMethod]
    public void ServerShouldListen()
    {
        Assert.IsTrue( _server?.Running );
    }

    /// <summary>   (Unit Test Method) client should broadcast. </summary>
    /// <remarks>
    /// NOTE!: This test often fails after running the other tests. Then it takes a bit of time for
    /// the test to run. <para>
    /// 
    /// With a set of two network cards, setting the server to any located the server on 192.168.0.40
    /// as the local host. </para><para>
    /// Pinging the local host at 192.168.4.255 yields no result; </para><para>
    /// pinging port mappers in subnet: 127.0.0.1. done. Found: 127.0.0.1:111 Listening set to False
    /// System.InvalidOperationException: Server still running after stopping RPC Processing. </para>
    /// 
    /// <code>
    ///    2023-01-23 21:43:22.310,cc.isr.ONC.RPC.MSTest.Udp.LocalHostBroadcastTest.LocalHostBroadcastTest
    ///    2023-01-23 21:43:22.315,starting the server task; this takes ~6 seconds...
    ///    2023-01-23 21:43:22.315,OncRpcTcpServer waiting listening 22.315
    ///    2023-01-23 21:43:22.316,Checking for portmap service
    ///    2023-01-23 21:43:25.325, No portmap service available.
    ///    2023-01-23 21:43:25.326,Creating embedded portmap instance
    ///    2023-01-23 21:43:25.538, embedded service started; try pinging port map
    ///    2023-01-23 21:43:25.543,portmap service is running; elapsed: 4ms
    ///    2023-01-23 21:43:25.547,Running set to True
    ///    2023-01-23 21:43:29.323, OncRpcTcpServer is running  29.323
    ///    2023-01-23 21:43:29.329,pinging port mappers in subnet 127.0.0.1:
    ///    .2023-01-23 21:43:30.335,    done.
    ///    2023-01-23 21:43:30.336,Found: 127.0.0.1:111
    ///    2023-01-23 21:43:30.344,Running set to False
    /// </code>
    /// </remarks>
    [TestMethod]
    public void ClientShouldBroadcast()
    {
        Assert.IsTrue( _server?.Running );

        // Create a portmap client object, which can then be used to contact
        // the local ONC/RPC 'OncRpcUdpServer' test server.

        IPAddress address = IPAddress.Loopback;
        RemoteHostBroadcastTest.AssertClientShouldBroadcast( address, 1001 );
    }

}
