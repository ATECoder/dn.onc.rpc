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

    /// <summary>   (Unit Test Method) embedded Portmap service should pass. </summary>
    /// <remarks>
    /// <code>
    /// Standard Output: 
    /// 2023-02-04 19:34:46.368,Starting the embedded Portmap service
    /// 2023-02-04 19:34:46.371,Checking for Portmap service
    /// 2023-02-04 19:34:46.401, No Portmap service available.
    /// 2023-02-04 19:34:46.401,Creating embedded Portmap instance
    /// 2023-02-04 19:34:46.613, Portmap service started; checked 29.9 ms.
    /// 2023-02-04 19:34:46.613,The embedded Portmap service started in 242.3 ms
    /// 2023-02-04 19:34:46.613,Deregistering non-existing program;
    /// 2023-02-04 19:34:46.618,deregistering a non-existing program was ignored.
    /// 2023-02-04 19:34:46.618, Registering dummy program;
    /// 2023-02-04 19:34:46.618,Registering a dummy program worked.
    /// 2023-02-04 19:34:46.618,Deregistering dummy program;
    /// 2023-02-04 19:34:46.619,Deregistering the registered dummy program worked.
    /// 2023-02-04 19:34:46.619, Exiting test method; OncRpcEmbeddedPortmapServiceStub will be disposed...
    /// </code>
    /// </remarks>
    [TestMethod]
    public void EmbeddedPortmapServiceShouldPass()
    {

        Logger.Writer.LogInformation( "Starting the embedded Portmap service" );
        Stopwatch stopwatch = Stopwatch.StartNew();
        using OncRpcEmbeddedPortmapServiceStub epm = OncRpcEmbeddedPortmapServiceStub.StartEmbeddedPortmapService(); // AssertPortmapServiceShouldStart();

        // It is assumed that no external Portmap services are available.
        Assert.IsFalse( epm.UsingExternalPortmapService, $"External Portmap services are not expected." );

        Logger.Writer.LogInformation( $"The embedded Portmap service started in {stopwatch.Elapsed.TotalMilliseconds:0.0} ms" );

        // Now register dummy ONC/RPC program. Note that the embedded
        // Portmap service must not automatically spin down when deregistering
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

        // dispose of the Portmap service
        Logger.Writer.LogInformation( $"Exiting test method; {nameof( OncRpcEmbeddedPortmapServiceStub )} will be disposed..." );

    }
}
