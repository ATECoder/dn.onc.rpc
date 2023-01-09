namespace cc.isr.ONC.RPC;

/// <summary>
/// The abstract <see cref="OncRpcCallMessageBase"/> class represents a remote procedure call
/// message as defined by ONC/RPC in RFC 1831.
/// </summary>
/// <remarks>
/// Such messages are sent by ONC/RPC clients to servers in order to request a remote procedure
/// call. <para>
/// Note that this is an abstract class. Because call message objects also need to deal with
/// authentication protocol issues, they need help of so-called authentication protocol handling
/// objects. These objects are of different classes, depending on where they are used (either
/// within the server or the client).</para> <para>
/// Please also note that this class implements no encoding or decoding functionality: it doesn't
/// need them. Only derived classes will be able to be encoded on the side of the client and
/// decoded at the end of the server.</para> <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public abstract class OncRpcCallMessageBase : OncRpcMessageBase
{
    /// <summary>
    /// (Immutable) Protocol version used by this ONC/RPC implementation. The protocol version 2
    /// is defined in RFC 1831. <para>
    /// Renamed from <c>ONCRPC_VERSION (=2)</c> </para> </summary>
    public const int OncRpcProtocolVersion = 2;

    /// <summary>   Constructs and initializes a new ONC/RPC call message header. </summary>
    /// <param name="messageId">    An identifier chosen by an ONC/RPC client to uniquely identify
    ///                             matching call and reply messages. </param>
    /// <param name="program">      Program number of the remote procedure to call. </param>
    /// <param name="version">      Program version number of the remote procedure to call. </param>
    /// <param name="procedure">    Procedure number (identifier) of the procedure to call. </param>
    public OncRpcCallMessageBase( int messageId, int program, int version, int procedure ) : base( messageId )
    {
        this.MessageType = OncRpcMessageType.OncRpcCallMessageType;
        this.ProtocolVersion = OncRpcProtocolVersion;
        this.Program = program;
        this.Version = version;
        this.Procedure = procedure;
    }

    /// <summary>   Constructs a new (incompletely initialized) ONC/RPC call message header. </summary>
    /// <remarks>
    /// The <see cref="OncRpcMessageBase.MessageType"/> is set to <see cref="OncRpcMessageType.OncRpcCallMessageType"/>
    /// and the <see cref="ProtocolVersion"/> is set to <see cref="OncRpcProtocolVersion"/>.
    /// </remarks>
    public OncRpcCallMessageBase() : base( OncRpcMessageBase.DefaultMessageId )
    {
        this.MessageType = OncRpcMessageType.OncRpcCallMessageType;
        this.ProtocolVersion = OncRpcProtocolVersion;
        this.Program = 0;
        this.Version = 0;
        this.Procedure = 0;
    }

    /// <summary>   Protocol version used by this ONC/RPC call message. </summary>
    public int ProtocolVersion { get; set; }

    /// <summary>   Program number of this particular remote procedure call message. </summary>
    public int Program { get; set; }

    /// <summary>   Program version number of this particular remote procedure call message. </summary>
    public int Version { get; set; }

    /// <summary>   Number (identifier) of remote procedure to call. </summary>
    public int Procedure { get; set; }


}
