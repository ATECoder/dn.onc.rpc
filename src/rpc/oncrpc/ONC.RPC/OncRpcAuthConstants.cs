namespace cc.isr.ONC.RPC;

/// <summary>
/// A collection of constants related to authentication and generally useful for ONC/RPC.
/// </summary>
/// <remarks> <para>
/// 
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public class OncRpcAuthConstants
{
    /// <summary>
    /// (Immutable) Maximum length of opaque authentication information. <para>
    /// 
    /// Renamed from <c>ONCRPC_MAX_AUTH_BYTES = 400</c>. </para>
    /// </summary>
    public const int OncRpcMaxAuthBytes = 400;

    /// <summary>
    /// (Immutable) Maximum length of machine name.  <para>
    /// 
    /// Renamed from <c>ONCRPC_MAX_MACHINE_NAME = 255</c>. </para>
    /// </summary>
    public const int OncRpcMaxMachineNameLength = 255;

    /// <summary>
    /// (Immutable) Maximum allowed number of groups.  <para>
    /// 
    /// Renamed from <c>ONCRPC_MAX_GROUPS = 16</c>. </para>
    /// </summary>
    public const int OncRpcMaxAllowedGroups = 16;
}
