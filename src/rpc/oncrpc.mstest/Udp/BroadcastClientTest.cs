
using System.ComponentModel;

using cc.isr.ONC.RPC.MSTest.Tcp;

namespace cc.isr.ONC.RPC.MSTest.Udp;

/// <summary>   (Unit Test Class) a broadcast client test. </summary>
/// <remarks>   2022-12-22. </remarks>
[TestClass]
public class BroadcastClientTest : IOncRpcBroadcastListener
{

    /// <summary>   Initializes the fixture. </summary>
    /// <remarks>   2022-12-22. </remarks>
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
            Console.WriteLine( ex.ToString() );
            CleanupFixture();
        }
    }

    /// <summary>   Gets or sets a context for the test. </summary>
    /// <value> The test context. </value>
    public TestContext? TestContext { get; set; }

    private static TestContext? _classTestContext;

    /// <summary>   Cleanup fixture. </summary>
    /// <remarks>   2022-12-22. </remarks>
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

    private readonly System.Collections.ArrayList _portmappers = new();

    /// <summary>   (Unit Test Method) server should listen. </summary>
    /// <remarks>   2022-12-24. </remarks>
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
    /// <remarks>   2022-12-20. </remarks>
    /// <param name="evt">  The event. </param>
	public virtual void ReplyReceived( OncRpcBroadcastEvent evt )
    {
        _ = this._portmappers.Add( evt.ReplyAddress );
        Console.Out.Write( "." );
    }

    /// <summary>   (Unit Test Method) client should broadcast. </summary>
    /// <remarks>   2022-12-22. </remarks>
    /// <exception cref="OncRpcException"></exception>
    /// <exception cref="IOException"></exception>
    [TestMethod]
    public void ClientShouldBroadcast()
    {
        // Create a portmap client object, which can then be used to contact
        // the local ONC/RPC 'OncRpcUdpServer' test server.
        // OncRpcUdpClient client = new( IPAddress.Parse( "255.255.255.255" ), 100000, 2, 111 );
        OncRpcUdpClient client = new( IPAddress.Loopback, OncRpcPortmapConstants.OncRpcPortmapProgramNumber,
                                                   OncRpcPortmapConstants.OncRpcPortmapProgramVersionNumber, OncRpcPortmapConstants.OncRpcPortmapPortNumber );

        // Ping all port mappers in this subnet...

        Console.Out.Write( "pinging port mappers in subnet: " );
        client.Timeout = 5 * 1000;
        try
        {
            client.BroadcastCall( 0, VoidXdrCodec.VoidXdrCodecInstance, VoidXdrCodec.VoidXdrCodecInstance, this );
        }
        catch ( OncRpcException e )
        {
            Console.WriteLine( $"method call failed unexpectedly: \n{e}" );
        }
        Console.Out.WriteLine( "done." );

        // Print addresses of all port mappers found...

        for ( int idx = 0; idx < this._portmappers.Count; ++idx )
            Console.Out.WriteLine( $"Found: {( IPAddress ) this._portmappers[idx]!}" );

        // Release resources bound by portmap client object as soon as possible.

        client.Close();
    }

}
