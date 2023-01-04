namespace cc.isr.ONC.RPC;

/// <summary>
/// The abstract <see cref="OncRpcMessageBase"/> class is a superclass for the <see cref="OncRpcMessageType.OncRpcCallMessageType"/> 
/// and <see cref="OncRpcMessageType.OncRpcReplyMessageType"/> message types defined by the ONC/RPC standard.
/// </summary>
/// <remarks>
/// The only things common to all ONC/RPC messages are a message identifier and the message type.
/// All other things do not come in until derived classes are introduced. <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public abstract class OncRpcMessageBase
{

    /// <summary>   (Immutable) the default message identifier. </summary>
    public const int DefaultMessageId = 0;

    /// <summary>
    /// Constructs  a new server <see cref="OncRpcMessageBase"/> object with default values: a given message type
    /// and no particular message identifier. The <see cref="MessageType"/> is initialized as 'not-specified'=-1.
    /// </summary>
    /// <param name="messageId">    The message id is used to identify matching ONC/RPC calls and
    ///                             replies. </param>
    public OncRpcMessageBase( int messageId )
    {
        this.MessageId = messageId;
        this.MessageType = -1;
    }

    /// <summary>
    /// The message id is used to identify matching ONC/RPC calls and replies.  This is typically
    /// chosen by the communication partner sending a request. The matching reply then must have the
    /// same message identifier, so the receiver can match calls and replies.
    /// </summary>
    public int MessageId { get; set; }

    /// <summary>
    /// The kind of ONC/RPC message, which can be either a call or a reply.  Can be one of the
    /// constants defined in <see cref="OncRpcMessageType"/>. The <see cref="MessageType"/> is
    /// initialized as 'not-specified'=-1.
    /// </summary>
    /// <value> The type of the message. </value>
    public int MessageType { get; set; }
}
