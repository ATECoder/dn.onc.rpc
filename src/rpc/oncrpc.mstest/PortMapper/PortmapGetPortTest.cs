using System.Diagnostics;
using System.Net.Sockets;

using cc.isr.ONC.RPC.Codecs;
using cc.isr.ONC.RPC.Portmap;

namespace cc.isr.ONC.RPC.MSTest.PortMapper;

/// <summary>   (Unit Test Class) a portmap get port test. </summary>
[TestClass]
public class APortmapGetPortTest
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

    private LoggerTraceListener<APortmapGetPortTest>? _traceListener;

    /// <summary> Initializes the test class instance before each test runs. </summary>
    [TestInitialize()]
    public void InitializeBeforeEachTest()
    {
        if ( Logger is not null )
        {
            this._loggerScope = Logger.BeginScope( this.TestContext?.TestName ?? string.Empty );
            this._traceListener = new LoggerTraceListener<APortmapGetPortTest>( Logger );
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
    public static ILogger<APortmapGetPortTest>? Logger { get; } = LoggerProvider.InitLogger<APortmapGetPortTest>();

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
        Trace.TraceInformation( "Testing tracing an info message" ); Trace.Flush();
        Assert.IsTrue( this._traceListener?.Any( TraceEventType.Information ), $"{nameof( this._traceListener )} should have {TraceEventType.Error} messages" );

        // no need to report errors for this test.

        this._traceListener?.Clear();
    }

    #endregion

    #region " port map get port tests "

    /// <summary>   (Unit Test Method) portmap should get port. </summary>
    /// <remarks> THIS TEST OFTEN FAILED WHEN RUN AFTER THE EMBEDDED PORTMAP TEST. 
    /// so we changed the order for now. 
    /// <code>
    /// Standard Output: 
    /// 2023-02-04 19:34:06.719,Checking for Portmap service
    /// 2023-02-04 19:34:06.745, No Portmap service available.
    /// 2023-02-04 19:34:06.745,Creating embedded Portmap instance
    /// 2023-02-04 19:34:06.957, Portmap service started; checked 23.3 ms.
    /// 2023-02-04 19:34:06.958,Host: 192.168.4.28
    /// 2023-02-04 19:34:06.958,pinging port mapper;
    /// 2023-02-04 19:34:06.962,port mapper pinged.
    /// 2023-02-04 19:34:06.962,GetPort for non-existing program
    /// 2023-02-04 19:34:06.963, succeeded; received error code( OncRpcProgramNotRegistered(15).
    /// 2023-02-04 19:34:06.963,SetPort dummy server identification:
    /// 2023-02-04 19:34:06.964, SetPort succeeded.
    /// 2023-02-04 19:34:06.964,executing ListRegisteredServers
    /// 2023-02-04 19:34:06.965, ListRegisteredServers succeeded.
    /// 2023-02-04 19:34:06.965,listing Registered servers
    /// 2023-02-04 19:34:06.965, Program Version Protocol Port
    /// 2023-02-04 19:34:06.965,100000 2 OncRpcTcp 111
    /// 2023-02-04 19:34:06.965,100000 2 OncRpcUdp 111
    /// 2023-02-04 19:34:06.965,1 42 OncRpcUdp 65535
    /// 2023-02-04 19:34:06.965,executing UnsetPort dummy server identification:
    /// 2023-02-04 19:34:06.965,UnsetPort succeeded.
    /// 2023-02-04 19:34:06.965, executing ListRegisteredServers
    /// 2023-02-04 19:34:06.966, ListRegisteredServersSucceeded.
    /// 2023-02-04 19:34:06.966, Exiting test method; OncRpcEmbeddedPortmapServiceStub will be disposed...
    /// </code>
    /// </remarks>
    [TestMethod]
    public void PortmapShouldGetPort()
    {
        using OncRpcEmbeddedPortmapServiceStub epm = OncRpcEmbeddedPortmapServiceStub.StartEmbeddedPortmapService(); // AssertPortmapServiceShouldStart();

        // It is assumed that no external Portmap services are available.
        Assert.IsFalse( epm.UsingExternalPortmapService, $"External Portmap services are not expected." );

        IPHostEntry host = Dns.GetHostEntry( Dns.GetHostName() );

        var ipAddress = host
            .AddressList
            .FirstOrDefault( ip => ip.AddressFamily == AddressFamily.InterNetwork );
        Logger?.LogInformation( $"Host: {ipAddress}" );

        // Create a portmap client object, which can then be used to contact
        // a local or remote ONC/RPC portmap process. In this test we contact
        // the local port mapper.

        using OncRpcPortmapClient portmap = new( ipAddress!, OncRpcProtocol.OncRpcUdp, Client.OncRpcUdpClient.TransmitTimeoutDefault );

        // portmap.RetransmitMode = OncRpcUdpRetransmitMode.OncRpcFixedTimeout;
        // portmap.RetransmitTimeout = 3*1000;

        // Ping the port mapper...

        Logger?.LogInformation( "pinging port mapper;" );
        portmap.PingPortmapService();
        Logger?.LogInformation( "port mapper pinged." );

        // Ask for a non-existent ONC/RPC server.

        int port;
        Logger?.LogInformation( $"{nameof( OncRpcPortmapClient.GetPort )} for non-existing program" );
        try
        {
            port = portmap.GetPort( 1, 1, OncRpcProtocol.OncRpcUdp );
            Assert.Fail( "method call failed (a non-existing program was found)." );
        }
        catch ( OncRpcException e )
        {
            if ( e.Reason != OncRpcExceptionReason.OncRpcProgramNotRegistered )
            {
                Assert.Fail( $"method call failed unexpectedly: {e}" );
            }
            Logger?.LogInformation( $"succeeded; received error code ({OncRpcExceptionReason.OncRpcProgramNotRegistered}({( int ) OncRpcExceptionReason.OncRpcProgramNotRegistered})." );
        }

        // Register dummy ONC/RPC server.

        Logger?.LogInformation( $"{nameof( OncRpcPortmapClient.SetPort )} dummy server identification: " );
        try
        {
            _ = portmap.SetPort( 1, 42, OncRpcProtocol.OncRpcUdp, 65535 );
        }
        catch ( OncRpcException e )
        {
            Assert.Fail( $"method call failed unexpectedly: {e}" );
        }
        Logger?.LogInformation( $"{nameof( OncRpcPortmapClient.SetPort )} succeeded." );

        // Now dump the current list of registered servers.

        OncRpcServerIdentifierCodec[] list = Array.Empty<OncRpcServerIdentifierCodec>();
        bool found = false;
        Logger?.LogInformation( $"executing {nameof( OncRpcPortmapClient.ListRegisteredServers )}" );
        try
        {
            list = portmap.ListRegisteredServers();
        }
        catch ( OncRpcException e )
        {
            Assert.Fail( $"method call failed unexpectedly: {e}" );
        }
        Logger?.LogInformation( $"{nameof( OncRpcPortmapClient.ListRegisteredServers )} succeeded." );

        Logger?.LogInformation( "listing Registered servers" );
        Logger?.LogInformation( $" Program Version Protocol Port" );
        foreach ( OncRpcServerIdentifierCodec value in list )
        {
            if ( value.Program == 1 && value.Version == 42 && value.Protocol == OncRpcProtocol.OncRpcUdp && value.Port == 65535 )
                found = true;
            Logger?.LogInformation( $"{value.Program} {value.Version} {value.Protocol} {value.Port}" );
        }
        Assert.IsTrue( found, "expected dummy server was not found among the registered servers." );

        // Deregister dummy ONC/RPC server.

        Logger?.LogInformation( $"executing {nameof( OncRpcPortmapClient.UnsetPort )} dummy server identification: " );
        try
        {
            _ = portmap.UnsetPort( 1, 42 );
        }
        catch ( OncRpcException e )
        {
            Assert.Fail( $"method call failed unexpectedly: {e}" );
        }
        Logger?.LogInformation( $"{nameof( OncRpcPortmapClient.UnsetPort )} succeeded." );

        // Now dump again the current list of registered servers.

        found = false;
        list = Array.Empty<OncRpcServerIdentifierCodec>();
        Logger?.LogInformation( $"executing {nameof( OncRpcPortmapClient.ListRegisteredServers )}" );
        try
        {
            list = portmap.ListRegisteredServers();
        }
        catch ( OncRpcException e )
        {
            Assert.Fail( $"method call failed unexpectedly: {e}" );
        }
        Logger?.LogInformation( $"{nameof( OncRpcPortmapClient.ListRegisteredServers )} succeeded." );

        foreach ( OncRpcServerIdentifierCodec value in list )
        {
            if ( value.Program == 1 && value.Version == 42 && value.Protocol == OncRpcProtocol.OncRpcUdp && value.Port == 65535 )
                Assert.Fail( $"registered dummy ({value.Program},{value.Version}) server still found after deregistering." );
        }

        // Release resources bound by portmap client object as soon as possible

        portmap.Close();

        // which disposes of the Portmap service

        Logger?.LogInformation( $"Exiting test method; {nameof( OncRpcEmbeddedPortmapServiceStub )} will be disposed..." );
    }

    #endregion
}
