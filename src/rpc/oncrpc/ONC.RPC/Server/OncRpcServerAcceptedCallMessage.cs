namespace cc.isr.ONC.RPC.Server;

/// <summary>
/// The server <see cref="OncRpcServerAcceptedCallMessage"/> class represents (on the sender's side) an
/// accepted ONC/RPC call.
/// </summary>
/// <remarks>
/// In ONC/RPC babble, an "accepted" call does not mean that it carries a result from the remote
/// procedure call, but rather that the call was accepted at the basic ONC/RPC level and no
/// authentication failure or else occurred. <para>This ONC/RPC reply header class is only a
/// convenience for server implementors.  </para> <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public class OncRpcServerAcceptedCallMessage : OncRpcServerReplyMessage
{
    /// <summary>
    /// Constructs an server <see cref="OncRpcServerAcceptedCallMessage"/> object which represents an
    /// accepted call, which was also successfully executed, so the reply will contain information
    /// from the remote procedure call.
    /// </summary>
    /// <param name="call"> The call message header, which is used to Constructs the matching reply
    ///                     message header from. </param>
    public OncRpcServerAcceptedCallMessage( OncRpcServerCallMessage call ) : base( call, OncRpcReplyStatus.OncRpcMessageAccepted, OncRpcAcceptStatus.OncRpcSuccess,
        OncRpcReplyMessageBase.UnusedMessageParameter, OncRpcReplyMessageBase.UnusedMessageParameter, OncRpcReplyMessageBase.UnusedMessageParameter, OncRpcAuthStatus.OncRpcAuthOkay )
    {
    }

    /// <summary>
    /// Constructs an server <see cref="OncRpcServerAcceptedCallMessage"/> object which represents an accepted call,
    /// which was not necessarily successfully carried out.
    /// </summary>
    /// <remarks>
    /// The <paramref name="acceptStatus"/> will then indicate the exact outcome of the ONC/RPC call.
    /// </remarks>
    /// <param name="call">         The call message header, which is used to Constructs the matching
    ///                             reply message header from. </param>
    /// <param name="acceptStatus"> The accept status of the call. This can be any one of the
    ///                             constants defined in the <see cref="OncRpcAcceptStatus"/> interface. </param>
    public OncRpcServerAcceptedCallMessage( OncRpcServerCallMessage call, int acceptStatus ) : base( call, OncRpcReplyStatus.OncRpcMessageAccepted,
        acceptStatus, OncRpcReplyMessageBase.UnusedMessageParameter, OncRpcReplyMessageBase.UnusedMessageParameter, OncRpcReplyMessageBase.UnusedMessageParameter,
        OncRpcAuthStatus.OncRpcAuthOkay )
    {
    }

    /// <summary>
    /// Constructs an server <see cref="OncRpcServerAcceptedCallMessage"/> object for an accepted call with an
    /// unsupported version.
    /// </summary>
    /// <remarks>
    /// The reply will contain information about the lowest and highest supported version.
    /// </remarks>
    /// <param name="call"> The call message header, which is used to Constructs the matching reply
    ///                     message header from. </param>
    /// <param name="low">  Lowest program version supported by this ONC/RPC server. </param>
    /// <param name="high"> Highest program version supported by this ONC/RPC server. </param>
    public OncRpcServerAcceptedCallMessage( OncRpcServerCallMessage
         call, int low, int high ) : base( call, OncRpcReplyStatus.OncRpcMessageAccepted, OncRpcAcceptStatus.OncRpcProgramVersionMismatch,
             OncRpcReplyMessageBase.UnusedMessageParameter, low, high, OncRpcAuthStatus.OncRpcAuthOkay )
    {
    }
}
