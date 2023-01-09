using System.ComponentModel;

namespace cc.isr.ONC.RPC;

/// <summary>
/// A collection of constants used for ONC/RPC messages to identify the type of message.
/// </summary>
/// <remarks>
/// Currently, ONC/RPC messages can be either calls or replies. Calls are sent by ONC/RPC clients
/// to servers to call a remote procedure. A server then will answer with a corresponding reply 
/// message, except for batched calls. <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public enum OncRpcMessageType
{

    /// <summary>   An enum constant representing the not specified option. </summary>
    [Description( "Unspecified ONC/RPC call." )] NotSpecified = -1,

    /// <summary>
    /// Identifies an ONC/RPC call. By a "call" a client request that a server carries
    /// out a particular remote procedure. <para>
    /// Renamed from <c>ONCRPC_CALL = 0</c>. </para>
    /// </summary>
    [Description( "Identifies an ONC/RPC call." )] OncRpcCallMessageType = 0,

    /// <summary>
    /// Identifies an ONC/RPC reply. A server responds with a "reply" after a client has
    /// sent a "call" for a particular remote procedure, sending back the results of calling that
    /// procedure.  <para>
    /// Renamed from <c>ONCRPC_REPLY = 1</c>. </para>
    /// </summary>
    [Description( "Identifies an ONC/RPC reply." )] OncRpcReplyMessageType = 1,
}

