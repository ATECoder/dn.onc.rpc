namespace cc.isr.ONC.RPC.Server;

/// <summary>
/// The server <see cref="OncRpcServerCallMessage"/> class represents an ONC/RPC call message on the
/// server side.
/// </summary>
/// <remarks>
/// For this reasons it just handles decoding of call messages but cannot do any encoding. This
/// class is also responsible for pulling off authentication information from the wire and
/// converting it into appropriate authentication protocol handling objects. As with all good
/// management, this class therefor delegates this somehow unpleasant work to the server- side
/// authentication protocol handling classes. <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public class OncRpcServerCallMessage : OncRpcCallMessageBase
{
    /// <summary>
    /// Decodes -- that is: deserializes -- a ONC/RPC message header object from a XDR stream
    /// according to RFC 1831.
    /// </summary>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="decoder">  A decoding XDR stream from which to receive all the mess. </param>
    public virtual void Decode( XdrDecodingStreamBase decoder )
    {
        this.MessageId = decoder.DecodeInt();

        // Make sure that we are really decoding an ONC/RPC message call
        // header. Otherwise, throw the appropriate OncRpcException exception.

        this.MessageType = ( OncRpcMessageType ) decoder.DecodeInt();
        if ( this.MessageType != OncRpcMessageType.OncRpcCallMessageType )
            throw new OncRpcException(
                $"; expected {nameof( OncRpcMessageType.OncRpcCallMessageType )}({OncRpcMessageType.OncRpcCallMessageType}); actual: {this.MessageType}",
                OncRpcExceptionReason.OncRpcWrongMessageType );

        // Make sure that the other side is talking the right slang --
        // we will only understand version 2 slang of ONC/RPC.

        this.ProtocolVersion = decoder.DecodeInt();
        if ( this.ProtocolVersion != OncRpcServerCallMessage.OncRpcProtocolVersion )
            throw new OncRpcException( $"; expected {nameof( OncRpcServerCallMessage.OncRpcProtocolVersion )}({OncRpcServerCallMessage.OncRpcProtocolVersion}); actual: {this.ProtocolVersion}",
                OncRpcExceptionReason.OncRpcClientServerVersionMismatch );

        // Now decode the remaining fields of the call header.

        this.Program = decoder.DecodeInt();
        this.Version = decoder.DecodeInt();
        this.Procedure = decoder.DecodeInt();

        // Last comes the authentication data. Note that the "factory" hidden
        // within xdrNew() will graciously recycle any old authentication
        // protocol handling object if it is of the same authentication type
        // as the new one just coming in from the XDR wire.

        this.Auth = OncRpcServerAuthBase.NewOncRpcServerAuth( decoder, this.Auth );
    }

    /// <summary>
    /// Gets or sets the authentication protocol handling object retrieved together with the call
    /// message itself. Defaults to <see cref="OncRpcServerAuthNone"/>
    /// </summary>
    /// <value> The authentication. </value>
    public OncRpcServerAuthBase Auth { get; set; } = new OncRpcServerAuthNone();

}
