namespace cc.isr.ONC.RPC;

/// <summary>
/// The <see cref="OncRpcReplyMessageBase"/> class represents an ONC/RPC reply message as defined by
/// ONC/RPC in RFC 1831.
/// </summary>
/// <remarks>
/// Such messages are sent back by ONC/RPC to servers to clients and contain (in case of real
/// success) the result of a remote procedure call. <para>
///  
/// The decision to define only one single class for the accepted and
/// rejected replies was driven by the motivation not to use polymorphism and thus have to upcast
/// and downcast references all the time. </para> <para>
/// 
/// The derived classes are only provided for convenience on the server
/// side. </para> <para>
/// 
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public abstract class OncRpcReplyMessageBase : OncRpcMessageBase
{

    #region " Construction and Cleanup "

    /// <summary>
    /// (Immutable)
    /// Dummy, which can be used to identify unused parameters when constructing
    /// <see cref="OncRpcReplyMessageBase"/> objects.
    /// </summary>
    public const int UnusedMessageParameter = 0;

    /// <summary>
    /// Initializes a new <see cref="OncRpcReplyMessageBase"/> object to represent an invalid state.
    /// </summary>
    /// <remarks>
    /// This default constructor should only be used if in the next step the real state of the reply
    /// message is immediately decoded from a XDR stream.
    /// </remarks>
    public OncRpcReplyMessageBase() : base( OncRpcMessageBase.MessageIdDefault )
    {
        this.MessageType = OncRpcMessageType.OncRpcReplyMessageType;
        this.ReplyStatus = OncRpcReplyStatus.OncRpcMessageAccepted;
        this.AcceptStatus = OncRpcAcceptStatus.OncRpcSystemError;
        this.RejectStatus = OncRpcReplyMessageBase.UnusedMessageParameter;
        this.LowVersion = 0;
        this.HighVersion = 0;
        this.AuthStatus = OncRpcReplyMessageBase.UnusedMessageParameter;
    }

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
    /// <param name="rejectStatus"> The rejection state <see cref="OncRpcRejectStatus"/>. </param>
    /// <param name="lowVersion">   lowest supported version. </param>
    /// <param name="highVersion">  highest supported version. </param>
    /// <param name="authStatus">   The authentication state <see cref="OncRpcAuthStatus"/>. </param>
    public OncRpcReplyMessageBase( OncRpcCallMessageBase call, OncRpcReplyStatus replyStatus, OncRpcAcceptStatus acceptStatus,
                                   OncRpcRejectStatus rejectStatus, int lowVersion, int highVersion,
                                   OncRpcAuthStatus authStatus ) : base( call.MessageId )
    {
        this.MessageType = OncRpcMessageType.OncRpcReplyMessageType;
        this.ReplyStatus = replyStatus;
        this.AcceptStatus = acceptStatus;
        this.RejectStatus = rejectStatus;
        this.LowVersion = lowVersion;
        this.HighVersion = highVersion;
        this.AuthStatus = authStatus;
    }

    #endregion

    #region " members "

    /// <summary>
    /// The reply status of the reply message. This can be either
    /// <see cref="OncRpcReplyStatus.OncRpcMessageAccepted"/>
    /// or
    /// <see cref="OncRpcReplyStatus.OncRpcMessageDenied"/>.
    /// Depending on the value of this field, other fields of an instance of
    /// <see cref="OncRpcReplyMessageBase"/> become important. <para>
    /// 
    /// The decision to define only one single class for the accepted and
    /// rejected replies was driven by the motivation not to use polymorphism
    /// and thus have to upcast and downcast references all the time.</para>
    /// </summary>
    public OncRpcReplyStatus ReplyStatus { get; set; }

    /// <summary>
    /// Acceptance status in case this reply was sent in response to an accepted call
    /// (<see cref="OncRpcReplyStatus.OncRpcMessageAccepted"/>).
    /// This field can take any of the values defined in the <see cref="OncRpcAcceptStatus"/> interface. <para>
    /// 
    /// Note that even for accepted calls that only in the case of
    /// <see cref="OncRpcAcceptStatus.OncRpcSuccess"/>
    /// result data will follow the reply message header. </para>
    /// </summary>
    public OncRpcAcceptStatus AcceptStatus { get; set; }

    /// <summary>
    /// Reject status in case this reply sent in response to a rejected call (<see cref="OncRpcReplyStatus.OncRpcMessageDenied"/>).
    /// This field can take any of the values defined in the <see cref="OncRpcRejectStatus"/> interface.
    /// </summary>
    /// <value> The reject status. </value>
    public OncRpcRejectStatus RejectStatus { get; set; }

    /// <summary>
    /// Lowest supported version in case of
    /// <see cref="OncRpcRejectStatus.OncRpcWrongProtocolVersion"/>
    /// and
    /// <see cref="OncRpcAcceptStatus.OncRpcProgramVersionMismatch"/>.
    /// </summary>
    public int LowVersion { get; set; }

    /// <summary>
    /// Highest supported version in case of
    /// <see cref="OncRpcRejectStatus.OncRpcWrongProtocolVersion"/>
    /// and
    /// <see cref="OncRpcAcceptStatus.OncRpcProgramVersionMismatch"/>.
    /// </summary>
    public int HighVersion { get; set; }

    /// <summary>
    /// Contains the <see cref="OncRpcAuthStatus"/> reason for authentication failure in the case of
    /// <see cref="OncRpcRejectStatus.OncRpcAuthError"/>.
    /// </summary>
    public OncRpcAuthStatus AuthStatus { get; set; }

    /// <summary>
    /// Check whether this <see cref="OncRpcReplyMessageBase"/> represents an accepted and successfully
    /// executed remote procedure call.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if remote procedure call was accepted and
    /// successfully executed.
    /// </returns>
    public virtual bool SuccessfullyAccepted()
    {
        return this.ReplyStatus == OncRpcReplyStatus.OncRpcMessageAccepted && this.AcceptStatus == OncRpcAcceptStatus.OncRpcSuccess;
    }

    /// <summary>   Gets or sets ( <see langword="protected"/> ) the type of the authentication. </summary>
    /// <value> The type of the authentication. </value>
    public OncRpcAuthType? AuthType { get; protected set; }

    /// <summary>   Gets or sets ( <see langword="protected"/> ) the authentication length. </summary>
    /// <value> The length of the authentication. </value>
    public int? AuthLength { get; protected set; }

    #endregion

    #region " actions "

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

                        case OncRpcAcceptStatus.OncRpcUnableToDecodeArguments:
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

    #endregion

}
