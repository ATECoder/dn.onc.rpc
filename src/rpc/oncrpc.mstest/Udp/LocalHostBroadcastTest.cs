using System.ComponentModel;
using System.Diagnostics;

using cc.isr.ONC.RPC.MSTest.Tcp;
using cc.isr.ONC.RPC.Portmap;
using cc.isr.ONC.RPC.Server;

namespace cc.isr.ONC.RPC.MSTest.Udp;

[TestClass]
[TestCategory( "broadcast" )]
public class LocalHostBroadcastTest
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

            string methodFullName = $"{testContext.FullyQualifiedTestClassName}.{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}";
            if ( Logger is null )
                Console.WriteLine( methodFullName );
            else
                Logger?.LogMemberInfo( methodFullName );
            _server = new();

            // _server.PropertyChanged += OnServerPropertyChanged;
            _server.ThreadExceptionOccurred += OnThreadException;

            _ = Task.Factory.StartNew( () => {
                Logger?.LogInformation( "starting the embedded port map service; this takes ~3.5 seconds..." );
                using OncRpcEmbeddedPortmapServiceStub epm = OncRpcEmbeddedPortmapServiceStub.StartEmbeddedPortmapService();
                epm.EmbeddedPortmapService!.ThreadExceptionOccurred += OnThreadException;

                Logger?.LogInformation( "starting the server task; this takes ~2.5 seconds..." );
                _server.Run();
            } );

            Logger?.LogInformation( $"{nameof( OncRpcTcpServer )} waiting listening {DateTime.Now:ss.fff}" );

            // because the initializing task is not awaited, we need to wait for the server to start here.

            if ( !_server.ServerStarted( 2 * LocalHostBroadcastTest.ServerStartTimeTypical, LocalHostBroadcastTest.ServerStartLoopDelay ) )
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

    /// <summary> Cleans up the test class after all tests in the class have run. </summary>
    /// <remarks> Use <see cref="CleanupTestClass"/> to run code after all tests in the class have run. </remarks>
    [ClassCleanup()]
    public static void CleanupTestClass()
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
                Logger?.LogError( "Exception cleaning up fixture", ex );
            }
            finally
            {
                _server = null;
            }
        }
    }

    private IDisposable? _loggerScope;

    private LoggerTraceListener<LocalHostBroadcastTest>? _traceListener;

    /// <summary> Initializes the test class instance before each test runs. </summary>
    [TestInitialize()]
    public void InitializeBeforeEachTest()
    {
        if ( Logger is not null )
        {
            this._loggerScope = Logger.BeginScope( this.TestContext?.TestName ?? string.Empty );
            this._traceListener = new LoggerTraceListener<LocalHostBroadcastTest>( Logger );
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
    public static ILogger<LocalHostBroadcastTest>? Logger { get; } = LoggerProvider.InitLogger<LocalHostBroadcastTest>();

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

    #region " codec tests "

    private static OncRpcUdpServer? _server;

    private static void OnThreadException( object? sender, ThreadExceptionEventArgs e )
    {
        string name = "unknown";
        if ( sender is OncRpcUdpServer ) name = nameof( OncRpcUdpServer );
        if ( sender is OncRpcServerStubBase ) name = nameof( OncRpcServerStubBase );

        Logger?.LogError( $"{name} encountered an exception during an asynchronous operation", e.Exception );
    }

    private static void OnServerPropertyChanged( object? sender, PropertyChangedEventArgs e )
    {
        if ( sender is not OncRpcUdpServer ) return;
        switch ( e.PropertyName )
        {
            case nameof( OncRpcTcpServer.ReadMessage ):
                Logger?.LogInformation( (( OncRpcUdpServer ) sender).ReadMessage );
                break;
            case nameof( OncRpcTcpServer.WriteMessage ):
                Logger?.LogInformation( (( OncRpcUdpServer ) sender).WriteMessage );
                break;
            case nameof( OncRpcTcpServer.PortNumber ):
                Logger?.LogInformation( $"{e.PropertyName} set to {(( OncRpcUdpServer ) sender).PortNumber}" );
                break;
            case nameof( OncRpcTcpServer.IPv4Address ):
                Logger?.LogInformation( $"{e.PropertyName} set to {(( OncRpcUdpServer ) sender).IPv4Address}" );
                break;
            case nameof( OncRpcTcpServer.Running ):
                Logger?.LogInformation( $"{e.PropertyName} set to {(( OncRpcUdpServer ) sender).Running}" );
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
    /// Standard Output: 
    ///   2023-02-04 19:25:58.694,cc.isr.ONC.RPC.MSTest.Udp.LocalHostBroadcastTest.LocalHostBroadcastTest
    ///   2023-02-04 19:25:58.699,OncRpcTcpServer waiting listening 58.699
    ///   2023-02-04 19:25:58.700,starting the embedded port map service; this takes ~3.5 seconds...
    ///   2023-02-04 19:25:58.700,Checking for Portmap service
    ///   2023-02-04 19:25:58.719, No Portmap service available.
    ///   2023-02-04 19:25:58.719,Creating embedded Portmap instance
    ///   2023-02-04 19:25:58.946, Portmap service started; checked 18.2 ms.
    ///   2023-02-04 19:25:58.946,starting the server task; this takes ~2.5 seconds...
    ///   2023-02-04 19:26:05.784,OncRpcTcpServer is running  05.784
    ///   2023-02-04 19:26:05.789,pinging port mappers in subnet 127.0.0.1:
    ///   2023-02-04 19:26:06.792,done.
    ///   2023-02-04 19:26:06.792,Found: 127.0.0.1:111
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

    #endregion

}
