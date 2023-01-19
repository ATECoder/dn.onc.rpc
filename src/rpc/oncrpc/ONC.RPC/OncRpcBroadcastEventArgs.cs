using cc.isr.ONC.RPC.Client;

namespace cc.isr.ONC.RPC;

/// <summary>
/// The class <see cref="OncRpcBroadcastEventArgs"/> defines the event arguments for the 
/// an event fired by <see cref="OncRpcUdpClient">UDP/IP-based clients</see> whenever replies to a
/// <see cref="OncRpcUdpClient.BroadcastCall(int, IXdrCodec, IXdrCodec, int)"/>
/// are received.  
/// </summary>
/// <remarks> <para>
/// 
/// Remote Tea authors: Harald Albrecht, Jay Walters. 
/// @atecoder: renamed and changed to inherit from <see cref="System.EventArgs"/>. </para>
/// </remarks>
public class OncRpcBroadcastEventArgs : EventArgs
{

    /// <summary>
    /// Creates a new <see cref="OncRpcBroadcastEventArgs"/> object and initializes its state.
    /// </summary>
    /// <param name="source">           The <see cref="OncRpcUdpClient">ONC/RPC client object</see>
    ///                                 which has fired this event. </param>
    /// <param name="remoteEndPoint">   Endpoint of the remote reply origin. </param>
    /// <param name="procedureNumber">  Procedure number of ONC/RPC call. </param>
    /// <param name="requestCodec">     The XDR codec that is sent to the procedure call. </param>
    /// <param name="replyCodec">       The XDR codec that receives the result of the procedure call. </param>
    public OncRpcBroadcastEventArgs( OncRpcUdpClient source, IPEndPoint remoteEndPoint, int procedureNumber, IXdrCodec requestCodec, IXdrCodec replyCodec )
    {
        this.Source = source;
        this.RemoteEndPoint = new IPEndPoint( remoteEndPoint.Address, remoteEndPoint.Port ) ;
        this.ProcedureNumber = procedureNumber;
        this.MessageSent = requestCodec;
        this.Reply = replyCodec;
    }

    /// <summary>   Gets or sets the source for this event. </summary>
    /// <value> The source. </value>
    public OncRpcUdpClient Source { get; private set; }

    /// <summary>
    /// Gets or sets (private) the <see cref="IPEndPoint"/> of the remote (sender) origin of the
    /// ONC/RPC reply message.
    /// </summary>
    /// <value> The end point of the replying server. </value>
    public IPEndPoint RemoteEndPoint { get; private set; }

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

    /// <summary>   Gets or sets the exception. </summary>
    /// <value> The exception. </value>
    public Exception? Exception { get; set; }

}
