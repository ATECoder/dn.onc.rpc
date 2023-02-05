using cc.isr.ONC.RPC.Logging;
using cc.isr.ONC.RPC.Client;
using cc.isr.ONC.RPC.Portmap;
using System.Net.Sockets;

namespace cc.isr.ONC.RPC.MSTest.Udp;

[TestClass]
[TestCategory( "broadcast" )]
public class RemoteHostBroadcastTest
{

    /// <summary>   Initializes the fixture. </summary>
    /// <param name="context">  The context. </param>
    [ClassInitialize]
    public static void InitializeFixture( TestContext context )
    {
        try
        {
            _classTestContext = context;
            Logger.Writer.LogInformation( $"{_classTestContext.FullyQualifiedTestClassName}.{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}" );
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
        _classTestContext = null;
    }

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
            Logger.Writer.LogError( $"Exception receiving reply from {e.RemoteEndPoint}:", e.Exception );
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

        Logger.Writer.LogInformation( $"pinging port mappers in subnet {address}: " );
        try
        {
            client.BroadcastCall( ( int ) OncRpcPortmapServiceProcedure.OncRpcPortmapPing,
                                  VoidXdrCodec.VoidXdrCodecInstance, VoidXdrCodec.VoidXdrCodecInstance, timeout );
        }
        catch ( OncRpcException e )
        {
            Logger.Writer.LogMemberError( $"method call failed unexpectedly:", e );
        }
        Logger.Writer.LogInformation( "done." );

        // Print addresses of all port mappers found...

        foreach ( IPEndPoint endPoint in _portmappers )
        {
            Logger.Writer.LogInformation( $"Found: {endPoint}" );
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
    ///   at cc.isr.ONC.RPC.Client.OncRpcClientReplyMessage.Decode( XdrDecodingStreamBase decoder ) in C:\my\lib\vs\iot\oncrpc\src\rpc\oncrpc\ONC.RPC\Client\OncRpcClientReplyMessage.cs:line 71
    ///   at cc.isr.ONC.RPC.Client.OncRpcUdpClient.BroadcastCall( Int32 procedureNumber, IXdrCodec requestCodec, IXdrCodec replyCodec, Int32 timeout ) in C:\my\lib\vs\iot\oncrpc\src\rpc\oncrpc\ONC.RPC\Client\OncRpcUdpClient.cs:line 599
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
            Logger.Writer.LogInformation( $"{nameof(ClientShouldBroadcast)} at {ip}" ) ;
            RemoteHostBroadcastTest.AssertClientShouldBroadcast( ip, 2001 );
        }
    }

}
