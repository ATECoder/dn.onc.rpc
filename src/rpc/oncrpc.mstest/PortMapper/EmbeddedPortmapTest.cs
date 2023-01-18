using System.Diagnostics;

using cc.isr.ONC.RPC.Portmap;

namespace cc.isr.ONC.RPC.MSTest.PortMapper;

/// <summary>   (Unit Test Class) an embedded portmap test. </summary>
[TestClass]
public class EmbeddedPortmapTest
{

    /// <summary>   Assert portmap service should start. </summary>
    internal static void AssertPortmapServiceShouldStart()
    {
        Console.WriteLine( $"{DateTime.Now.ToShortTimeString()} Checking for portmap service: " );
        bool externalPortmap = OncRpcEmbeddedPortmapService.IsPortmapRunning();
        if ( externalPortmap )
            Console.WriteLine( "A portmap service is already running." );
        else
            Console.WriteLine( "No portmap service available." );

        // Create embedded portmap service and check whether is has sprung
        // into action.

        Console.WriteLine( "Creating embedded portmap instance: " );
        OncRpcEmbeddedPortmapService epm = new();

        if ( !epm.EmbeddedPortmapInUse() )
            Console.WriteLine( "embedded service not used: " );
        else
            Console.WriteLine( "embedded service started: " );

        if ( epm.EmbeddedPortmapInUse() == externalPortmap )
        {
            Assert.Fail( "ERROR: no service available or both." );
        }
        Stopwatch sw = Stopwatch.StartNew();
        externalPortmap = OncRpcEmbeddedPortmapService.IsPortmapRunning();
        Assert.IsTrue( externalPortmap, "portmap service is not running" );
        Console.WriteLine( $"portmap service is {(externalPortmap ? "running" : "idle")}; elapsed: {sw.ElapsedMilliseconds:0}ms" );
    }

    /// <summary>   (Unit Test Method) embedded portmap service should pass. </summary>
    [TestMethod]
    public void EmbeddedPortmapServiceShouldPass()
    {
        AssertPortmapServiceShouldStart();

        // Now register dummy ONC/RPC program. Note that the embedded
        // portmap service must not automatically spin down when deregistering
        // the non-existing dummy program.

        int dummyProgram = 12345678;
        int dummyVersion = 42;
        int dummyPort = 42;

        using OncRpcPortmapClient pmap = new( IPAddress.Loopback, OncRpcProtocols.OncRpcUdp, Client.OncRpcUdpClient.TransmitTimeoutDefault );
        Console.Out.Write( "Deregistering non-existing program: " );
        bool actual = pmap.UnsetPort( dummyProgram, dummyVersion );
        Assert.IsFalse( actual );
        Console.WriteLine( "Passed." );

        Console.Out.Write( "Registering dummy program: " );
        actual = pmap.SetPort( dummyProgram, dummyVersion, OncRpcProtocols.OncRpcTcp, dummyPort );
        Assert.IsTrue( actual );
        Console.WriteLine( "Passed." );

        Console.Out.Write( "Deregistering dummy program: " );
        actual = pmap.UnsetPort( dummyProgram, dummyVersion );
        Assert.IsTrue( actual );
        Console.WriteLine( "Passed." );

        // let the service stop.
        int timeout = 1000;
        DateTime endtime = DateTime.Now.AddMilliseconds( timeout );
        while ( DateTime.Now < endtime && OncRpcEmbeddedPortmapService.IsPortmapRunning( timeout / 5 ) ) { Thread.Sleep( timeout / 5 ); }

        // Check that an embedded portmap service spins down properly if it
        // was started within this test.
        if ( OncRpcEmbeddedPortmapService.IsPortmapRunning() ) // && !externalPortmap )
            Assert.Fail( "ERROR: embedded portmap service still running." );
    }

}
