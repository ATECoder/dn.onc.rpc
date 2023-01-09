using System.ComponentModel;

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
    /// Constructs an <see cref="OncRpcException"/> with a reason of <see cref="OncRpcExceptionReason.OncRpcFailed"/>.
    /// </summary>
    public OncRpcException() : this( OncRpcExceptionReason.OncRpcFailed )
    {
    }

    /// <summary>
    /// Constructs an <see cref="OncRpcException"/> with the specified detail reason and message.
    /// </summary>
    /// <param name="reason">  The detail reason. </param>
    /// <param name="message"> The detail message. </param>
    public OncRpcException( OncRpcExceptionReason reason, string message ) : base( message )
    {
        this.Reason = reason;
    }

    /// <summary>
    /// Constructs an <see cref="OncRpcException"/> with the specified detail message.
    /// </summary>
    /// <param name="message"> The detail message. </param>
    public OncRpcException( string message ) : this( OncRpcExceptionReason.OncRpcFailed, message )
    {
    }

    /// <summary>
    /// Constructs an <see cref="OncRpcException"/> with the specified detail reason.
    /// </summary>
    /// <remarks>   The detail message is derived automatically from the <paramref name="reason"/>. </remarks>
    /// <param name="reason">   The reason. This can be one of the constants -- oops, that should be
    ///                         "public final static integers" -- defined in this interface. </param>
    public OncRpcException( OncRpcExceptionReason reason ) : this( reason, Support.GetDescription( reason ) )
    {
    }

    /// <summary>
    /// Constructs an <see cref="OncRpcException"/> with the specified detail reason and inner exception.
    /// </summary>
    /// <param name="reason">           The detail reason. </param>
    /// <param name="innerException">   The inner exception. </param>
    public OncRpcException( OncRpcExceptionReason reason, Exception innerException ) : base( Support.GetDescription( reason ), innerException )
    {
        this.Reason = reason;
    }

    /// <summary>
    /// Constructs an <see cref="OncRpcException"/> with the specified reason and a message to append
    /// to the default message.
    /// </summary>
    /// <param name="suffixMessage">    Message to append to default message. </param>
    /// <param name="reason">           The detail reason. </param>
    public OncRpcException( string suffixMessage, OncRpcExceptionReason reason ) : this( reason, Support.GetDescription( reason ) + suffixMessage )
    {
    }

    /// <summary>   Converts a reason to a short description. </summary>
    /// <remarks>   2023-01-07. </remarks>
    /// <param name="reason">   The detail reason. </param>
    /// <returns>   Reason as a string. </returns>
    public static string ToShortDescription( OncRpcExceptionReason reason ) 
    {
        switch ( reason )
        {
            case OncRpcExceptionReason.OncRpcCannotEncodeArgs:
                    return "cannot encode RPC arguments";

            case OncRpcExceptionReason.OncRpcCannotDecodeResult:
                    return "cannot decode RPC result";

            case OncRpcExceptionReason.OncRpcCannotReceive:
                    return "cannot receive ONC/RPC data";

            case OncRpcExceptionReason.OncRpcCannotSend:
                    return "cannot send ONC/RPC data";

            case OncRpcExceptionReason.OncRpcProcedureCallTimedOut:
                    return "ONC/RPC call timed out";

            case OncRpcExceptionReason.OncRpcClientServerVersionMismatch:
                    return "ONC/RPC version mismatch";

            case OncRpcExceptionReason.OncRpcAuthenticationError:
                    return "ONC/RPC authentication error";

            case OncRpcExceptionReason.OncRpcProgramNotAvailable:
                    return "ONC/RPC program not available";

            case OncRpcExceptionReason.OncRpcCannotDecodeArgs:
                    return "cannot decode ONC/RPC arguments";

            case OncRpcExceptionReason.OncRpcProgramVersionNotSupported:
                    return "ONC/RPC program version mismatch";

            case OncRpcExceptionReason.OncRpcProcedureNotAvailable:
                    return "ONC/RPC procedure not available";

            case OncRpcExceptionReason.OncRpcSystemError:
                    return "ONC/RPC system error";

            case OncRpcExceptionReason.OncRpcUnknownIpProtocol:
                    return "unknown IP protocol";

            case OncRpcExceptionReason.OncRpcPortMapServiceFailure:
                    return "ONC/RPC portmap failure";

            case OncRpcExceptionReason.OncRpcProgramNotRegistered:
                    return "ONC/RPC program not registered";

            case OncRpcExceptionReason.OncRpcFailed:
                    return "ONC/RPC generic failure";

            case OncRpcExceptionReason.OncRpcBufferOverflow:
                    return "ONC/RPC buffer overflow";

            case OncRpcExceptionReason.OncRpcBufferUnderflow:
                    return "ONC/RPC buffer underflow";

            case OncRpcExceptionReason.OncRpcWrongMessageType:
                    return "wrong ONC/RPC message type received";

            case OncRpcExceptionReason.OncRpcCannotRegisterTransport:
                    return "cannot register ONC/RPC port with local portmap";

            case OncRpcExceptionReason.OncRpcSuccess:
                return "success";
            default:
                return "unknown";
        }
    }

    /// <summary>
    /// Specific (reason) for this <see cref="OncRpcException"/> <see cref="OncRpcException"/>, like
    /// the ONC/RPC error code, as defined by the constants of this class.
    /// </summary>
    /// <value>
    /// The error reason of this <see cref="OncRpcException"/> object if it was
    /// <see cref="OncRpcException(OncRpcExceptionReason)">created</see> with an error reason; or
    /// <see cref="OncRpcExceptionReason.OncRpcFailed"/> if it was <see cref="OncRpcException()">created</see>
    /// with no error reason.
    /// </value>
    public OncRpcExceptionReason Reason { get; private set; }

}

/// <summary>
/// A collection of constants used to identify the <see cref="OncRpcException"/>.
/// </summary>
public enum OncRpcExceptionReason
{

    /// <summary>   The remote procedure call was carried out successfully. <para>
    /// Renamed from <c>RPC_SUCCESS (=0)</c>, which maps to VXI-11 Visa32.VISA.VI_SUCCESS.  </para></summary>
    [Description( "The remote procedure call was carried out successfully." )]
    OncRpcSuccess = 0,

    /// <summary>
    /// (Immutable)
    /// The client cannot encode the arguments to be sent for the remote procedure call. <para>
    /// Renamed from <c>RPC_CANTENCODEARGS (=1)</c>.  </para></summary>
    [Description( "The RPC arguments cannot be encoded." )]
    OncRpcCannotEncodeArgs = 1,

    /// <summary>
    /// The client cannot decode the result from the remote procedure call. <para>
    /// Renamed from <c>RPC_CANTDECODERES (=2)</c>.  </para></summary>
    [Description( "The RPC result cannot be decoded." )]
    OncRpcCannotDecodeResult = 2,

    /// <summary>   Encoded information cannot be sent. <para>
    /// Renamed from <c>RPC_CANTSEND (=3)</c>.  </para></summary>
    [Description( "Cannot send ONC/RPC data." )]
    OncRpcCannotSend = 3,

    /// <summary>   Information to be decoded cannot be received. <para>
    /// Renamed from <c>RPC_CANTRECV (=4)</c>.  </para></summary>
    [Description( "Cannot receive ONC/RPC data." )]
    OncRpcCannotReceive = 4,

    /// <summary>   The remote procedure call timed out. <para>
    /// Renamed from <c>RPC_TIMEDOUT (=5)</c>.  </para></summary>
    [Description( "ONC/RPC call timed out" )]
    OncRpcProcedureCallTimedOut = 5,

    /// <summary>   ONC/RPC versions of server and client are not compatible. <para>
    /// Renamed from <c>RPC_VERSMISMATCH (=6)</c>.  </para></summary>
    [Description( "ONC/RPC versions of server and client are incompatible" )]
    OncRpcClientServerVersionMismatch = 6,

    /// <summary>
    /// (Immutable)
    /// The ONC/RPC server did not accept the authentication sent by the client. <para>
    /// Renamed from <c>RPC_AUTHERROR (=7)</c>. </para></summary>
    [Description( "The ONC/RPC server did not accept the authentication sent by the client." )]
    OncRpcAuthenticationError = 7,

    /// <summary>   The ONC/RPC server does not support this particular program. <para>
    /// Renamed from <c>RPC_PROGUNAVAIL (=8)</c>, which maps to Visa32.VISA.VI_ERROR_INV_EXPR.  </para></summary>
    [Description( "The ONC/RPC server does not support this particular program." )]
    OncRpcProgramNotAvailable = 8,

    /// <summary>
    /// The ONC/RPC server does not support this particular version of the program. <para>
    /// Renamed from <c>RPC_PROGVERSMISMATCH (=9)</c>. </para></summary>
    [Description( "The ONC/RPC server does not support this particular version of the program." )]
    OncRpcProgramVersionNotSupported = 9,

    /// <summary>   The given procedure is not available at the ONC/RPC server.  <para>
    /// Renamed from <c>RPC_PROCUNAVAIL = 10</c>. </para></summary>
    [Description( "The given procedure is not available at the ONC/RPC server." )]
    OncRpcProcedureNotAvailable = 10,

    /// <summary>
    /// The ONC/RPC server could not decode the arguments sent within the call message. <para>
    /// Renamed from <c>RPC_CANTDECODEARGS = 11</c>. </para></summary>
    [Description( "The ONC/RPC server could not decode the arguments sent within the call message." )]
    OncRpcCannotDecodeArgs = 11,

    /// <summary>
    /// The ONC/RPC server encountered a system error and thus was not able to carry out the
    /// requested remote function call successfully.
    /// Maps to Visa32.VISA.VI_ERROR_SYSTEM_ERROR <para>
    /// Renamed from <c>RPC_SYSTEMERROR = 12</c>. </para></summary>
    [Description( "The ONC/RPC server encountered a system error and thus was not able to carry out the requested remote function call successfully." )]
    OncRpcSystemError = 12,

    /// <summary>   The portmapper could not be contacted at the given host.  <para>
    /// Renamed from <c>RPC_PMAPFAILURE = 14</c>. </para></summary>
    [Description( "The portmapper could not be contacted at the given host." )]
    OncRpcPortMapServiceFailure = 14,

    /// <summary>   The requested program is not registered with the given host.  <para>
    /// Renamed from <c>RPC_PROGNOTREGISTERED = 15</c>. </para></summary>
    [Description( "The requested program is not registered with the given host." )]
    OncRpcProgramNotRegistered = 15,

    /// <summary>   A generic ONC/RPC exception occurred.  <para>
    /// Renamed from <c>RPC_FAILED = 16</c>. </para></summary>
    [Description( "A generic ONC/RPC exception occurred." )]
    OncRpcFailed = 16,

    /// <summary>
    /// The caller specified an unknown/unsupported IP protocol. Currently, only
    /// <see cref="OncRpcProtocols.OncRpcTcp"/> and <see cref="OncRpcProtocols.OncRpcUdp"/>
    /// are supported. <para>
    /// Renamed from <c>RPC_UNKNOWNPROTO = 17</c>. </para></summary>
    [Description( "The caller specified an unknown/unsupported IP protocol. Currently, only TCP/IP and UDP/IP are supported." )]
    OncRpcUnknownIpProtocol = 17,

    /// <summary>
    /// A buffer overflow occurred with an encoding XDR stream. This happens if you use
    /// UDP-based (datagram-based) XDR streams and you try to encode more data than can fit into the
    /// sending buffers. <para>
    /// Renamed from <c>RPC_BUFFEROVERFLOW (=42)</c>. </para></summary>
    [Description( "A buffer overflow occurred with an encoding XDR stream." )]
    OncRpcBufferOverflow = 42,

    /// <summary>
    /// A buffer underflow occurred with an decoding XDR stream. This happens if you try
    /// to decode more data than was sent by the other communication partner. <para>
    /// Renamed from <c>RPC_BUFFERUNDERFLOW = 43</c>. </para>
    /// </summary>
    [Description( "A buffer underflow occurred with an decoding XDR stream." )]
    OncRpcBufferUnderflow = 43,

    /// <summary>
    /// Either a ONC/RPC server or client received the wrong type of ONC/RPC message when waiting for
    /// a request or reply. Currently, only the decoding methods of the classes
    /// <see cref="OncRpcCallMessageBase"/> and <see cref="OncRpcReplyMessageBase"/>
    /// throw exceptions with this reason. <para>
    /// Renamed from <c>RPC_WRONGMESSAGE = 44</c>. </para></summary>
    [Description( "Either a ONC/RPC server or client received the wrong type of ONC/RPC message when waiting for a request or reply." )]
    OncRpcWrongMessageType = 44,

    /// <summary>
    /// A server could not register a transport with the ONC/RPC port mapper. <para>
    /// Renamed from <c>RPC_CANNOTREGISTER = 45</c>. </para></summary>
    [Description( "A server could not register a transport with the ONC/RPC port mapper." )]
    OncRpcCannotRegisterTransport = 45,

}
