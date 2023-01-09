namespace cc.isr.ONC.RPC.Client;

/// <summary>
/// The <see cref="OncRpcReplyMessageBase"/> class represents an ONC/RPC reply message as defined by
/// ONC/RPC in RFC 1831.
/// </summary>
/// <remarks>
/// Such messages are sent back by ONC/RPC to servers to clients and contain
/// (in case of real success) the result of a remote procedure call. <para>
/// The decision to define only one single class for the accepted and
/// rejected replies was driven by the motivation not to use polymorphism and thus have to upcast
/// and downcast references all the time.  </para> <para>
/// The derived classes are only provided for convenience on the server
/// side.  </para><para>
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
    /// Check whether this <see cref="OncRpcReplyMessageBase"/> represents an accepted and successfully
    /// executed remote procedure call.
    /// </summary>
    /// <returns>
    /// <see cref="T:true"/> if remote procedure call was accepted and
    /// successfully executed.
    /// </returns>
    public virtual bool SuccessfullyAccepted()
    {
        return this.ReplyStatus == OncRpcReplyStatus.OncRpcMessageAccepted && this.AcceptStatus == OncRpcAcceptStatus.OncRpcSuccess;
    }

    /// <summary>
    /// Return an appropriate exception object according to the state this reply message header
    /// object is in.
    /// </summary>
    /// <returns>
    /// Exception object of class
    /// <see cref="OncRpcException"/>
    /// or a subclass thereof.
    /// </returns>
    public virtual OncRpcException NewException()
    {
        switch ( this.ReplyStatus )
        {
            case OncRpcReplyStatus.OncRpcMessageAccepted:
                {
                    switch ( this.AcceptStatus )
                    {
                        case OncRpcAcceptStatus.OncRpcSuccess:
                            {
                                return new OncRpcException( OncRpcExceptionReason.OncRpcSuccess );
                            }

                        case OncRpcAcceptStatus.OncRpcProcedureNotAvailable:
                            {
                                return new OncRpcException( OncRpcExceptionReason.OncRpcProcedureNotAvailable );
                            }

                        case OncRpcAcceptStatus.OncRpcProgramVersionMismatch:
                            {
                                return new OncRpcException( OncRpcExceptionReason.OncRpcProgramVersionNotSupported );
                            }

                        case OncRpcAcceptStatus.OncRpcProgramNotAvailable:
                            {
                                return new OncRpcException( OncRpcExceptionReason.OncRpcProgramNotAvailable );
                            }

                        case OncRpcAcceptStatus.OncRpcUnableToDecodingArguments:
                            {
                                return new OncRpcException( OncRpcExceptionReason.OncRpcCannotDecodeArgs );
                            }

                        case OncRpcAcceptStatus.OncRpcSystemError:
                            {
                                return new OncRpcException( OncRpcExceptionReason.OncRpcSystemError );
                            }
                    }
                    break;
                }

            case OncRpcReplyStatus.OncRpcMessageDenied:
                {
                    switch ( this.RejectStatus )
                    {
                        case OncRpcRejectStatus.OncRpcAuthError:
                            {
                                return new OncRpcAuthException( this.AuthStatus );
                            }

                        case OncRpcRejectStatus.OncRpcWrongProtocolVersion:
                            {
                                return new OncRpcException( OncRpcExceptionReason.OncRpcFailed );
                            }
                    }
                    break;
                }
        }
        return new OncRpcException();
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- a ONC/RPC message header object from a XDR stream.
    /// </summary>
    /// <param name="decoder">  The XDR decoding stream. </param>
    ///
    /// <exception cref="OncRpcException">                  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="OncRpcAuthException">    Thrown when an ONC/RPC Authentication
    ///                                                     error condition occurs. </exception>
    /// <exception cref="System.IO.IOException">            Thrown when an I/O error condition occurs. </exception>
    public virtual void Decode( XdrDecodingStreamBase decoder )
    {
        this.MessageId = decoder.DecodeInt();

        // Make sure that we are really decoding an ONC/RPC message call
        // header. Otherwise, throw the appropriate OncRpcException exception.

        this.MessageType = ( OncRpcMessageType ) decoder.DecodeInt();
        if ( this.MessageType != OncRpcMessageType.OncRpcReplyMessageType )
            throw new OncRpcException(
                $"; expected {nameof( OncRpcMessageType.OncRpcCallMessageType )}({OncRpcMessageType.OncRpcCallMessageType}); actual: {this.MessageType}",
                OncRpcExceptionReason.OncRpcWrongMessageType );
        this.ReplyStatus = decoder.DecodeInt();
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
                        int replyAuthType = decoder.DecodeInt();
                        if ( replyAuthType != ( int ) OncRpcAuthType.OncRpcAuthTypeNone )
                            throw new OncRpcAuthException(
                                $"; expected {nameof( OncRpcAuthType.OncRpcAuthTypeNone )}({OncRpcAuthType.OncRpcAuthTypeNone}); actual: {replyAuthType}",
                                OncRpcAuthStatus.OncRpcAuthFailed );
                        // then check on the message value.
                        int replyAuthLength = decoder.DecodeInt();
                        if ( replyAuthLength != OncRpcClientAuthNone.AuthMessageLength )
                            throw new OncRpcAuthException(
                                $"; expected {nameof( OncRpcClientAuthNone.AuthMessageLength )}({OncRpcClientAuthNone.AuthMessageLength}); actual: {replyAuthLength}",
                                OncRpcAuthStatus.OncRpcAuthFailed );
                    }

                    // Even if the call was accepted by the server, it can still
                    // indicate an error. Depending on the status of the accepted
                    // call we will receive an indication about the range of
                    // versions a particular program (server) supports.

                    this.AcceptStatus = ( OncRpcAcceptStatus ) decoder.DecodeInt();
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

                    this.RejectStatus = decoder.DecodeInt();
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
                                this.AuthStatus = ( OncRpcAuthStatus ) decoder.DecodeInt();
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

    /// <summary>
    /// Client-side authentication protocol handling object to use when decoding the reply message.
    /// </summary>
    /// <value> The authentication. </value>
    internal OncRpcClientAuthBase Auth { get; private set; }
}
