using cc.isr.ONC.RPC.Logging;
using cc.isr.ONC.RPC.Client;
using cc.isr.ONC.RPC.Portmap;

namespace cc.isr.ONC.RPC.MSTest.Udp;

[TestClass]
[TestCategory( "192.168.0.255" )]
public class RemoteHostBroadcastTest
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
    }

    private static readonly List<IPEndPoint> _portmappers = new();


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
        Console.Out.Write( "." );
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
            Logger.Writer.LogMemberError( $"method call failed unexpectedly:",e );
        }
        Logger.Writer.LogInformation( "    done." );

        // Print addresses of all port mappers found...

        for ( int idx = 0; idx < _portmappers.Count; ++idx )
            Logger.Writer.LogInformation( $"Found: {_portmappers[idx]!}" );

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
    /// </remarks>
    [TestMethod]
    public void ClientShouldBroadcast()
    {
        IPAddress address = IPAddress.Parse( "192.168.0.255" );
        RemoteHostBroadcastTest.AssertClientShouldBroadcast( address, 2001 );
    }

}
