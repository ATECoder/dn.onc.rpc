namespace cc.isr.ONC.RPC.Server;

/// <summary>
/// The <see cref="OncRpcReplyMessageBase"/> class represents an ONC/RPC reply message as defined by
/// ONC/RPC in RFC 1831.
/// </summary>
/// <remarks>
/// Such messages are sent back by ONC/RPC to servers to clients and contain
/// (in case of real success) the result of a remote procedure call. <para>
/// 
/// This class and all its derived classes can be encoded only. They are
/// not able to encode themselves, because they are used solely on the server side of an ONC/RPC
/// connection. </para> <para>
/// 
/// The decision to define only one single class for the accepted and
/// rejected replies was driven by the motivation not to use polymorphism and thus have to upcast
/// and downcast references all the time.  </para> <para>
/// 
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public class OncRpcServerReplyMessage : OncRpcReplyMessageBase
{
    /// <summary>
    /// Initializes a new <see cref="OncRpcReplyMessageBase"/> object and initializes its complete state
    /// from the given parameters.
    /// </summary>
    /// <remarks>
    /// Note that depending on the reply, acceptance and reject status some parameters are unused and
    /// can be specified as server <see cref="OncRpcReplyMessageBase.UnusedMessageParameter"/>.
    /// </remarks>
    /// <param name="call">         The ONC/RPC call this reply message corresponds to. </param>
    /// <param name="replyStatus">  The reply status <see cref="OncRpcReplyStatus"/>. </param>
    /// <param name="acceptStatus"> The acceptance status <see cref="OncRpcAcceptStatus"/>. </param>
    /// <param name="rejectStatus"> The rejection status <see cref="OncRpcRejectStatus"/>. </param>
    /// <param name="lowVersion">   lowest supported version. </param>
    /// <param name="highVersion">  highest supported version. </param>
    /// <param name="authStatus">   The authentication status (<see cref="OncRpcAuthStatus"/>). </param>
    public OncRpcServerReplyMessage( OncRpcServerCallMessage call, OncRpcReplyStatus replyStatus,
                                     OncRpcAcceptStatus acceptStatus, OncRpcRejectStatus rejectStatus,
                                     int lowVersion, int highVersion, OncRpcAuthStatus authStatus ) : base( call, replyStatus, acceptStatus,
                                                                                                            rejectStatus, lowVersion, highVersion, authStatus )
    {
        this.Auth = call.Auth;
    }

    /// <summary>
    /// Encodes -- that is: serializes -- a ONC/RPC reply header object into a XDR stream.
    /// </summary>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="encoder">  An encoding XDR stream. </param>
    public virtual void Encode( XdrEncodingStreamBase encoder )
    {
        encoder.EncodeInt( this.MessageId );
        encoder.EncodeInt( ( int ) this.MessageType );
        encoder.EncodeInt( ( int ) this.ReplyStatus );
        switch ( this.ReplyStatus )
        {
            case OncRpcReplyStatus.OncRpcMessageAccepted:
                {

                    // Encode the information returned for accepted message calls.

                    // First encode the authentication data. If someone has
                    // nulled (nuked?) the authentication protocol handling object
                    // from the call information object, then we can still fall back
                    // to sending 'none' replies...

                    if ( this.Auth is not null )
                        this.Auth.EncodeVerfier( encoder );
                    else
                    {
                        // encode the authentication type as none as set length to zero.
                        encoder.EncodeInt( ( int ) OncRpcServerAuthNone.VerifierAuthTypeDefault );
                        encoder.EncodeInt( OncRpcServerAuthNone.VerifierAuthMessageLengthDefault );
                    }

                    // Even if the call was accepted by the server, it can still
                    // indicate an error. Depending on the status of the accepted
                    // call we have to Sends back an indication about the range of
                    // versions we support of a particular program (server).

                    encoder.EncodeInt( ( int ) this.AcceptStatus );
                    switch ( this.AcceptStatus )
                    {
                        case OncRpcAcceptStatus.OncRpcProgramVersionMismatch:
                            {
                                encoder.EncodeInt( this.LowVersion );
                                encoder.EncodeInt( this.HighVersion );
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

                    encoder.EncodeInt( ( int ) this.RejectStatus );
                    switch ( this.RejectStatus )
                    {
                        case OncRpcRejectStatus.OncRpcWrongProtocolVersion:
                            {
                                encoder.EncodeInt( this.LowVersion );
                                encoder.EncodeInt( this.HighVersion );
                                break;
                            }

                        case OncRpcRejectStatus.OncRpcAuthError:
                            {
                                encoder.EncodeInt( ( int ) this.AuthStatus );
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

    /// <summary>   gets or sets the authentication protocol handling object. 
    /// defaults to <see cref="OncRpcServerAuthNone"/> </summary>
    /// <value> The authentication. </value>
    internal OncRpcServerAuthBase Auth { get; private set; } = new OncRpcServerAuthNone();
}
