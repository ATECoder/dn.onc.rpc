namespace cc.isr.ONC.RPC.Portmap;

/// <summary>
/// A collection of constants used for ONC/RPC messages to identify the 
/// ONC/RPC port mappers port, program and version.
/// </summary>
/// <remarks> <para>
/// 
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public class OncRpcPortmapConstants
{

    /// <summary>
    /// (Immutable) Well-known port where the portmap process can be found on Internet hosts. <para>
    /// 
    /// Renamed from <c>PMAP_PORT = 111</c>. </para>
    /// </summary>
    public const int OncRpcPortmapPortNumber = 111;

    /// <summary>   (Immutable) Program number of the port mapper as defined in RFC 1832.  <para>
    /// 
    /// Renamed from <c>PMAP_PROGRAM = 100000.</c> </para> </summary>
    public const int OncRpcPortmapProgramNumber = 100000;

    /// <summary>
    /// (Immutable) Program version number of the port mapper as defined in RFC 1832.  <para>
    /// 
    /// Renamed from <c>PMAP_VERSION = 2</c>. </para>
    /// </summary>
    public const int OncRpcPortmapProgramVersionNumber = 2;

}
