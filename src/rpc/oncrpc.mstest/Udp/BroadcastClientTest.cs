
using System.ComponentModel;

using cc.isr.ONC.RPC.Client;
using cc.isr.ONC.RPC.MSTest.Tcp;
using cc.isr.ONC.RPC.Portmap;

namespace cc.isr.ONC.RPC.MSTest.Udp;

[TestClass]
[TestCategory("broadcast")]
public class BroadcastClientTest
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
            Console.WriteLine( "Failed initializing fixture: " );
            Console.WriteLine( ex.ToString() );
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
        _server = null;
    }

    private static OncRpcUdpServer? _server;

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

    private readonly List<IPEndPoint> _portmappers = new();

    /// <summary>   (Unit Test Method) server should listen. </summary>
    [TestMethod]
    public void ServerShouldListen()
    {
        Assert.IsTrue( _server?.Listening );
    }

    /// <summary>
    /// List of addresses of port mappers that replied to our call...
    /// 
    /// Remember addresses of replies for later processing. Please note that you should not do any
    /// lengthy things (like DNS name lookups)
    /// in this event handler, as you will otherwise miss some incoming replies because the OS will
    /// drop them.
    /// </summary>
    /// <param name="evt">  The event. </param>
	public virtual void ReplyReceived( object? sender, OncRpcBroadcastEventArgs evt )
    {
        this._portmappers.Add( evt.RemoteEndPoint );
        Console.Out.Write( "." );
    }

    /// <summary>   (Unit Test Method) client should broadcast. </summary>
    [TestMethod]
    public void ClientShouldBroadcast()
    {

        Assert.IsTrue( _server?.Listening );

        // Create a portmap client object, which can then be used to contact
        // the local ONC/RPC 'OncRpcUdpServer' test server.
        // OncRpcUdpClient client = new( IPAddress.Parse( "255.255.255.255" ), 100000, 2, 111 );
        using OncRpcUdpClient client = new( IPAddress.Loopback,
                                                         OncRpcPortmapConstants.OncRpcPortmapProgramNumber,
                                                         OncRpcPortmapConstants.OncRpcPortmapProgramVersionNumber,
                                                         OncRpcPortmapConstants.OncRpcPortmapPortNumber,
                                                         0 , OncRpcUdpClient.TransmitTimeoutDefault );

        // subscribe the reply received method to the broadcast reply received event.
        client.BroadcastReplyReceived += this.ReplyReceived;

        client.IOTimeout = OncRpcUdpClient.IOTimeoutDefault;
        // Ping all port mappers in this subnet...

        int timeout = 5000;
        Console.Out.Write( "pinging port mappers in subnet: " );
        try
        {
            client.BroadcastCall( ( int ) OncRpcPortmapServiceProcedure.OncRpcPortmapPing,
                                  VoidXdrCodec.VoidXdrCodecInstance, VoidXdrCodec.VoidXdrCodecInstance, timeout);
        }
        catch ( OncRpcException e )
        {
            Console.WriteLine( $"method call failed unexpectedly: \n{e}" );
        }
        Console.WriteLine( "done." );

        // Print addresses of all port mappers found...

        for ( int idx = 0; idx < this._portmappers.Count; ++idx )
            Console.WriteLine( $"Found: {this._portmappers[idx]!}" );

        // Release resources bound by portmap client object as soon as possible.

        client.Close();
    }

}
