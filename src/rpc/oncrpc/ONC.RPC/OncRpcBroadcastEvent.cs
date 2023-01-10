using cc.isr.ONC.RPC.Client;

namespace cc.isr.ONC.RPC;

/// <summary>
/// The class <see cref="OncRpcBroadcastEvent"/> defines an event fired by
/// <see cref="OncRpcUdpClient">UDP/IP-based clients</see> whenever replies to a
/// <see cref="OncRpcUdpClient.BroadcastCall(int, IXdrCodec, IXdrCodec, IOncRpcBroadcastListener)"/>
/// are received.  
/// </summary>
/// <remarks>   Remote Tea authors: Harald Albrecht, Jay Walters. </remarks>
[Serializable]
public class OncRpcBroadcastEvent
{

    /// <summary>
    /// Creates a new <see cref="OncRpcBroadcastEvent"/> object and initializes its state.
    /// </summary>
    /// <param name="source">           The <see cref="OncRpcUdpClient">ONC/RPC client object</see>
    ///                                 which has fired this event. </param>
    /// <param name="replyAddress">     Internet address of reply's origin. </param>
    /// <param name="procedureNumber">  Procedure number of ONC/RPC call. </param>
    /// <param name="requestCodec">     The XDR codec that is sent to the procedure call. </param>
    /// <param name="replyCodec">       The XDR codec that receives the result of the procedure call. </param>
    public OncRpcBroadcastEvent( OncRpcUdpClient source, IPAddress replyAddress, int procedureNumber, IXdrCodec requestCodec, IXdrCodec replyCodec )
    {
        this.Source = source;
        this.ReplyAddress = replyAddress;
        this.ProcedureNumber = procedureNumber;
        this.MessageSent = requestCodec;
        this.Reply = replyCodec;
    }

    /// <summary>   Gets or sets the source for this event. </summary>
    /// <value> The source. </value>
    public OncRpcUdpClient Source { get; private set; }

    /// <summary>
    /// Gets or sets (private) the address of the sender of the ONC/RPC reply message.
    /// </summary>
    /// <value> The address of the replying server. </value>
    public IPAddress ReplyAddress { get; private set; }

    /// <summary>
    /// Gets or sets (private) Contains the reply from a remote ONC/RPC server, which answered the broadcast call.
    /// </summary>
    public IXdrCodec Reply { get; private set; }

    /// <summary>   Gets or sets (private) the number of the remote procedure called. </summary>
    /// <value> The procedure number. </value>
    public int ProcedureNumber { get; private set; }

    /// <summary>   Gets or sets (private) the parameters sent in the ONC/RPC broadcast call. </summary>
    /// <value> The message sent. </value>
    public IXdrCodec MessageSent { get; private set; }

}
