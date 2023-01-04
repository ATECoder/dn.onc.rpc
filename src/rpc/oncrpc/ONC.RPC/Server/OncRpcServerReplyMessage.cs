namespace cc.isr.ONC.RPC.Server;

/// <summary>
/// The <see cref="OncRpcReplyMessageBase"/> class represents an ONC/RPC reply message as defined by
/// ONC/RPC in RFC 1831.
/// </summary>
/// <remarks>
/// Such messages are sent back by ONC/RPC to servers to clients and contain
/// (in case of real success) the result of a remote procedure call. <para>
/// This class and all its derived classes can be encoded only. They are
/// not able to encode themselves, because they are used solely on the server side of an ONC/RPC
/// connection. </para> <para>
/// The decision to define only one single class for the accepted and
/// rejected replies was driven by the motivation not to use polymorphism and thus have to upcast
/// and downcast references all the time.  </para> <para>
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
    /// <param name="replyStatus">  The reply status (see <see cref="OncRpcReplyStatus"/>). </param>
    /// <param name="acceptStatus"> The acceptance state (<see cref="OncRpcAcceptStatus"/>). </param>
    /// <param name="rejectStatus"> The rejection state (<see cref="OncRpcRejectStatus"/>). </param>
    /// <param name="lowVersion">   lowest supported version. </param>
    /// <param name="highVersion">  highest supported version. </param>
    /// <param name="authStatus">   The authentication state (<see cref="OncRpcAuthStatus"/>). </param>
    public OncRpcServerReplyMessage( OncRpcServerCallMessage call, int replyStatus, int acceptStatus, int rejectStatus,
        int lowVersion, int highVersion, int authStatus ) : base( call, replyStatus, acceptStatus, rejectStatus, lowVersion, highVersion, authStatus )
    {
        this.Auth = call.Auth;
    }

    /// <summary>
    /// Encodes -- that is: serializes -- a ONC/RPC reply header object into a XDR stream.
    /// </summary>
    /// <param name="xdr">  An encoding XDR stream. </param>
    ///
    /// <exception cref="OncRpcException">          Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="System.IO.IOException">    Thrown when an I/O error condition occurs. </exception>
    public virtual void Encode( XdrEncodingStreamBase xdr )
    {
        xdr.EncodeInt( this.MessageId );
        xdr.EncodeInt( this.MessageType );
        xdr.EncodeInt( this.ReplyStatus );
        switch ( this.ReplyStatus )
        {
            case OncRpcReplyStatus.OncRpcMessageAccepted:
                {

                    // Encode the information returned for accepted message calls.

                    // First encode the authentication data. If someone has
                    // nulled (nuked?) the authentication protocol handling object
                    // from the call information object, then we can still fall back
                    // to sending 'none' replies...

                    if ( this.Auth != null )
                        this.Auth.EncodeVerfier( xdr );
                    else
                    {
                        xdr.EncodeInt( OncRpcAuthType.OncRpcAuthTypeNone );
                        xdr.EncodeInt( 0 );
                    }

                    // Even if the call was accepted by the server, it can still
                    // indicate an error. Depending on the status of the accepted
                    // call we have to Sends back an indication about the range of
                    // versions we support of a particular program (server).

                    xdr.EncodeInt( this.AcceptStatus );
                    switch ( this.AcceptStatus )
                    {
                        case OncRpcAcceptStatus.OncRpcProgramVersionMismatch:
                            {
                                xdr.EncodeInt( this.LowVersion );
                                xdr.EncodeInt( this.HighVersion );
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

                    xdr.EncodeInt( this.RejectStatus );
                    switch ( this.RejectStatus )
                    {
                        case OncRpcRejectStatus.OncRpcWrongProtocolVersion:
                            {
                                xdr.EncodeInt( this.LowVersion );
                                xdr.EncodeInt( this.HighVersion );
                                break;
                            }

                        case OncRpcRejectStatus.OncRpcAuthError:
                            {
                                xdr.EncodeInt( this.AuthStatus );
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

    /// <summary>   gets or sets the authentication protocol handling object. </summary>
    /// <value> The authentication. </value>
    internal OncRpcServerAuthBase Auth { get; private set; }
}
