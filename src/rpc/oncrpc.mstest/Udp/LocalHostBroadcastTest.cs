using System.ComponentModel;

using cc.isr.ONC.RPC.Logging;
using cc.isr.ONC.RPC.MSTest.Tcp;
namespace cc.isr.ONC.RPC.MSTest.Udp;

[TestClass]
[TestCategory( "broadcast" )]
public class LocalHostBroadcastTest
{

    /// <summary>   Initializes the fixture. </summary>
    /// <param name="context">  The context. </param>
    [ClassInitialize]
    public static void InitializeFixture( TestContext context )
    {
        try
        {
            Logger.Writer.LogInformation( $"{context.FullyQualifiedTestClassName}.{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name} Tester" );
            _classTestContext = context;
            _server = new() {
                Listening = false
            };
            _server.PropertyChanged += OnServerPropertyChanged;
            _ = Task.Factory.StartNew( () => {
                Logger.Writer.LogInformation( "starting the server task; this takes ~6 seconds..." );
                _server.Run();
            } );

            Logger.Writer.LogInformation( $"{nameof( OncRpcTcpServer )} waiting listening {DateTime.Now:ss.fff}" );
            // wait till the server is running.
            do
            {
                System.Threading.Thread.Sleep( 500 );
            }
            while ( !_server.Listening );
            Logger.Writer.LogInformation( $"{nameof( OncRpcTcpServer )} is {(_server.Listening ? "running" : "idle")}  {DateTime.Now:ss.fff}" );
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
        _server?.Shutdown();
        _server?.Dispose();
        _server = null;
    }

    private static OncRpcUdpServer? _server;

    private static void OnServerPropertyChanged( object? sender, PropertyChangedEventArgs args )
    {
        if ( _server is null ) return;
        switch ( args.PropertyName )
        {
            case nameof( OncRpcTcpServer.ReadMessage ):
                Logger.Writer.LogInformation( _server.ReadMessage );
                break;
            case nameof( OncRpcTcpServer.WriteMessage ):
                Logger.Writer.LogInformation( _server.WriteMessage );
                break;
            case nameof( OncRpcTcpServer.PortNumber ):
                Logger.Writer.LogInformation( $"{args.PropertyName} set to {_server?.PortNumber}" );
                break;
            case nameof( OncRpcTcpServer.IPv4Address ):
                Logger.Writer.LogInformation( $"{args.PropertyName} set to {_server?.IPv4Address}" );
                break;
            case nameof( OncRpcTcpServer.Listening ):
                Logger.Writer.LogInformation( $"{args.PropertyName} set to {_server?.Listening}" );
                break;
        }
    }

    private readonly List<IPEndPoint> _portmappers = new();

    /// <summary>   (Unit Test Method) server should listen. </summary>
    [TestMethod]
    public void ServerShouldListen()
    {
        Assert.IsTrue( _server?.Listening );
    }

    /// <summary>   (Unit Test Method) client should broadcast. </summary>
    /// <remarks>
    /// NOTE!: This test often fails after running the other tests. Then it takes a bit of time for the
    /// test to run. <para>
    ///     
    /// With a set of two network cards, setting the server to any located the server on 192.168.0.40
    /// as the local host. </para><para>
    /// Pinging the local host at 192.168.4.255 yields no result; </para><para>
    /// pinging port mappers in subnet: 127.0.0.1. done.
    /// Found: 127.0.0.1:111
    /// Listening set to False
    /// System.InvalidOperationException: Server still running after stopping RPC Processing. </para>
    /// 
    /// </remarks>
    [TestMethod]
    public void ClientShouldBroadcast()
    {

        Assert.IsTrue( _server?.Listening );

        // Create a portmap client object, which can then be used to contact
        // the local ONC/RPC 'OncRpcUdpServer' test server.

        IPAddress address = IPAddress.Loopback;
        RemoteHostBroadcastTest.AssertClientShouldBroadcast( address, 1001 );
    }

}
