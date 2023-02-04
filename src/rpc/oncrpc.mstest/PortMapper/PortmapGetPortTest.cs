using System.Net.Sockets;
using cc.isr.ONC.RPC.Logging;
using cc.isr.ONC.RPC.Codecs;
using cc.isr.ONC.RPC.Portmap;

namespace cc.isr.ONC.RPC.MSTest.PortMapper;

/// <summary>   (Unit Test Class) a portmap get port test. </summary>
[TestClass]
public class APortmapGetPortTest
{

    /// <summary>   (Unit Test Method) portmap should get port. </summary>
    /// <remarks> THIS TEST OFTEN FAILED WHEN RUN AFTER THE EMBEDDED PORTMAP TEST. 
    /// so we changed the order for now. 
    /// <code>
    /// Standard Output: 
    ///   2023-02-02 20:00:16.956,Checking for portmap service
    ///   2023-02-02 20:00:17.062, No portmap service available.
    ///   2023-02-02 20:00:17.062,Creating embedded portmap instance
    ///   2023-02-02 20:00:17.289, Portmap service started; checked 105.5 ms.
    ///   2023-02-02 20:00:17.290,Host: 192.168.4.28
    ///   2023-02-02 20:00:17.290,pinging port mapper;
    ///   2023-02-02 20:00:17.294,port mapper pinged.
    ///   2023-02-02 20:00:17.294,GetPort for non-existing program
    ///   2023-02-02 20:00:17.295, succeeded; received error code( OncRpcProgramNotRegistered(15).
    ///   2023-02-02 20:00:17.295,SetPort dummy server identification:
    ///   2023-02-02 20:00:17.296, SetPort succeeded.
    ///   2023-02-02 20:00:17.296,executing ListRegisteredServers
    ///   2023-02-02 20:00:17.296, ListRegisteredServers succeeded.
    ///   2023-02-02 20:00:17.296,listing Registered servers
    ///   2023-02-02 20:00:17.296, Program Version Protocol Port
    ///   2023-02-02 20:00:17.297,100000 2 OncRpcTcp 111
    ///   2023-02-02 20:00:17.297,100000 2 OncRpcUdp 111
    ///   2023-02-02 20:00:17.297,1 42 OncRpcUdp 65535
    ///   2023-02-02 20:00:17.297,executing UnsetPort dummy server identification:
    ///   2023-02-02 20:00:17.297,UnsetPort succeeded.
    ///   2023-02-02 20:00:17.297, executing ListRegisteredServers
    ///   2023-02-02 20:00:17.297, ListRegisteredServerssucceeded.
    ///   2023-02-02 20:00:17.298, Exiting test method; OncRpcEmbeddedPortmapServiceStub will be disposed...
    /// </code>
    /// </remarks>
    [TestMethod]
    public void PortmapShouldGetPort()
    {
        using OncRpcEmbeddedPortmapServiceStub epm = OncRpcEmbeddedPortmapServiceStub.StartEmbeddedPortmapService(); // AssertPortmapServiceShouldStart();

        // It is assumed that no external portmap services are available.
        Assert.IsFalse( epm.UsingExternalPortmapService, $"External portmap services are not expected." );

        IPHostEntry host = Dns.GetHostEntry( Dns.GetHostName() );

        var ipAddress = host
            .AddressList
            .FirstOrDefault( ip => ip.AddressFamily == AddressFamily.InterNetwork );
        Logger.Writer.LogInformation( $"Host: {ipAddress}" );

        // Create a portmap client object, which can then be used to contact
        // a local or remote ONC/RPC portmap process. In this test we contact
        // the local port mapper.

        using OncRpcPortmapClient portmap = new( ipAddress!, OncRpcProtocol.OncRpcUdp, Client.OncRpcUdpClient.TransmitTimeoutDefault );

        // portmap.RetransmitMode = OncRpcUdpRetransmitMode.OncRpcFixedTimeout;
        // portmap.RetransmitTimeout = 3*1000;

        // Ping the port mapper...

        Logger.Writer.LogInformation( "pinging port mapper;" );
        portmap.PingPortmapService();
        Logger.Writer.LogInformation( "port mapper pinged." );

        // Ask for a non-existent ONC/RPC server.

        int port;
        Logger.Writer.LogInformation( $"{nameof( OncRpcPortmapClient.GetPort )} for non-existing program" );
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
            Logger.Writer.LogInformation( $"succeeded; received error code ({OncRpcExceptionReason.OncRpcProgramNotRegistered}({( int ) OncRpcExceptionReason.OncRpcProgramNotRegistered})." );
        }

        // Register dummy ONC/RPC server.

        Logger.Writer.LogInformation( $"{nameof( OncRpcPortmapClient.SetPort )} dummy server identification: " );
        try
        {
            _ = portmap.SetPort( 1, 42, OncRpcProtocol.OncRpcUdp, 65535 );
        }
        catch ( OncRpcException e )
        {
            Assert.Fail( $"method call failed unexpectedly: {e}" );
        }
        Logger.Writer.LogInformation( $"{nameof( OncRpcPortmapClient.SetPort )} succeeded." );

        // Now dump the current list of registered servers.
        OncRpcServerIdentifierCodec[] list = Array.Empty<OncRpcServerIdentifierCodec>();
        bool found = false;
        Logger.Writer.LogInformation( $"executing {nameof( OncRpcPortmapClient.ListRegisteredServers )}" );
        try
        {
            list = portmap.ListRegisteredServers();
        }
        catch ( OncRpcException e )
        {
            Assert.Fail( $"method call failed unexpectedly: {e}" );
        }
        Logger.Writer.LogInformation( $"{nameof( OncRpcPortmapClient.ListRegisteredServers )} succeeded." );

        Logger.Writer.LogInformation( "listing Registered servers" );
        Logger.Writer.LogInformation( $" Program Version Protocol Port" );
        foreach ( OncRpcServerIdentifierCodec value in list )
        {
            if ( value.Program == 1 && value.Version == 42 && value.Protocol == OncRpcProtocol.OncRpcUdp && value.Port == 65535 )
                found = true;
            Logger.Writer.LogInformation( $"{value.Program} {value.Version} {value.Protocol} {value.Port}" );
        }
        Assert.IsTrue( found, "expected dummy server was not found among the registered servers." );

        // Deregister dummy ONC/RPC server.
        Logger.Writer.LogInformation( $"executing {nameof( OncRpcPortmapClient.UnsetPort )} dummy server identification: " );
        try
        {
            _ = portmap.UnsetPort( 1, 42 );
        }
        catch ( OncRpcException e )
        {
            Assert.Fail( $"method call failed unexpectedly: {e}" );
        }
        Logger.Writer.LogInformation( $"{nameof( OncRpcPortmapClient.UnsetPort )} succeeded." );

        // Now dump again the current list of registered servers.
        found = false;
        list = Array.Empty<OncRpcServerIdentifierCodec>();
        Logger.Writer.LogInformation( $"executing {nameof( OncRpcPortmapClient.ListRegisteredServers )}" );
        try
        {
            list = portmap.ListRegisteredServers();
        }
        catch ( OncRpcException e )
        {
            Assert.Fail( $"method call failed unexpectedly: {e}" );
        }
        Logger.Writer.LogInformation( $"{nameof( OncRpcPortmapClient.ListRegisteredServers )}succeeded." );

        foreach ( OncRpcServerIdentifierCodec value in list )
        {
            if ( value.Program == 1 && value.Version == 42 && value.Protocol == OncRpcProtocol.OncRpcUdp && value.Port == 65535 )
                Assert.Fail( $"registered dummy ({value.Program},{value.Version}) server still found after deregistering." );
        }

        // Release resources bound by portmap client object as soon as possible
        portmap.Close();

        // dispose of the portmap service
        Logger.Writer.LogInformation( $"Exiting test method; {nameof( OncRpcEmbeddedPortmapServiceStub )} will be disposed..." );
    }
}
