using System.Diagnostics;

using cc.isr.ONC.RPC.Portmap;

namespace cc.isr.ONC.RPC.MSTest.PortMapper;

/// <summary>   (Unit Test Class) an embedded portmap test. </summary>
[TestClass]
public class EmbeddedPortmapTest
{

    #region " construction and cleanup "

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
    { }

    private IDisposable? _loggerScope;

    private LoggerTraceListener<EmbeddedPortmapTest>? _traceListener;

    /// <summary> Initializes the test class instance before each test runs. </summary>
    [TestInitialize()]
    public void InitializeBeforeEachTest()
    {
        if ( Logger is not null )
        {
            this._loggerScope = Logger.BeginScope( this.TestContext?.TestName ?? string.Empty );
            this._traceListener = new LoggerTraceListener<EmbeddedPortmapTest>( Logger );
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
    public static ILogger<EmbeddedPortmapTest>? Logger { get; } = LoggerProvider.InitLogger<EmbeddedPortmapTest>();

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

    /// <summary>   (Unit Test Method) embedded Portmap service should pass. </summary>
    /// <remarks>
    /// <code>
    /// Standard Output: 
    /// 2023-02-04 19:34:46.368,Starting the embedded Portmap service
    /// 2023-02-04 19:34:46.371,Checking for Portmap service
    /// 2023-02-04 19:34:46.401, No Portmap service available.
    /// 2023-02-04 19:34:46.401,Creating embedded Portmap instance
    /// 2023-02-04 19:34:46.613, Portmap service started; checked 29.9 ms.
    /// 2023-02-04 19:34:46.613,The embedded Portmap service started in 242.3 ms
    /// 2023-02-04 19:34:46.613,Deregistering non-existing program;
    /// 2023-02-04 19:34:46.618,deregistering a non-existing program was ignored.
    /// 2023-02-04 19:34:46.618, Registering dummy program;
    /// 2023-02-04 19:34:46.618,Registering a dummy program worked.
    /// 2023-02-04 19:34:46.618,Deregistering dummy program;
    /// 2023-02-04 19:34:46.619,Deregistering the registered dummy program worked.
    /// 2023-02-04 19:34:46.619, Exiting test method; OncRpcEmbeddedPortmapServiceStub will be disposed...
    /// </code>
    /// </remarks>
    [TestMethod]
    public void EmbeddedPortmapServiceShouldPass()
    {

        Logger?.LogInformation( "Starting the embedded Portmap service" );

        Stopwatch stopwatch = Stopwatch.StartNew();
        using OncRpcEmbeddedPortmapServiceStub epm = OncRpcEmbeddedPortmapServiceStub.StartEmbeddedPortmapService(); // AssertPortmapServiceShouldStart();

        // It is assumed that no external Portmap services are available.
        Assert.IsFalse( epm.UsingExternalPortmapService, $"External Portmap services are not expected." );

        Logger?.LogInformation( $"The embedded Portmap service started in {stopwatch.Elapsed.TotalMilliseconds:0.0} ms" );

        // Now register dummy ONC/RPC program. Note that the embedded
        // Portmap service must not automatically spin down when deregistering
        // the non-existing dummy program.

        int dummyProgram = 12345678;
        int dummyVersion = 42;
        int dummyPort = 42;

        using OncRpcPortmapClient portMapClient = new( IPAddress.Loopback, OncRpcProtocol.OncRpcUdp,
                                                                       Client.OncRpcUdpClient.TransmitTimeoutDefault );

        Logger?.LogInformation( "Deregistering non-existing program;" );

        bool actual = portMapClient.UnsetPort( dummyProgram, dummyVersion );
        Assert.IsFalse( actual );
        Logger?.LogInformation( "deregistering a non-existing program was ignored." );

        Logger?.LogInformation( "Registering dummy program;" );
        actual = portMapClient.SetPort( dummyProgram, dummyVersion, OncRpcProtocol.OncRpcTcp, dummyPort );
        Assert.IsTrue( actual );
        Logger?.LogInformation( "Registering a dummy program worked." );

        Logger?.LogInformation( "Deregistering dummy program;" );
        actual = portMapClient.UnsetPort( dummyProgram, dummyVersion );
        Assert.IsTrue( actual );
        Logger?.LogInformation( "Deregistering the registered dummy program worked." );

        // dispose of the Portmap service
        Logger?.LogInformation( $"Exiting test method; {nameof( OncRpcEmbeddedPortmapServiceStub )} will be disposed..." );

    }
}
