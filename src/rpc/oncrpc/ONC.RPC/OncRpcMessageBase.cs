namespace cc.isr.ONC.RPC;

/// <summary>
/// The abstract <see cref="OncRpcMessageBase"/> class is a superclass for the <see cref="OncRpcMessageType.OncRpcCallMessageType"/> 
/// and <see cref="OncRpcMessageType.OncRpcReplyMessageType"/> message types defined by the ONC/RPC standard.
/// </summary>
/// <remarks>
/// The only things common to all ONC/RPC messages are a message identifier and the message type.
/// All other things do not come in until derived classes are introduced. <para>
///  
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public abstract class OncRpcMessageBase
{

    /// <summary>   The default message identifier. </summary>
    /// <value> The message identifier default. </value>
    public static int MessageIdDefault { get; set; } = 0;

    /// <summary>
    /// Constructs a new <see cref="OncRpcMessageBase"/> object with default values: a given message id.
    /// The <see cref="MessageType"/> is initialized as <see cref="OncRpcMessageType.NotSpecified"/> (-1).
    /// </summary>
    /// <param name="messageId">    The message id is used to identify matching ONC/RPC calls and
    ///                             replies. </param>
    public OncRpcMessageBase( int messageId )
    {
        this.MessageId = messageId;
        this.MessageType = OncRpcMessageType.NotSpecified;
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
    /// initialized as <see cref="OncRpcMessageType.NotSpecified"/> (-1).
    /// </summary>
    /// <value> The type of the message. </value>
    public OncRpcMessageType MessageType { get; set; }

    /// <summary>   Generates a message identifier with a more-or-less random value. </summary>
    /// <remarks>   Presently, the 'random' value is based on a seed of <see cref="DateTime.Now"/>.Ticks
    /// that is XOR'ed with its 31 right shifted value
    /// </remarks>
    /// <returns>   The client identifier. </returns>
    public static int GenerateMessageIdentifier()
    {
        // Initialize the client identifier with some more-or-less random value.
        long seed = DateTime.Now.Ticks;
        return ( int ) seed ^ ( int ) (seed >> (32 & 0x1f));
    }


}
