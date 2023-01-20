using cc.isr.ONC.RPC.EnumExtensions;

namespace cc.isr.ONC.RPC.Client;

/// <summary>
/// The <see cref="OncRpcReplyMessageBase"/> class represents an ONC/RPC reply message as defined by
/// ONC/RPC in RFC 1831.
/// </summary>
/// <remarks>
/// Such messages are sent back by ONC/RPC to servers to clients and contain
/// (in case of real success) the result of a remote procedure call. <para>
/// 
/// The decision to define only one single class for the accepted and
/// rejected replies was driven by the motivation not to use polymorphism and thus have to upcast
/// and downcast references all the time.  </para> <para>
/// 
/// The derived classes are only provided for convenience on the server
/// side.  </para><para>
/// 
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public class OncRpcClientReplyMessage : OncRpcReplyMessageBase
{

    /// <summary>
    /// Initializes a new <see cref="OncRpcReplyMessageBase"/> object to represent an invalid state.
    /// </summary>
    /// <remarks>
    /// This default constructor should only be used if in the next step the real state of the reply
    /// message is immediately decoded from a XDR stream.
    /// </remarks>
    /// <param name="auth"> Client-side authentication protocol handling object which is to be used
    ///                     when decoding the verifier data contained in the reply. </param>
    public OncRpcClientReplyMessage( OncRpcClientAuthBase auth ) : base()
    {
        this.Auth = auth;
    }

    /// <summary>
    /// Client-side authentication protocol handling object to use when decoding the reply message.
    /// </summary>
    /// <value> The authentication. </value>
    internal OncRpcClientAuthBase Auth { get; private set; }

    /// <summary>
    /// Decodes -- that is: deserializes -- a ONC/RPC message header object from a XDR stream.
    /// </summary>
    /// <exception cref="OncRpcAuthException">  Thrown when an ONC/RPC Authentication error condition occurs. </exception>
    /// <exception cref="OncRpcException">      Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="decoder">  The XDR decoding stream. </param>
    public virtual void Decode( XdrDecodingStreamBase decoder )
    {
        this.MessageId = decoder.DecodeInt();

        // Make sure that we are really decoding an ONC/RPC message call
        // header. Otherwise, throw the appropriate OncRpcException exception.

        this.MessageType = decoder.DecodeInt().ToMessageType();

        if ( this.MessageType != OncRpcMessageType.OncRpcReplyMessageType )
        {
            // set the reply status to indicate that the message was denied
            this.ReplyStatus = OncRpcReplyStatus.OncRpcMessageDenied;
            throw new OncRpcException(
                $"; expected {nameof( OncRpcMessageType.OncRpcReplyMessageType )}.{OncRpcMessageType.OncRpcReplyMessageType}({( int ) OncRpcMessageType.OncRpcReplyMessageType}); actual: {this.MessageType}({( int ) this.MessageType})",
                OncRpcExceptionReason.OncRpcWrongMessageType );
        }

        this.ReplyStatus = decoder.DecodeInt().ToReplyStatus();
        switch ( this.ReplyStatus )
        {
            case OncRpcReplyStatus.OncRpcMessageAccepted:
                {

                    // Decode the information returned for accepted message calls.
                    // If we have an associated client-side authentication protocol
                    // object, we use that. Otherwise we fall back to the default
                    // handling of only the 'none' authentication.

                    if ( this.Auth is not null )
                        this.Auth.DecodeVerfier( decoder );
                    else
                    {

                        // If we don't have a protocol handler and the server sent its
                        // reply using another authentication scheme than 'none', we
                        // will throw an exception. Also we check that no-one is
                        // actually sending opaque information within 'none'.

                        this.AuthType = decoder.DecodeInt().ToAuthType();
                        if ( this.AuthType != OncRpcAuthType.OncRpcAuthTypeNone )
                            throw new OncRpcAuthException(
                                $"; expected {nameof( OncRpcAuthType )}.{nameof( OncRpcAuthType.OncRpcAuthTypeNone )}({( int ) OncRpcAuthType.OncRpcAuthTypeNone}); actual: {this.AuthType}({( int ) this.AuthType})",
                                OncRpcAuthStatus.OncRpcAuthFailed );

                        // then check on the message value.

                        this.AuthLength = decoder.DecodeInt();
                        if ( this.AuthLength != OncRpcClientAuthNone.AuthMessageLengthDefault )
                            throw new OncRpcAuthException(
                                $"; expected {nameof( OncRpcClientAuthNone )}.{nameof( OncRpcClientAuthNone.AuthMessageLengthDefault )}({OncRpcClientAuthNone.AuthMessageLengthDefault}); actual: {this.AuthLength}",
                                OncRpcAuthStatus.OncRpcAuthFailed );
                    }

                    // Even if the call was accepted by the server, it can still
                    // indicate an error. Depending on the status of the accepted
                    // call we will receive an indication about the range of
                    // versions a particular program (server) supports.

                    this.AcceptStatus = decoder.DecodeInt().ToAcceptStatus();
                    switch ( this.AcceptStatus )
                    {
                        case OncRpcAcceptStatus.OncRpcProgramVersionMismatch:
                            {
                                this.LowVersion = decoder.DecodeInt();
                                this.HighVersion = decoder.DecodeInt();
                                break;
                            }

                        default:
                            {

                                // Otherwise "open ended set of problem", like the author
                                // of Sun's ONC/RPC source once wrote...

                                break;
                            }
                    }
                    break;
                }

            case OncRpcReplyStatus.OncRpcMessageDenied:
                {

                    // Encode the information returned for denied message calls.

                    this.RejectStatus = decoder.DecodeInt().ToRejectStatus();
                    switch ( this.RejectStatus )
                    {
                        case OncRpcRejectStatus.OncRpcWrongProtocolVersion:
                            {
                                this.LowVersion = decoder.DecodeInt();
                                this.HighVersion = decoder.DecodeInt();
                                break;
                            }

                        case OncRpcRejectStatus.OncRpcAuthError:
                            {
                                this.AuthStatus = decoder.DecodeInt().ToAuthStatus();
                                break;
                            }

                        default:
                            {
                                break;
                            }
                    }
                    break;
                }
        }
    }

}
