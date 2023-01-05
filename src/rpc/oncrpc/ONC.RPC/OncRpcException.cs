namespace cc.isr.ONC.RPC;

/// <summary>
/// The class <see cref="OncRpcException"/> indicates ONC/RPC conditions that a reasonable
/// application might want to catch.
/// </summary>
/// <remarks>
/// The class <see cref="OncRpcException"/> also defines a set of ONC/RPC error codes as
/// defined by RFC 1831. Note that all these error codes are solely used on the client-side or
/// server-side, but never transmitted over the wire. For error codes transmitted over the
/// network, refer to <see cref="OncRpcAcceptStatus"/> and <see cref="OncRpcRejectStatus"/>. <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
[Serializable]
public class OncRpcException : Exception
{

    /// <summary>
    /// Constructs an <see cref="OncRpcException"/> with a reason of <see cref="OncRpcException.OncRpcFailed"/>.
    /// </summary>
    public OncRpcException() : this( OncRpcException.OncRpcFailed )
    {
    }

    /// <summary>
    /// Constructs an <see cref="OncRpcException"/> with the specified detail message.
    /// </summary>
    /// <param name="message"> The detail message. </param>
    public OncRpcException( string message ) : base()
    {
        this.Reason = OncRpcException.OncRpcFailed;
        this._message = message;
    }

    /// <summary>
    /// Constructs an <see cref="OncRpcException"/> with the specified detail reason and message.
    /// </summary>
    /// <param name="reason">  The detail reason. </param>
    /// <param name="message"> The detail message. </param>
    public OncRpcException( int reason, string message ) : base()
    {
        this.Reason = reason;
        this._message = message;
    }

    /// <summary>
    /// Constructs an <see cref="OncRpcException"/> with the specified detail reason.
    /// </summary>
    /// <remarks>   The detail message is derived automatically from the <paramref name="reason"/>. </remarks>
    /// <param name="reason">   The reason. This can be one of the constants -- oops, that should be
    ///                         "public final static integers" -- defined in this interface. </param>
    public OncRpcException( int reason ) : base()
    {
        this.Reason = reason;
        switch ( reason )
        {
            case OncRpcException.OncRpcCannotEncodeArgs:
                {
                    this._message = "cannot encode RPC arguments";
                    break;
                }

            case OncRpcException.OncRpcCannotDecodeResult:
                {
                    this._message = "cannot decode RPC result";
                    break;
                }

            case OncRpcException.OncRpcCannotReceive:
                {
                    this._message = "cannot receive ONC/RPC data";
                    break;
                }

            case OncRpcException.OncRpcCannotSend:
                {
                    this._message = "cannot send ONC/RPC data";
                    break;
                }

            case OncRpcException.OncRpcProcedureCallTimedOut:
                {
                    this._message = "ONC/RPC call timed out";
                    break;
                }

            case OncRpcException.OncRpcClientServerVersionMismatch:
                {
                    this._message = "ONC/RPC version mismatch";
                    break;
                }

            case OncRpcException.OncRpcAuthenticationError:
                {
                    this._message = "ONC/RPC authentication error";
                    break;
                }

            case OncRpcException.OncRpcProgramNotAvailable:
                {
                    this._message = "ONC/RPC program not available";
                    break;
                }

            case OncRpcException.OncRpcCannotDecodeArgs:
                {
                    this._message = "cannot decode ONC/RPC arguments";
                    break;
                }

            case OncRpcException.OncRpcProgramVersionNotSupported:
                {
                    this._message = "ONC/RPC program version mismatch";
                    break;
                }

            case OncRpcException.OncRpcProcedureNotAvailable:
                {
                    this._message = "ONC/RPC procedure not available";
                    break;
                }

            case OncRpcException.OncRpcSystemError:
                {
                    this._message = "ONC/RPC system error";
                    break;
                }

            case OncRpcException.OncRpcUnknownIpProtocol:
                {
                    this._message = "unknown IP protocol";
                    break;
                }

            case OncRpcException.OncRpcPortMapServiceFailure:
                {
                    this._message = "ONC/RPC portmap failure";
                    break;
                }

            case OncRpcException.OncRpcProgramNotRegistered:
                {
                    this._message = "ONC/RPC program not registered";
                    break;
                }

            case OncRpcException.OncRpcFailed:
                {
                    this._message = "ONC/RPC generic failure";
                    break;
                }

            case OncRpcException.OncRpcBufferOverflow:
                {
                    this._message = "ONC/RPC buffer overflow";
                    break;
                }

            case OncRpcException.OncRpcBufferUnderflow:
                {
                    this._message = "ONC/RPC buffer underflow";
                    break;
                }

            case OncRpcException.OncRpcWrongMessageType:
                {
                    this._message = "wrong ONC/RPC message type received";
                    break;
                }

            case OncRpcException.OncRpcCannotRegisterTransport:
                {
                    this._message = "cannot register ONC/RPC port with local portmap";
                    break;
                }

            case OncRpcException.OncRpcSuccess:
            default:
                {
                    break;
                }
        }
    }

    private readonly string _message;

    /// <summary>   Returns the error message string of this ONC/RPC object. </summary>
    /// <value>
    /// The error message string of this <see cref="OncRpcException"/>
    /// object if it was created either with an error message string or an ONC/RPC error code.
    /// </value>
    public override string Message => this._message;

    /// <summary>
    /// Specific (reason) for this <see cref="OncRpcException"/> <see cref="OncRpcException"/>, like
    /// the ONC/RPC error code, as defined by the constants of this class.
    /// </summary>
    /// <value>
    /// The error reason of this <see cref="OncRpcException"/> object if it was
    /// <see cref="OncRpcException(int)">created</see> with an error reason; or
    /// <see cref="OncRpcFailed"/> if it was <see cref="OncRpcException()">created</see>
    /// with no error reason.
    /// </value>
    public int Reason { get; private set; }


    /// <summary>   (Immutable) The remote procedure call was carried out successfully. <para>
    /// Renamed from <c>RPC_SUCCESS (=0)</c>, which maps to VXI-11 Visa32.VISA.VI_SUCCESS.  </para></summary>
    public const int OncRpcSuccess = 0;

    /// <summary>
    /// (Immutable)
    /// The client cannot encode the arguments to be sent for the remote procedure call. <para>
    /// Renamed from <c>RPC_CANTENCODEARGS (=1)</c>.  </para></summary>
    public const int OncRpcCannotEncodeArgs = 1;

    /// <summary>
    /// (Immutable) The client cannot decode the result from the remote procedure call. <para>
    /// Renamed from <c>RPC_CANTDECODERES (=2)</c>.  </para></summary>
    public const int OncRpcCannotDecodeResult = 2;

    /// <summary>   (Immutable) Encoded information cannot be sent. <para>
    /// Renamed from <c>RPC_CANTSEND (=3)</c>.  </para></summary>
    public const int OncRpcCannotSend = 3;

    /// <summary>   (Immutable) Information to be decoded cannot be received. <para>
    /// Renamed from <c>RPC_CANTRECV (=4)</c>.  </para></summary>
    public const int OncRpcCannotReceive = 4;

    /// <summary>   (Immutable) The remote procedure call timed out. <para>
    /// Renamed from <c>RPC_TIMEDOUT (=5)</c>.  </para></summary>
    public const int OncRpcProcedureCallTimedOut = 5;

    /// <summary>   (Immutable) ONC/RPC versions of server and client are not compatible. <para>
    /// Renamed from <c>RPC_VERSMISMATCH (=6)</c>.  </para></summary>
    public const int OncRpcClientServerVersionMismatch = 6;

    /// <summary>
    /// (Immutable)
    /// The ONC/RPC server did not accept the authentication sent by the client. <para>
    /// Renamed from <c>RPC_AUTHERROR (=7)</c>. </para></summary>
    public const int OncRpcAuthenticationError = 7;

    /// <summary>   (Immutable) The ONC/RPC server does not support this particular program. <para>
    /// Renamed from <c>RPC_PROGUNAVAIL (=8)</c>, which maps to Visa32.VISA.VI_ERROR_INV_EXPR.  </para></summary>
    public const int OncRpcProgramNotAvailable = 8;

    /// <summary>
    /// (Immutable) The ONC/RPC server does not support this particular version of the program. <para>
    /// Renamed from <c>RPC_PROGVERSMISMATCH (=9)</c>. </para></summary>
    public const int OncRpcProgramVersionNotSupported = 9;

    /// <summary>   (Immutable) The given procedure is not available at the ONC/RPC server.  <para>
    /// Renamed from <c>RPC_PROCUNAVAIL = 10</c>. </para></summary>
    public const int OncRpcProcedureNotAvailable = 10;

    /// <summary>
    /// (Immutable) The ONC/RPC server could not decode the arguments sent within the call message. <para>
    /// Renamed from <c>RPC_CANTDECODEARGS = 11</c>. </para></summary>
    public const int OncRpcCannotDecodeArgs = 11;

    /// <summary>
    /// (Immutable) The ONC/RPC server encountered a system error and thus was not able to carry out the
    /// requested remote function call successfully.
    /// Maps to Visa32.VISA.VI_ERROR_SYSTEM_ERROR <para>
    /// Renamed from <c>RPC_SYSTEMERROR = 12</c>. </para></summary>
    public const int OncRpcSystemError = 12;

    /// <summary>
    /// (Immutable) The caller specified an unknown/unsupported IP protocol. Currently, only
    /// <see cref="OncRpcProtocols.OncRpcTcp"/> and <see cref="OncRpcProtocols.OncRpcUdp"/>
    /// are supported. <para>
    /// Renamed from <c>RPC_UNKNOWNPROTO = 17</c>. </para></summary>
    public const int OncRpcUnknownIpProtocol = 17;

    /// <summary>   (Immutable) The portmapper could not be contacted at the given host.  <para>
    /// Renamed from <c>RPC_PMAPFAILURE = 14</c>. </para></summary>
    public const int OncRpcPortMapServiceFailure = 14;

    /// <summary>   (Immutable) The requested program is not registered with the given host.  <para>
    /// Renamed from <c>RPC_PROGNOTREGISTERED = 15</c>. </para></summary>
    public const int OncRpcProgramNotRegistered = 15;

    /// <summary>   (Immutable) A generic ONC/RPC exception occurred.  <para>
    /// Renamed from <c>RPC_FAILED = 16</c>. </para></summary>
    public const int OncRpcFailed = 16;

    /// <summary>
    /// (Immutable) A buffer overflow occurred with an encoding XDR stream. This happens if you use
    /// UDP-based (datagram-based) XDR streams and you try to encode more data than can fit into the
    /// sending buffers. <para>
    /// Renamed from <c>RPC_BUFFEROVERFLOW (=42)</c>. </para></summary>
    public const int OncRpcBufferOverflow = 42;

    /// <summary>
    /// (Immutable) A buffer underflow occurred with an decoding XDR stream. This happens if you try
    /// to decode more data than was sent by the other communication partner. <para>
    /// Renamed from <c>RPC_BUFFERUNDERFLOW = 43</c>. </para>
    /// </summary>
    public const int OncRpcBufferUnderflow = 43;

    /// <summary>
    /// (Immutable)
    /// Either a ONC/RPC server or client received the wrong type of ONC/RPC message when waiting for
    /// a request or reply. Currently, only the decoding methods of the classes
    /// <see cref="OncRpcCallMessageBase"/> and <see cref="OncRpcReplyMessageBase"/>
    /// throw exceptions with this reason. <para>
    /// Renamed from <c>RPC_WRONGMESSAGE = 44</c>. </para></summary>
    public const int OncRpcWrongMessageType = 44;

    /// <summary>
    /// (Immutable)
    /// Indicates that a server could not register a transport with the ONC/RPC port mapper. <para>
    /// Renamed from <c>RPC_CANNOTREGISTER = 45</c>. </para></summary>
    public const int OncRpcCannotRegisterTransport = 45;

}
