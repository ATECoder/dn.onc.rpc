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
    /// <param name="xdr">  A decoding XDR stream from which to receive all the mess. </param>
    ///
    /// <exception cref="System.IO.IOException">    Thrown when an I/O error condition occurs. </exception>
    public virtual void Decode( XdrDecodingStreamBase xdr )
    {
        this.MessageId = xdr.DecodeInt();

        // Make sure that we are really decoding an ONC/RPC message call
        // header. Otherwise, throw the appropriate OncRpcException exception.

        this.MessageType = xdr.DecodeInt();
        if ( this.MessageType != OncRpcMessageType.OncRpcCallMessageType )
            throw new OncRpcException( OncRpcException.OncRpcWrongMessageType );

        // Make sure that the other side is talking the right slang --
        // we will only understand version 2 slang of ONC/RPC.

        this.ProtocolVersion = xdr.DecodeInt();
        if ( this.ProtocolVersion != OncRpcProtocolVersion )
            throw new OncRpcException( OncRpcException.OncRpcClientServerVersionMismatch );

        // Now decode the remaining fields of the call header.

        this.Program = xdr.DecodeInt();
        this.Version = xdr.DecodeInt();
        this.Procedure = xdr.DecodeInt();

        // Last comes the authentication data. Note that the "factory" hidden
        // within xdrNew() will graciously recycle any old authentication
        // protocol handling object if it is of the same authentication type
        // as the new one just coming in from the XDR wire.

        this.Auth = OncRpcServerAuthBase.NewOncRpcServerAuto( xdr, this.Auth );
    }

    /// <summary>
    /// Gets or sets the authentication protocol handling object retrieved together with the call
    /// message itself.
    /// </summary>
    /// <value> The authentication. </value>
    public OncRpcServerAuthBase Auth { get; set; }

}
