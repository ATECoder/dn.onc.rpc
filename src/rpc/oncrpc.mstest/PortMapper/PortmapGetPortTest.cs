using System.Net.Sockets;

using cc.isr.ONC.RPC.Codecs;
using cc.isr.ONC.RPC.Portmap;

namespace cc.isr.ONC.RPC.MSTest.PortMapper;

/// <summary>   (Unit Test Class) a portmap get port test. </summary>
/// <remarks>   2022-12-22. </remarks>
[TestClass]
public class PortmapGetPortTest
{

    /// <summary>   (Unit Test Method) portmap should get port. </summary>
    /// <remarks>   2022-12-22. </remarks>
    [TestMethod]
    public void PortmapShouldGetPort()
    {

        EmbeddedPortmapTest.AssertPortmapServiceShouldStart();

        IPHostEntry host = Dns.GetHostEntry( Dns.GetHostName() );

        var ipAddress = host
            .AddressList
            .FirstOrDefault( ip => ip.AddressFamily == AddressFamily.InterNetwork );
        Console.WriteLine( $"Host: {ipAddress}" );

        // Create a portmap client object, which can then be used to contact
        // a local or remote ONC/RPC portmap process. In this test we contact
        // the local port mapper.

        OncRpcPortmapClient portmap = new( ipAddress );

        //portmap.setRetransmissionMode(OncRpcUdpRetransmissionMode.FIXED);
        //portmap.setRetransmissionTimeout(3*1000);

        // Ping the port mapper...

        Console.Out.Write( "pinging port mapper: " );
        portmap.Ping();
        Console.WriteLine( "port mapper is alive." );

        // Ask for a non-existent ONC/RPC server.

        int port;
        Console.Out.Write( "getPort() for non-existing program: " );
        try
        {
            port = portmap.GetPort( 1, 1, OncRpcProtocols.OncRpcUdp );
            Assert.Fail( "method call failed (a non-existing program was found)." );
        }
        catch ( OncRpcException e )
        {
            if ( e.Reason != OncRpcExceptionReason.OncRpcProgramNotRegistered )
            {
                Assert.Fail( $"method call failed unexpectedly: {e}" );
            }
            Console.WriteLine( "succeeded (RPC_PROGNOTREGISTERED)." );
        }

        // Register dummy ONC/RPC server.

        Console.Out.Write( "setPort() dummy server identification: " );
        try
        {
            _ = portmap.SetPort( 1, 42, OncRpcProtocols.OncRpcUdp, 65535 );
        }
        catch ( OncRpcException e )
        {
            Assert.Fail( $"method call failed unexpectedly: {e}" );
        }
        Console.WriteLine( "succeeded." );

        // Now dump the current list of registered servers.
        OncRpcServerIdentifierCodec[] list = Array.Empty<OncRpcServerIdentifierCodec>();
        int i;
        bool found = false;
        Console.Out.Write( "listServers(): " );
        try
        {
            list = portmap.ListServers();
        }
        catch ( OncRpcException e )
        {
            Assert.Fail( $"method call failed unexpectedly: {e}" );
        }
        Console.WriteLine( "succeeded." );

        Console.WriteLine( "Servers(): " );
        Console.WriteLine( $" Program Version Protocol Port" );
        for ( i = 0; i < list.Length; ++i )
        {
            if ( list[i].Program == 1 && list[i].Version == 42 && list[i].Protocol == OncRpcProtocols.OncRpcUdp && list[i].Port == 65535 )
                found = true;
            Console.WriteLine( $"  {list[i].Program} {list[i].Version} {list[i].Protocol} {list[i].Port}" );
        }
        Assert.IsTrue( found, "registered dummy server ident not found." );

        // Deregister dummy ONC/RPC server.
        Console.Out.Write( "unsetPort() dummy server identification: " );
        try
        {
            _ = portmap.UnsetPort( 1, 42 );
        }
        catch ( OncRpcException e )
        {
            Assert.Fail( $"method call failed unexpectedly: {e}" );
        }
        Console.WriteLine( "succeeded." );

        // Now dump again the current list of registered servers.
        found = false;
        list = Array.Empty<OncRpcServerIdentifierCodec>();
        Console.Out.Write( "listServers(): " );
        try
        {
            list = portmap.ListServers();
        }
        catch ( OncRpcException e )
        {
            Assert.Fail( $"method call failed unexpectedly: {e}" );
        }
        Console.WriteLine( "succeeded." );
        for ( i = 0; i < list.Length; ++i )
            if ( list[i].Program == 1 && list[i].Version == 42 && list[i].Protocol == OncRpcProtocols.OncRpcUdp && list[i].Port == 65535 )
            {
                found = true;
                break;
            }
        if ( found )
        {
            Assert.Fail( "registered dummy server ident still found after deregistering." );
        }

        // Release resources bound by portmap client object as soon as possible
        portmap.Close();
    }

}
