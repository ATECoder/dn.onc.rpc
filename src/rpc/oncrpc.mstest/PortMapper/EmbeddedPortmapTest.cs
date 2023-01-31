using System.Diagnostics;

using cc.isr.ONC.RPC.Logging;
using cc.isr.ONC.RPC.Portmap;

namespace cc.isr.ONC.RPC.MSTest.PortMapper;

/// <summary>   (Unit Test Class) an embedded portmap test. </summary>
[TestClass]
public class EmbeddedPortmapTest
{

    /// <summary>   Cleanup fixture. </summary>
    [ClassCleanup]
    public static void CleanupFixture()
    {
    }

    /// <summary>   (Unit Test Method) embedded portmap service should pass. </summary>
    [TestMethod]
    public void EmbeddedPortmapServiceShouldPass()
    {

        Logger.Writer.LogInformation( "Starting the embedded portmap service" );
        Stopwatch stopwatch = Stopwatch.StartNew();
        using OncRpcEmbeddedPortmapServiceStub epm = OncRpcEmbeddedPortmapServiceStub.StartEmbeddedPortmapService(); // AssertPortmapServiceShouldStart();

        // It is assumed that no external portmap services are available.
        Assert.IsFalse( epm.UsingExternalPortmapService, $"External portmap services are not expected." );

        Logger.Writer.LogInformation( $"The embedded portmap service started in {stopwatch.ElapsedMilliseconds:0}ms" );

        // Now register dummy ONC/RPC program. Note that the embedded
        // portmap service must not automatically spin down when deregistering
        // the non-existing dummy program.

        int dummyProgram = 12345678;
        int dummyVersion = 42;
        int dummyPort = 42;

        using OncRpcPortmapClient pmap = new( IPAddress.Loopback, OncRpcProtocol.OncRpcUdp, Client.OncRpcUdpClient.TransmitTimeoutDefault );

        Logger.Writer.LogInformation( "Deregistering non-existing program;" );
        bool actual = pmap.UnsetPort( dummyProgram, dummyVersion );
        Assert.IsFalse( actual );
        Logger.Writer.LogInformation( "deregistering a non-existing program was ignored." );

        Console.Out.Write( "Registering dummy program;" );
        actual = pmap.SetPort( dummyProgram, dummyVersion, OncRpcProtocol.OncRpcTcp, dummyPort );
        Assert.IsTrue( actual );
        Logger.Writer.LogInformation( "Registering a dummy program worked." );

        Console.Out.Write( "Deregistering dummy program;" );
        actual = pmap.UnsetPort( dummyProgram, dummyVersion );
        Assert.IsTrue( actual );
        Logger.Writer.LogInformation( "Deregistering the registered dummy program worked." );

        // dispose of the portmap service
        Logger.Writer.LogInformation( $"Exiting test method; {nameof( OncRpcEmbeddedPortmapServiceStub )} will be disposed..." );

    }
}
