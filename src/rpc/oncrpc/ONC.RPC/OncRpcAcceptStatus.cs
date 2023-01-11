using System.ComponentModel;

namespace cc.isr.ONC.RPC;

/// <summary>
/// A collection of constants used to identify the acceptance status of ONC/RPC reply messages.
/// </summary>
/// <remarks> <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public enum OncRpcAcceptStatus
{
    /// <summary>   The remote procedure was called and executed successfully. <para>
    /// Renamed from <c>ONCRPC_SUCCESS = 0</c>. </para> </summary>
    [Description( "The remote procedure was called and executed successfully." )]
    OncRpcSuccess = 0,

    /// <summary>
    /// The requested program is not available. So the remote host does not export this
    /// particular program and the ONC/RPC server which you tried to send a RPC call message doesn't
    /// know of this program either. <para>
    /// Renamed from <c>ONCRPC_PROG_UNAVAIL = 1</c>. </para>
    /// </summary>
    [Description( "The requested program is not available." )] OncRpcProgramNotAvailable = 1,

    /// <summary>
    /// The remote ONC/RPC server does not support this particular version of the program. <para>
    /// Renamed from <c>ONCRPC_PROG_MISMATCH = 2</c>. </para>
    /// </summary>
    [Description( "The remote ONC/RPC server does not support this particular version of the program." )]
    OncRpcProgramVersionMismatch = 2,

    /// <summary>
    /// The procedure requested is not available. The remote ONC/RPC server does not
    /// support this particular procedure. <para>
    /// Renamed from <c>ONCRPC_PROC_UNAVAIL = 3</c>. </para>
    /// </summary>
    [Description( "The remote ONC/RPC server does not support this particular procedure." )]
    OncRpcProcedureNotAvailable = 3,

    /// <summary>
    /// The server could not decode the arguments sent within the ONC/RPC call message. <para>
    /// Renamed from <c>ONCRPC_GARBAGE_ARGS = 4</c>. </para>
    /// </summary>
    [Description( "The server could not decode the arguments sent within the ONC/RPC call message." )]
    OncRpcUnableToDecodeArguments = 4,

    /// <summary>
    /// The server encountered a system error and thus was not able to process the procedure call.
    /// Causes might be memory shortage, disinterest and sloth. <para>
    /// Renamed from <c>ONCRPC_SYSTEM_ERR = 5</c>. </para>
    /// </summary>
    [Description( "The server encountered a system error and thus was not able to process the procedure call." )]
    OncRpcSystemError = 5,
}
