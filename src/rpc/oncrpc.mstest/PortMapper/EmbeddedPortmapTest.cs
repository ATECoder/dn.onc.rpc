using System.Diagnostics;

using cc.isr.ONC.RPC.Logging;
using cc.isr.ONC.RPC.Portmap;

namespace cc.isr.ONC.RPC.MSTest.PortMapper;

/// <summary>   (Unit Test Class) an embedded portmap test. </summary>
[TestClass]
public class EmbeddedPortmapTest
{

#if false
    /// <summary>   Assert portmap service should start. </summary>
    internal static OncRpcEmbeddedPortmapService AssertPortmapServiceShouldStart()
    {
        Logger.Writer.LogInformation( "Checking for portmap service: " );
        bool externalPortmap = OncRpcEmbeddedPortmapService.TryPingPortmapService();
        if ( externalPortmap )
            Logger.Writer.LogInformation( "A portmap service is already running." );
        else
            Logger.Writer.LogInformation( "No portmap service available." );

        // Create embedded portmap service and check whether is has sprung
        // into action.

        Logger.Writer.LogInformation( "Creating embedded portmap instance: " );
        OncRpcEmbeddedPortmapService epm = new(0 );

        if ( !epm.EmbeddedPortmapInUse() )
            Logger.Writer.LogInformation( "embedded service not used: " );
        else
            Logger.Writer.LogInformation( "embedded service started: " );

        if ( epm.EmbeddedPortmapInUse() == externalPortmap )
        {
            Assert.Fail( "ERROR: no service available or both." );
        }
        Stopwatch sw = Stopwatch.StartNew();
        externalPortmap = OncRpcEmbeddedPortmapService.TryPingPortmapService();
        Assert.IsTrue( externalPortmap, "portmap service is not running" );
        Logger.Writer.LogInformation( $"portmap service is {(externalPortmap ? "running" : "idle")}; elapsed: {sw.ElapsedMilliseconds:0}ms" );
        return epm;
    }
#endif

    /// <summary>   (Unit Test Method) embedded portmap service should pass. </summary>
    [TestMethod]
    public void EmbeddedPortmapServiceShouldPass()
    {

        Logger.Writer.LogInformation( "Starting the embedded portmap service" );
        Stopwatch stopwatch = Stopwatch.StartNew();
        OncRpcEmbeddedPortmapServiceStub epm = OncRpcEmbeddedPortmapServiceStub.StartEmbeddedPortmapService(); // AssertPortmapServiceShouldStart();
        // OncRpcEmbeddedPortmapService epm = EmbeddedPortmapTest.AssertPortmapServiceShouldStart();
        Logger.Writer.LogInformation( $"The embedded portmap service started in {stopwatch.ElapsedMilliseconds:0}ms" );

        // Now register dummy ONC/RPC program. Note that the embedded
        // portmap service must not automatically spin down when deregistering
        // the non-existing dummy program.

        int dummyProgram = 12345678;
        int dummyVersion = 42;
        int dummyPort = 42;

        using OncRpcPortmapClient pmap = new( IPAddress.Loopback, OncRpcProtocols.OncRpcUdp, Client.OncRpcUdpClient.TransmitTimeoutDefault );
        Logger.Writer.LogInformation( "Deregistering non-existing program: " );
        bool actual = pmap.UnsetPort( dummyProgram, dummyVersion );
        Assert.IsFalse( actual );
        Logger.Writer.LogInformation( "   Passed." );

        Console.Out.Write( "Registering dummy program: " );
        actual = pmap.SetPort( dummyProgram, dummyVersion, OncRpcProtocols.OncRpcTcp, dummyPort );
        Assert.IsTrue( actual );
        Logger.Writer.LogInformation( "    Passed." );

        Console.Out.Write( "Deregistering dummy program: " );
        actual = pmap.UnsetPort( dummyProgram, dummyVersion );
        Assert.IsTrue( actual );
        Logger.Writer.LogInformation( "    Passed." );

        // let the service stop.
        int timeout = 1000;
        DateTime endtime = DateTime.Now.AddMilliseconds( timeout );
        while ( DateTime.Now < endtime && OncRpcEmbeddedPortmapServiceStub.TryPingPortmapService( timeout / 5 ) ) { Thread.Sleep( timeout / 5 ); }

        // Check that an embedded portmap service spins down properly if it
        // was started within this test.
        if ( OncRpcEmbeddedPortmapServiceStub.TryPingPortmapService() ) // && !externalPortmap )
            Assert.Fail( "ERROR: embedded portmap service still running." );

        // dispose of the portmap service
        // this does not solve the test abortion issue on testing the broadcast.
        // epm?.PortmapService?.Dispose();
    }

}
