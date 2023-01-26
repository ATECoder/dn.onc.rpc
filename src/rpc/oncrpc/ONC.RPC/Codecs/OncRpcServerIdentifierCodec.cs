using cc.isr.ONC.RPC.EnumExtensions;

namespace cc.isr.ONC.RPC.Codecs;

/// <summary>
/// The class server <see cref="OncRpcServerIdentifierCodec"/> represents a tuple
/// {program, version, protocol, port} uniquely identifying a particular
/// ONC/RPC server on a given host.
/// </summary>
/// <remarks>
/// This information is used, for instance, as the ONC/RPC portmap PMAP_GETPORT call parameters. <para>
/// 
/// An server <see cref="OncRpcServerIdentifierCodec"/> can be directly serialized into an
/// encoding XDR stream (that is more political correct than "flushed down the toilet").</para> <para>
/// 
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public class OncRpcServerIdentifierCodec : IXdrCodec
{

    /// <summary>
    /// Constructs an server <see cref="OncRpcServerIdentifierCodec"/> object with senseless default values for the
    /// requested program number, version number, protocol type and port number.
    /// </summary>
    public OncRpcServerIdentifierCodec()
    {
        this.Program = 0;
        this.Version = 0;
        this.Protocol = 0;
        this.Port = 0;
    }

    /// <summary>
    /// Constructs an server <see cref="OncRpcServerIdentifierCodec"/> object with the requested program number,
    /// version number, protocol type and port number.
    /// </summary>
    /// <param name="program">  The program number of the ONC/RPC server in question. </param>
    /// <param name="version">  The program version number of the ONC/RPC server in question. </param>
    /// <param name="protocol"> The protocol used for communicating with the ONC/RPC server in
    ///                         question. </param>
    /// <param name="port">     The port number of the ONC/RPC server in question. </param>
    public OncRpcServerIdentifierCodec( int program, int version, OncRpcProtocol protocol, int port )
    {
        this.Program = program;
        this.Version = version;
        this.Protocol = protocol;
        this.Port = port;
    }

    /// <summary>
    /// Constructs an server <see cref="OncRpcServerIdentifierCodec"/> object and restores its state from the given XDR
    /// stream.
    /// </summary>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public OncRpcServerIdentifierCodec( XdrDecodingStreamBase decoder )
    {
        this.Decode( decoder );
    }

    /// <summary>   Gets or sets the program number of the ONC/RPC server in question. </summary>
    /// <value> The program. </value>
    public int Program { get; set; }

    /// <summary>   Gets or sets the program version number of the ONC/RPC server in question. </summary>
    /// <value> The version. </value>
    public int Version { get; set; }

    /// <summary>
    /// Gets or sets the protocol used for communicating with the ONC/RPC server in question. This
    /// can be one of the constants defined in the <see cref="OncRpcProtocol"/>
    /// interface.
    /// </summary>
    /// <value> The protocol. </value>
    public OncRpcProtocol Protocol { get; set; }

    /// <summary>   Gets or sets the port number of the ONC/RPC server in question. </summary>
    /// <value> The port. </value>
    public int Port { get; set; }


    /// <summary>
    /// Encodes -- that is: serializes -- an OncRpcServerIdent object into a XDR stream.
    /// </summary>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public virtual void Encode( XdrEncodingStreamBase encoder )
    {
        encoder.EncodeInt( this.Program );
        encoder.EncodeInt( this.Version );
        encoder.EncodeInt( ( int ) this.Protocol );
        encoder.EncodeInt( this.Port );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an OncRpcServerIdent object from a XDR stream.
    /// </summary>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public virtual void Decode( XdrDecodingStreamBase decoder )
    {
        this.Program = decoder.DecodeInt();
        this.Version = decoder.DecodeInt();
        this.Protocol = decoder.DecodeInt().ToProtocols();
        this.Port = decoder.DecodeInt();
    }
}
