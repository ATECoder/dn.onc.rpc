using cc.isr.ONC.RPC.Portmap;

namespace cc.isr.ONC.RPC.MSTest.PortMapper;

/// <summary>   (Unit Test Class) an embedded portmap test. </summary>
/// <remarks>   2022-12-22. </remarks>
[TestClass]
public class EmbeddedPortmapTest
{

    /// <summary>   Assert portmap service should start. </summary>
    /// <remarks>   2022-12-30. </remarks>
    internal static void AssertPortmapServiceShouldStart()
    {

        // Ignore all problems during unregistration.

        OncRpcEmbeddedPortmapService epm;

        Console.WriteLine( "Checking for portmap service: " );
        bool externalPortmap = OncRpcEmbeddedPortmapService.IsPortmapRunning();
        if ( externalPortmap )
            Console.WriteLine( "A portmap service is already running." );
        else
            Console.WriteLine( "No portmap service available." );

        // Create embedded portmap service and check whether is has sprung
        // into action.

        Console.WriteLine( "Creating embedded portmap instance: " );
        epm = new OncRpcEmbeddedPortmapService();

        if ( !epm.EmbeddedPortmapInUse() )
            Console.WriteLine( "embedded service not used: " );
        else
            Console.WriteLine( "embedded service started: " );

        if ( epm.EmbeddedPortmapInUse() == externalPortmap )
        {
            Assert.Fail( "ERROR: no service available or both." );
        }
        Console.WriteLine( "Passed." );
    }


    /// <summary>   (Unit Test Method) embedded portmap service should pass. </summary>
    /// <remarks>   2022-12-20. </remarks>
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

        OncRpcPortmapClient pmap = new( IPAddress.Loopback );
        Console.Out.Write( "Deregistering non-existing program: " );
        bool actual = pmap.UnsetPort( dummyProgram, dummyVersion );
        Assert.IsFalse( actual );
        Console.Out.WriteLine( "Passed." );

        Console.Out.Write( "Registering dummy program: " );
        actual = pmap.SetPort( dummyProgram, dummyVersion, OncRpcProtocols.OncRpcTcp, dummyPort );
        Assert.IsTrue( actual );
        Console.Out.WriteLine( "Passed." );

        Console.Out.Write( "Deregistering dummy program: " );
        actual = pmap.UnsetPort( dummyProgram, dummyVersion );
        Assert.IsTrue( actual );
        Console.Out.WriteLine( "Passed." );

        // let the service stop.
        int timeout = 1000;
        DateTime endtime= DateTime.Now.AddMilliseconds( timeout );
        while ( DateTime.Now < endtime && OncRpcEmbeddedPortmapService.IsPortmapRunning( timeout / 5 ) ) { Thread.Sleep( timeout / 5 );  }

        // Check that an embedded portmap service spins down properly if it
        // was started within this test.
        if ( OncRpcEmbeddedPortmapService.IsPortmapRunning() ) // && !externalPortmap )
            Assert.Fail( "ERROR: embedded portmap service still running." );
    }

}
