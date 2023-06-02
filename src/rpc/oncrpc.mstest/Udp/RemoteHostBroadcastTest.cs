using cc.isr.ONC.RPC.Client;
using cc.isr.ONC.RPC.Portmap;
using System.Net.Sockets;
using System.Diagnostics;

namespace cc.isr.ONC.RPC.MSTest.Udp;

[TestClass]
[TestCategory( "broadcast" )]
public class RemoteHostBroadcastTest
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

    /// <summary>   Gets or sets a context for the test. </summary>
    /// <value> The test context. </value>
    /// <summary> Cleans up the test class after all tests in the class have run. </summary>
    /// <remarks> Use <see cref="CleanupTestClass"/> to run code after all tests in the class have run. </remarks>
    [ClassCleanup()]
    public static void CleanupTestClass()
    { }

    private IDisposable? _loggerScope;

    private LoggerTraceListener<RemoteHostBroadcastTest>? _traceListener;

    /// <summary> Initializes the test class instance before each test runs. </summary>
    [TestInitialize()]
    public void InitializeBeforeEachTest()
    {
        if ( Logger is not null )
        {
            this._loggerScope = Logger.BeginScope( this.TestContext?.TestName ?? string.Empty );
            this._traceListener = new LoggerTraceListener<RemoteHostBroadcastTest>( Logger );
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
    public static ILogger<RemoteHostBroadcastTest>? Logger { get; } = LoggerProvider.InitLogger<RemoteHostBroadcastTest>();

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

    #region " remote host broadcast tests "

    private static readonly List<IPEndPoint> _portmappers = new();

    /// <summary>   Gets local inter network (IPv4) addresses. </summary>
    /// <returns>   An array of IPv4 addresses. </returns>
    public static IPAddress[] GetLocalInterNetworkAddresses()
    {
        IPAddress[] localIPs = Dns.GetHostAddresses( Dns.GetHostName() );
        return localIPs.Where( ip => ip.AddressFamily == AddressFamily.InterNetwork ).ToArray();
    }

    /// <summary>   Gets local broadcast addresses. </summary>
    /// <returns>   An array of IPv4 broadcast addresses. </returns>
    public static IPAddress[] GetLocalBroadcastAddresses()
    {
        List<IPAddress> ipv4s = new();
        foreach ( IPAddress ip in GetLocalInterNetworkAddresses() )
        {
            if ( ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork )
            {
                byte[] bytes = ip.GetAddressBytes();
                bytes[3] = 255;
                ipv4s.Add( new IPAddress( bytes ) );
            }
        }
        return ipv4s.ToArray();
    }

    /// <summary>
    /// List of addresses of port mappers that replied to our call...
    /// 
    /// Remember addresses of replies for later processing. Please note that you should not do any
    /// lengthy things (like DNS name lookups)
    /// in this event handler, as you will otherwise miss some incoming replies because the OS will
    /// drop them.
    /// </summary>
    /// <param name="sender">   Source of the event. </param>
    /// <param name="e">        ONC/RPC broadcast event information. </param>
	public static void ReplyReceived( object? sender, OncRpcBroadcastEventArgs e )
    {
        _portmappers.Add( e.RemoteEndPoint );
    }

    /// <summary>   Report reply failures. </summary>
    /// <param name="sender">   Source of the event. </param>
    /// <param name="e">        ONC/RPC broadcast event information. </param>
    public static void ReplyFailed( object? sender, OncRpcBroadcastEventArgs e )
    {
        if ( e?.Exception is not null )
            Logger?.LogError( $"Exception receiving reply from {e.RemoteEndPoint}:", e.Exception );
    }

    /// <summary>   Assert client should broadcast. </summary>
    /// <param name="address">  The address. </param>
    public static void AssertClientShouldBroadcast( IPAddress address, int timeout )
    {

        // Create a portmap client object, which can then be used to contact
        // the remote ONC/RPC instruments.
        // OncRpcUdpClient client = new( IPAddress.Parse( "255.255.255.255" ), 100000, 2, 111 );
        // 
        using OncRpcUdpClient client = new( address,
                                                         OncRpcPortmapConstants.OncRpcPortmapProgramNumber,
                                                         OncRpcPortmapConstants.OncRpcPortmapProgramVersionNumber,
                                                         OncRpcPortmapConstants.OncRpcPortmapPortNumber,
                                                         0, OncRpcUdpClient.TransmitTimeoutDefault );

        // subscribe the reply received method to the broadcast reply received event.
        client.BroadcastReplyReceived += ReplyReceived;
        client.BroadcastReplyFailed += ReplyFailed;

        client.IOTimeout = OncRpcUdpClient.IOTimeoutDefault;
        // Ping all port mappers in this subnet...

        Logger?.LogInformation( $"pinging port mappers in subnet {address}: " );
        try
        {
            client.BroadcastCall( ( int ) OncRpcPortmapServiceProcedure.OncRpcPortmapPing,
                                  VoidXdrCodec.VoidXdrCodecInstance, VoidXdrCodec.VoidXdrCodecInstance, timeout );
        }
        catch ( OncRpcException e )
        {
            Logger?.LogMemberError( $"method call failed unexpectedly:", e );
        }
        Logger?.LogInformation( "done." );

        // Print addresses of all port mappers found...

        foreach ( IPEndPoint endPoint in _portmappers )
        {
            Logger?.LogInformation( $"Found: {endPoint}" );
        }
        // Release resources bound by portmap client object as soon as possible.

        client.Close();
    }

    /// <summary>   (Unit Test Method) client should broadcast. </summary>
    /// <remarks>  
    /// NOTE!: This test often fails after running the other tests. Then it takes a bit of time for the 
    /// test to run. <para>
    /// With a set of two network cards, setting the server to any located the server on 192.168.0.40
    /// as the local host. </para><para>
    /// 
    /// Pinging port mappers in subnet 192.168.0.255: . </para><para>
    /// Exception receiving reply from 192.168.0.254:111: </para><para>
    ///   cc.isr.ONC.RPC.OncRpcException: Either a ONC/RPC server or client received the wrong type of ONC/RPC message when waiting for a request or reply.; expected OncRpcReplyMessageType(1); actual: OncRpcCallMessageType(0)
    ///   at cc.isr.ONC.RPC.Client.OncRpcClientReplyMessage.Decode( XdrDecodingStreamBase decoder ) in C:\my\lib\vs\iot\ONCRPC\src\rpc\ONCRPC\ONC.RPC\Client\OncRpcClientReplyMessage.cs:line 71
    ///   at cc.isr.ONC.RPC.Client.OncRpcUdpClient.BroadcastCall( Int32 procedureNumber, IXdrCodec requestCodec, IXdrCodec replyCodec, Int32 timeout ) in C:\my\lib\vs\iot\ONCRPC\src\rpc\ONCRPC\ONC.RPC\Client\OncRpcUdpClient.cs:line 599
    /// .done. </para><para>
    /// Found: 192.168.0.154:111 </para><para>
    /// Found: 192.168.0.254:111 </para><para>
    /// 
    /// The following instruments were not found:  </para><para>
    /// Keithley 7510 at 192.168.0.144 </para><para>
    /// Keithley 2450 at 192.168.0.153 </para>
    /// 
    /// <code>
    /// Standard Output: 
    /// 2023-02-04 19:26:57.352,cc.isr.ONC.RPC.MSTest.Udp.RemoteHostBroadcastTest.RemoteHostBroadcastTest
    /// 2023-02-04 19:26:57.365,ClientShouldBroadcast at 192.168.4.255
    /// 2023-02-04 19:26:57.367, pinging port mappers in subnet 192.168.4.255:
    /// 2023-02-04 19:26:59.376,done.
    /// 2023-02-04 19:26:59.378,ClientShouldBroadcast at 192.168.0.255
    /// 2023-02-04 19:26:59.378, pinging port mappers in subnet 192.168.0.255:
    /// 2023-02-04 19:27:01.384,done.
    /// 2023-02-04 19:27:01.384,Found: 192.168.0.154:111
    /// </code>
    /// </remarks>
    [TestMethod]
    [TestCategory( "broadcast" )]
    public void ClientShouldBroadcast()
    {
        foreach ( IPAddress ip in GetLocalBroadcastAddresses() )
        {
            Logger?.LogInformation( $"{nameof( ClientShouldBroadcast )} at {ip}" );
            RemoteHostBroadcastTest.AssertClientShouldBroadcast( ip, 2001 );
        }
    }

    #endregion
}
