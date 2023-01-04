using cc.isr.XDR;

namespace cc.isr.ONC.RPC.Server;

/// <summary>
/// Objects of class <see cref="OncRpcCallInformation"/> contain information about individual
/// ONC/RPC calls.
/// </summary>
/// <remarks>
/// They are given to ONC/RPC <see cref="IOncRpcDispatchable">call dispatchers</see>,
/// so they can Sends back the reply to the appropriate caller, etc. Use only this call info
/// objects to retrieve call parameters and Sends back replies as in the future UDP/IP-based
/// transports may become multi-threaded handling. The call info object is responsible to control
/// access to the underlaying transport, so never mess with the transport directly. <para>
/// Note that this class provides two different patterns for accessing
/// parameters sent by clients within the ONC/RPC call and sending back replies. </para>
/// <list type="bullet">The convenient high-level access: <item>
/// Use<see cref="RetrieveCall(IXdrCodec)"/>
/// to retrieve the parameters of the call and deserialize it into a parameter object.</item><item>
/// Use <see cref="Reply(IXdrCodec)"/>
/// to Sends back the reply by serializing a reply/result object. Or use the <i>Fail*</i>
/// methods to Sends back an error indication instead. </item><item>
/// The lower-level access, giving more control over how and when data is deserialized and
/// serialized: </item></list><list type="bullet"><item>
/// Use <see cref="GetXdrDecodingStream()"/> to get a reference to the XDR stream from which you
/// can deserialize the call's parameter.</item> <item>
/// When you are finished deserializing, call <see cref="EndDecoding()"/>. </item><item>
/// To Sends back the reply/result, call <see cref="BeginEncoding(OncRpcServerReplyMessage)"/>.
/// Using the XDR stream returned by <see cref="GetXdrEncodingStream()"/>
/// serialize the reply/result. Finally finish the serializing step by calling
/// <see cref="EndEncoding()"/>. </item></list><para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public class OncRpcCallInformation
{
    /// <summary>
    /// Create an <see cref="OncRpcCallInformation"/> object and associate it with a ONC/RPC server
    /// transport.
    /// </summary>
    /// <remarks>
    /// Typically, <see cref="OncRpcCallInformation"/> objects are created by transports
    /// once before handling incoming calls using the same call info object.
    /// To support multi-threaded handling of calls in the future (for UDP/IP),
    /// the transport is already divided from the call info.
    /// </remarks>
    /// <param name="transport">    ONC/RPC server transport. </param>
    internal OncRpcCallInformation( OncRpcServerTransportBase transport )
    {
        this.Transport = transport;
    }

    /// <summary>
    /// Gets or set the call message header from ONC/RPC identifying this particular call.
    /// </summary>
    /// <value> A call message header from ONC/RPC identifying this particular call. </value>
    public OncRpcServerCallMessage CallMessage { get; set; } = new();

    /// <summary>
    /// Gets or sets the Internet address of the peer from which we received an ONC/RPC call or whom
    /// we intend to call.
    /// </summary>
    /// <value> The peer IP address. </value>
    public IPAddress PeerIPAddress { get; set; } = null;

    /// <summary>
    /// Gets or sets the port number of the peer from which we received an ONC/RPC call or whom we intend to call.
    /// </summary>
    /// <value> The peer port number. </value>
    public int PeerPort { get; set; } = 0;

    /// <summary>
    /// Gets or sets the associated transport from which we receive the ONC/RPC call parameters and
    /// to which we serialize the ONC/RPC reply. Never mess with this member or you might break all
    /// future extensions horribly -- but this warning probably only stimulates you...
    /// </summary>
    /// <value> The transport. </value>
    internal OncRpcServerTransportBase Transport { get; set; }

    /// <summary>   Retrieves the parameters sent within an ONC/RPC call message. </summary>
    /// <remarks>
    /// It also makes sure that the deserialization process is properly finished after the call
    /// parameters have been retrieved.
    /// </remarks>
    /// <param name="call"> The call. </param>
    ///
    /// <exception cref="OncRpcException">          if an ONC/RPC exception occurs, like the data
    ///                                             could not be successfully deserialized. </exception>
    /// <exception cref="System.IO.IOException">    if an I/O exception occurs, like transmission
    ///                                             failures over the network, etc. </exception>
    public virtual void RetrieveCall( IXdrCodec call )
    {
        this.Transport.RetrieveCall( call );
    }

    /// <summary>
    /// Returns XDR stream which can be used for deserializing the parameters of this ONC/RPC call.
    /// This method belongs to the lower-level access pattern when handling ONC/RPC calls.
    /// </summary>
    /// <returns>   Reference to decoding XDR stream. </returns>
    public virtual XdrDecodingStreamBase GetXdrDecodingStream()
    {
        return this.Transport.GetXdrDecodingStream();
    }

    /// <summary>   Finishes call parameter deserialization. </summary>
    /// <remarks>
    /// Afterwards the XDR stream returned by <see cref="GetXdrDecodingStream()"/>
    /// must not be used any more. This method belongs to the lower-level access pattern when
    /// handling ONC/RPC calls.
    /// </remarks>
    ///
    /// <exception cref="OncRpcException">          if an ONC/RPC exception occurs, like the data
    ///                                             could not be successfully deserialized. </exception>
    /// <exception cref="System.IO.IOException">    if an I/O exception occurs, like transmission
    ///                                             failures over the network, etc. </exception>
    public virtual void EndDecoding()
    {
        this.Transport.EndDecoding();
    }

    /// <summary>   Begins the sending phase for ONC/RPC replies. </summary>
    /// <remarks>
    /// After beginning sending you can serialize the reply/result (but only if the call was accepted,
    /// see <see cref="OncRpcReplyMessageBase"/>
    /// for details). The stream to use for serialization can be obtained using
    /// <see cref="GetXdrEncodingStream()"/>.
    /// This method belongs to the lower-level access pattern when handling ONC/RPC calls.
    /// </remarks>
    /// <param name="state">    ONC/RPC reply header indicating success or failure. </param>
    ///
    /// <exception cref="OncRpcException">          if an ONC/RPC exception occurs, like the data
    ///                                             could not be successfully deserialized. </exception>
    /// <exception cref="System.IO.IOException">    if an I/O exception occurs, like transmission
    ///                                             failures over the network, etc. </exception>
    public virtual void BeginEncoding( OncRpcServerReplyMessage state )
    {
        this.Transport.BeginEncoding( this, state );
    }

    /// <summary>   Begins the sending phase for accepted ONC/RPC replies. </summary>
    /// <remarks>
    /// After beginning sending you can serialize the result/reply. The stream to use for
    /// serialization can be obtained using <see cref="GetXdrEncodingStream()"/>.
    /// This method belongs to the lower-level access pattern when handling ONC/RPC calls.
    /// </remarks>
    ///
    /// <exception cref="OncRpcException">          if an ONC/RPC exception occurs, like the data
    ///                                             could not be successfully deserialized. </exception>
    /// <exception cref="System.IO.IOException">    if an I/O exception occurs, like transmission
    ///                                             failures over the network, etc. </exception>
    public virtual void BeginEncoding()
    {
        this.Transport.BeginEncoding( this, new OncRpcServerReplyMessage( this.CallMessage, OncRpcReplyStatus.OncRpcMessageAccepted,
            OncRpcAcceptStatus.OncRpcSuccess, OncRpcReplyMessageBase.UnusedMessageParameter, OncRpcReplyMessageBase.UnusedMessageParameter,
            OncRpcReplyMessageBase.UnusedMessageParameter, OncRpcReplyMessageBase.UnusedMessageParameter ) );
    }

    /// <summary>
    /// Returns XDR stream which can be used for serializing the reply to this ONC/RPC call.
    /// </summary>
    /// <remarks>
    /// This method belongs to the lower-level access pattern when handling ONC/RPC calls.
    /// </remarks>
    /// <returns>   Reference to encoding XDR stream. </returns>
    public virtual XdrEncodingStreamBase GetXdrEncodingStream()
    {
        return this.Transport.GetXdrEncodingStream();
    }

    /// <summary>   Finishes encoding the reply to this ONC/RPC call. </summary>
    /// <remarks>
    /// Afterwards you must not use the XDR stream returned by <see cref="GetXdrEncodingStream()"/> any longer.
    /// </remarks>
    ///
    /// <exception cref="OncRpcException">          if an ONC/RPC exception occurs, like the data
    ///                                             could not be successfully deserialized. </exception>
    /// <exception cref="System.IO.IOException">    if an I/O exception occurs, like transmission
    ///                                             failures over the network, etc. </exception>
    public virtual void EndEncoding()
    {
        this.Transport.EndEncoding();
    }

    /// <summary> Sends back an ONC/RPC reply to the caller who sent in this call. </summary>
    /// <remarks>
    /// This is a low-level function and typically should not be used by call dispatchers. Instead
    /// use the other <see cref="Reply(IXdrCodec)">reply method</see> which just expects a serializable
    /// object to Sends back to the caller.
    /// </remarks>
    /// <param name="state">    ONC/RPC reply message header indicating success or failure and
    ///                         containing associated state information. </param>
    /// <param name="reply">    If not <see langword="null"/>, then this parameter references the reply to
    ///                         be serialized after the reply message header. </param>
    ///
    /// <exception cref="OncRpcException">          if an ONC/RPC exception occurs, like the data
    ///                                             could not be successfully deserialized. </exception>
    /// <exception cref="System.IO.IOException">    if an I/O exception occurs, like transmission
    ///                                             failures over the network, etc. </exception>
    public virtual void Reply( OncRpcServerReplyMessage state, IXdrCodec reply )
    {
        this.Transport.Reply( this, state, reply );
    }

    /// <summary> Sends back an ONC/RPC reply to the caller who sent in this call. </summary>
    /// <remarks>
    /// This automatically sends an ONC/RPC reply header before the reply part, indicating success
    /// within the header.
    /// </remarks>
    /// <param name="rply"> Reply body the ONC/RPC reply message. </param>
    ///
    /// <exception cref="OncRpcException">          if an ONC/RPC exception occurs, like the data
    ///                                             could not be successfully deserialized. </exception>
    /// <exception cref="System.IO.IOException">    if an I/O exception occurs, like transmission
    ///                                             failures over the network, etc. </exception>
    public virtual void Reply( IXdrCodec rply )
    {
        this.Reply( new OncRpcServerReplyMessage( this.CallMessage, OncRpcReplyStatus.OncRpcMessageAccepted, OncRpcAcceptStatus.OncRpcSuccess,
            OncRpcReplyMessageBase.UnusedMessageParameter, OncRpcReplyMessageBase.UnusedMessageParameter, OncRpcReplyMessageBase.UnusedMessageParameter,
            OncRpcReplyMessageBase.UnusedMessageParameter ), rply );
    }

    /// <summary>
    /// Sends back an ONC/RPC failure indication about invalid arguments to the caller who sent in
    /// this call reporting <see cref="OncRpcAcceptStatus.OncRpcUnableToDecodingArguments"/>
    /// </summary>
    ///
    /// <exception cref="OncRpcException">          if an ONC/RPC exception occurs, like the data
    ///                                             could not be successfully deserialized. </exception>
    /// <exception cref="System.IO.IOException">    if an I/O exception occurs, like transmission
    ///                                             failures over the network, etc. </exception>
    public virtual void ReplyUnableToDecodingArguments()
    {
        this.Reply( new OncRpcServerReplyMessage( this.CallMessage, OncRpcReplyStatus.OncRpcMessageAccepted, OncRpcAcceptStatus.OncRpcUnableToDecodingArguments,
            OncRpcReplyMessageBase.UnusedMessageParameter, OncRpcReplyMessageBase.UnusedMessageParameter, OncRpcReplyMessageBase.UnusedMessageParameter,
            OncRpcReplyMessageBase.UnusedMessageParameter ), null );
    }

    /// <summary>
    /// Sends back an ONC/RPC failure indication about an unavailable procedure call to the caller who
    /// sent in this call reporting <see cref="OncRpcAcceptStatus.OncRpcProcedureNotAvailable"/>
    /// </summary>
    ///
    /// <exception cref="OncRpcException">          if an ONC/RPC exception occurs, like the data
    ///                                             could not be successfully deserialized. </exception>
    /// <exception cref="System.IO.IOException">    if an I/O exception occurs, like transmission
    ///                                             failures over the network, etc. </exception>
    public virtual void ReplyProcedureNotAvailable()
    {
        this.Reply( new OncRpcServerReplyMessage( this.CallMessage, OncRpcReplyStatus.OncRpcMessageAccepted, OncRpcAcceptStatus.OncRpcProcedureNotAvailable,
            OncRpcReplyMessageBase.UnusedMessageParameter, OncRpcReplyMessageBase.UnusedMessageParameter, OncRpcReplyMessageBase.UnusedMessageParameter,
            OncRpcReplyMessageBase.UnusedMessageParameter ), null );
    }

    /// <summary>
    /// Sends back an ONC/RPC failure indication about an unavailable program to the caller who sent
    /// in this call sending <see cref="OncRpcAcceptStatus.OncRpcProgramNotAvailable"/>
    /// </summary>
    ///
    /// <exception cref="OncRpcException">          if an ONC/RPC exception occurs, like the data
    ///                                             could not be successfully deserialized. </exception>
    /// <exception cref="System.IO.IOException">    if an I/O exception occurs, like transmission
    ///                                             failures over the network, etc. </exception>
    public virtual void ReplyProgramNotAvailable()
    {
        this.Reply( new OncRpcServerReplyMessage( this.CallMessage, OncRpcReplyStatus.OncRpcMessageAccepted, OncRpcAcceptStatus.OncRpcProgramNotAvailable,
            OncRpcReplyMessageBase.UnusedMessageParameter, OncRpcReplyMessageBase.UnusedMessageParameter, OncRpcReplyMessageBase.UnusedMessageParameter,
            OncRpcReplyMessageBase.UnusedMessageParameter ), null );
    }

    /// <summary>
    /// Sends back an ONC/RPC failure indication about a program version mismatch to the caller who
    /// sent in this call replying <see cref="OncRpcAcceptStatus.OncRpcProgramVersionMismatch"/>
    /// </summary>
    /// <param name="lowVersion">   lowest supported program version. </param>
    /// <param name="highVersion">  highest supported program version. </param>
    ///
    /// <exception cref="OncRpcException">          if an ONC/RPC exception occurs, like the data
    ///                                             could not be successfully deserialized. </exception>
    /// <exception cref="System.IO.IOException">    if an I/O exception occurs, like transmission
    ///                                             failures over the network, etc. </exception>
    public virtual void ReplyProgramVersionMismatch( int lowVersion, int highVersion )
    {
        this.Reply( new OncRpcServerReplyMessage( this.CallMessage, OncRpcReplyStatus.OncRpcMessageAccepted, OncRpcAcceptStatus.OncRpcProgramVersionMismatch,
            OncRpcReplyMessageBase.UnusedMessageParameter, lowVersion, highVersion, OncRpcReplyMessageBase.UnusedMessageParameter ), null );
    }

    /// <summary>
    /// Sends back an ONC/RPC failure indication about a system error to the caller who sent in this
    /// call replying <see cref="OncRpcAcceptStatus.OncRpcSystemError"/>
    /// </summary>
    ///
    /// <exception cref="OncRpcException">          if an ONC/RPC exception occurs, like the data
    ///                                             could not be successfully deserialized. </exception>
    /// <exception cref="System.IO.IOException">    if an I/O exception occurs, like transmission
    ///                                             failures over the network, etc. </exception>
    public virtual void ReplySystemError()
    {
        this.Reply( new OncRpcServerReplyMessage( this.CallMessage, OncRpcReplyStatus.OncRpcMessageAccepted, OncRpcAcceptStatus.OncRpcSystemError,
            OncRpcReplyMessageBase.UnusedMessageParameter, OncRpcReplyMessageBase.UnusedMessageParameter, OncRpcReplyMessageBase.UnusedMessageParameter,
            OncRpcReplyMessageBase.UnusedMessageParameter ), null );
    }

    /// <summary>
    /// Sends back an ONC/RPC failure indication about a ONC/RPC version mismatch call to the caller
    /// who sent in this call replying <see cref="OncRpcRejectStatus.OncRpcWrongProtocolVersion"/>
    /// </summary>
    ///
    /// <exception cref="OncRpcException">          if an ONC/RPC exception occurs, like the data
    ///                                             could not be successfully deserialized. </exception>
    /// <exception cref="System.IO.IOException">    if an I/O exception occurs, like transmission
    ///                                             failures over the network, etc. </exception>
    public virtual void RerplyWrongProtocolVersion()
    {
        this.Reply( new OncRpcServerReplyMessage( this.CallMessage, OncRpcReplyStatus.OncRpcMessageDenied, OncRpcReplyMessageBase.UnusedMessageParameter,
            OncRpcRejectStatus.OncRpcWrongProtocolVersion, OncRpcCallMessageBase.OncRpcProtocolVersion, OncRpcCallMessageBase.OncRpcProtocolVersion,
            OncRpcReplyMessageBase.UnusedMessageParameter ), null );
    }

    /// <summary>
    /// Sends back an ONC/RPC failure indication about a failed authentication to the caller who sent
    /// in this call replying <see cref="OncRpcRejectStatus.OncRpcAuthError"/>
    /// </summary>
    /// <param name="authStatus">   <see cref="OncRpcAuthStatus">Reason</see>
    ///                             why authentication failed. </param>
    ///
    /// <exception cref="OncRpcException">          if an ONC/RPC exception occurs, like the data
    ///                                             could not be successfully deserialized. </exception>
    /// <exception cref="System.IO.IOException">    if an I/O exception occurs, like transmission
    ///                                             failures over the network, etc. </exception>
    public virtual void ReplyAuthError( int authStatus )
    {
        this.Reply( new OncRpcServerReplyMessage( this.CallMessage, OncRpcReplyStatus.OncRpcMessageDenied, OncRpcReplyMessageBase.UnusedMessageParameter,
            OncRpcRejectStatus.OncRpcAuthError, OncRpcReplyMessageBase.UnusedMessageParameter, OncRpcReplyMessageBase.UnusedMessageParameter, authStatus ), null );
    }
}
