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
    /// <remarks>
    /// <code>
    /// Standard Output: 
    ///   2023-02-02 20:00:17.305,Starting the embedded portmap service
    ///   2023-02-02 20:00:17.305,Checking for portmap service
    ///   2023-02-02 20:00:17.413, No portmap service available.
    ///   2023-02-02 20:00:17.413,Creating embedded portmap instance
    ///   2023-02-02 20:00:17.636, Portmap service started; checked 107.8 ms.
    ///   2023-02-02 20:00:17.636,The embedded portmap service started in 330.8 ms
    ///   2023-02-02 20:00:17.636,Deregistering non-existing program;
    ///   2023-02-02 20:00:17.637,deregistering a non-existing program was ignored.
    ///   2023-02-02 20:00:17.637,Registering dummy program;
    ///   2023-02-02 20:00:17.637,Registering a dummy program worked.
    ///   2023-02-02 20:00:17.637,Deregistering dummy program;
    ///   2023-02-02 20:00:17.637,Deregistering the registered dummy program worked.
    ///   2023-02-02 20:00:17.637, Exiting test method; OncRpcEmbeddedPortmapServiceStub will be disposed...
    /// </code>
    /// </remarks>
    [TestMethod]
    public void EmbeddedPortmapServiceShouldPass()
    {

        Logger.Writer.LogInformation( "Starting the embedded portmap service" );
        Stopwatch stopwatch = Stopwatch.StartNew();
        using OncRpcEmbeddedPortmapServiceStub epm = OncRpcEmbeddedPortmapServiceStub.StartEmbeddedPortmapService(); // AssertPortmapServiceShouldStart();

        // It is assumed that no external portmap services are available.
        Assert.IsFalse( epm.UsingExternalPortmapService, $"External portmap services are not expected." );

        Logger.Writer.LogInformation( $"The embedded portmap service started in {stopwatch.Elapsed.TotalMilliseconds:0.0} ms" );

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

        Logger.Writer.LogInformation( "Registering dummy program;" );
        actual = pmap.SetPort( dummyProgram, dummyVersion, OncRpcProtocol.OncRpcTcp, dummyPort );
        Assert.IsTrue( actual );
        Logger.Writer.LogInformation( "Registering a dummy program worked." );

        Logger.Writer.LogInformation( "Deregistering dummy program;" );
        actual = pmap.UnsetPort( dummyProgram, dummyVersion );
        Assert.IsTrue( actual );
        Logger.Writer.LogInformation( "Deregistering the registered dummy program worked." );

        // dispose of the portmap service
        Logger.Writer.LogInformation( $"Exiting test method; {nameof( OncRpcEmbeddedPortmapServiceStub )} will be disposed..." );

    }
}
