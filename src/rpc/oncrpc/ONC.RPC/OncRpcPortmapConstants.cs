namespace cc.isr.ONC.RPC;

/// <summary>
/// A collection of constants used for ONC/RPC messages to identify the 
/// ONC/RPC port mappers p[ort, program and version.
/// </summary>
/// <remarks> <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public class OncRpcPortmapConstants
{

    /// <summary>
    /// (Immutable) Well-known port where the portmap process can be found on Internet hosts. <para>
    /// Renamed from OncRpcPortmapConstants.OncRpcPortmapPortNumber = 111. </para>
    /// </summary>
    public const int OncRpcPortmapPortNumber = 111;

    /// <summary>   (Immutable) Program number of the port mapper as defined in RFC 1832.  <para>
    /// Renamed from OncRpcPortmapConstants.OncRpcPortmapProgramNumber = 100000. </para> </summary>
    public const int OncRpcPortmapProgramNumber = 100000;

    /// <summary>
    /// (Immutable) Program version number of the port mapper as defined in RFC 1832.  <para>
    /// Renamed from OncRpcPortmapConstants.OncRpcPortmapProgramVersionNumber = 2. </para>
    /// </summary>
    public const int OncRpcPortmapProgramVersionNumber = 2;

}
