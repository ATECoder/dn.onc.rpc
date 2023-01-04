namespace cc.isr.ONC.RPC;

/// <summary>
/// A collection of constants used for ONC/RPC messages to identify the remote procedure calls
/// offered by ONC/RPC port mappers.
/// </summary>
/// <remarks> <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public class OncRpcPortmapServices
{
    /// <summary>
    /// (Immutable) Procedure number of portmap service to register an ONC/RPC server. <para>
    /// Renamed from PMAP_SET (=1)</para>
    /// </summary>
    public const int OncRpcPortmapRegisterServer = 1;

    /// <summary>
    /// (Immutable) Procedure number of portmap service to unregister an ONC/RPC server. <para>
    /// Renamed from PMAP_UNSET (=2)</para>
    /// </summary>
    public const int OncRpcPortmapUnregisterServer = 2;

    /// <summary>
    /// (Immutable)
    /// Procedure number of portmap service to retrieve port number of a particular ONC/RPC server. <para>
    /// Renamed from PMAP_GETPORT (=3)</para>
    /// </summary>
    public const int OncRpcPortmapGetPortNumber = 3;

    /// <summary>
    /// (Immutable)
    /// Procedure number of portmap service to return information about all currently registered
    /// ONC/RPC servers. <para>
    /// Renamed from PMAP_DUMP (=4)</para>
    /// </summary>
    public const int OncRpcPortmapListServersInfo = 4;

    /// <summary>
    /// (Immutable)
    /// Procedure number of portmap service to indirectly call a remote procedure an ONC/RPC server
    /// through the ONC/RPC portmapper. <para>
    /// Renamed from PMAP_CALLIT (=5)</para>
    /// </summary>
    public const int OncRpcPortmapCallRemoteProcedure = 5;
}
