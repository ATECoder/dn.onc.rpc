using System.ComponentModel;

namespace cc.isr.ONC.RPC.Portmap;

/// <summary>
/// A collection of constants used for ONC/RPC messages to identify the remote procedure calls
/// offered by ONC/RPC port mappers.
/// </summary>
/// <remarks> <para>
/// 
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public enum OncRpcPortmapServiceProcedure
{

    /// <summary>
    /// Procedure number for the portmap ping. <para>
    /// 
    /// This procedure does no work. By convention, procedure 0 of any protocol takes no parameters
    /// and returns no results.</para><para>
    /// 
    /// IBM PMAPPROC_NULL. </para>
    /// </summary>
    OncRpcPortmapPing = 0,

    /// <summary>
    /// Procedure number of Portmap service to register an ONC/RPC server. <para>
    /// 
    /// When a program first becomes available on a machine, it registers itself with the port mapper
    /// program on that machine. The program passes its program, version and transport protocol numbers,
    /// and the port on which it awaits service request. The procedure returns a Boolean response whose 
    /// value is either True if the procedure successfully established the mapping, or False if otherwise. 
    /// The procedure does not establish a mapping if the values for the program, version, and protocol 
    /// parameters indicate a mapping already exists. </para><para>
    /// 
    /// Renamed from <c>PMAP_SET (=1)</c></para>
    /// </summary>
    [Description( "Register an ONC/RPC server" )]
    OncRpcPortmapRegisterServer = 1,

    /// <summary>
    /// Procedure number of Portmap service to unregister an ONC/RPC server. <para>
    /// 
    /// When a program becomes unavailable, it should unregister itself with the port mapper program on the same machine.
    /// The parameters and results have meanings identical to those of the <see cref="OncRpcPortmapServiceProcedure.OncRpcPortmapRegisterServer"/>
    /// procedure. The protocol and port number fields of the argument are ignored. </para><para>
    /// 
    /// Renamed from <c>PMAP_UNSET (=2)</c></para> </summary>
    [Description( "Unregister an ONC/RPC server" )]
    OncRpcPortmapUnregisterServer = 2,

    /// <summary>
    /// Procedure number of Portmap service to retrieve port number of a particular ONC/RPC server. <para>
    /// 
    /// Given a program, version, and transport protocol numbers, this procedure returns the port number on 
    /// which the program is awaiting call requests. A port value of zero means the program has not been 
    /// registered. The port parameter of the argument is then ignored. </para><para>
    /// 
    /// Renamed from <c>PMAP_GETPORT (=3)</c></para> </summary>
    [Description( "Retrieve the port number of a particular ONC/RPC server." )]
    OncRpcPortmapGetPortNumber = 3,

    /// <summary>
    /// Procedure number of Portmap service to return information about all currently registered
    /// ONC/RPC servers. <para>
    /// 
    /// This procedure enumerates all entries in the port mapper database. The procedure takes no parameters 
    /// and returns a list of <see cref="Codecs.OncRpcServerIdentifierCodec">program, version, protocol, and port</see>. </para><para>
    /// 
    /// Renamed from <c>PMAP_DUMP (=4)</c></para> </summary>
    [Description( "Return information about all currently registered ONC/RPC servers." )]
    OncRpcPortmapListRegisteredServers = 4,

    /// <summary>
    /// (Immutable)
    /// Procedure number of Portmap service to indirectly call a remote procedure an ONC/RPC server
    /// through the ONC/RPC portmapper. <para>
    /// 
    /// This procedure allows a caller to call another remote procedure on the same machine without knowing 
    /// the remote procedure's port number. It supports broadcasts to arbitrary remote programs through the 
    /// well-known port mapper port. The program, version, and procedure parameters, and the bytes of the 
    /// arguments parameter of an RPC call represent the program number, version number, procedure number, 
    /// and arguments, respectively. The procedure sends a response only if the procedure is successfully run. 
    /// The port mapper communicates with the remote program using UDP only. The procedure returns the remote 
    /// program's port number, and the bytes of results are the results of the remote procedure.
    /// </para><para>
    /// 
    /// Renamed from <c>PMAP_CALLIT (=5)</c></para> </summary>
    [Description( "Indirectly call a remote procedure an ONC/RPC server through the ONC/RPC portmapper." )]
    OncRpcPortmapCallRemoteProcedure = 5,
}
