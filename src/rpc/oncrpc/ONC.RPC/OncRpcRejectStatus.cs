namespace cc.isr.ONC.RPC;

/// <summary>
/// A collection of constants used to describe why a remote procedure call message was rejected.
/// </summary>
/// <remarks>
/// This constants are used in <see cref="OncRpcReplyMessageBase"/>
/// objects, which represent rejected messages if their <see cref="OncRpcReplyMessageBase.ReplyStatus"/>
/// field has the value <see cref="OncRpcReplyStatus.OncRpcMessageDenied"/>.
/// </remarks>
public class OncRpcRejectStatus
{
    /// <summary>
    /// (Immutable) Wrong ONC/RPC protocol version used in call (it needs to be version 2). <para>
    /// Renamed from ONCRPC_RPC_MISMATCH = 0. </para>
    /// </summary>
    public const int OncRpcWrongProtocolVersion = 0;

    /// <summary>
    /// (Immutable) The remote ONC/RPC server could not authenticate the caller.  <para>
    /// Renamed from ONCRPC_AUTH_ERROR = 1. </para>
    /// </summary>
    public const int OncRpcAuthError = 1;
}
