namespace cc.isr.ONC.RPC.Codecs;

/// <summary>
/// The server <see cref="OncRpcGetPortCodec"/> class represents the result from a 
/// <see cref="OncRpcPortmapServices.OncRpcPortmapGetPortNumber"/> remote
/// procedure call to the ONC/RPC portmapper.
/// </summary>
/// <remarks> <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public class OncRpcGetPortCodec : IXdrCodec
{
    /// <summary>
    /// The port number of the ONC/RPC in question. This is the only interesting piece of information
    /// in this class. Go live with it, you don't have alternatives.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Default constructor for initializing an server <see cref="OncRpcGetPortCodec"/>
    /// result object. It sets the <see cref="Port"/> member to a useless value.
    /// </summary>
    public OncRpcGetPortCodec()
    {
        this.Port = 0;
    }

    /// <summary>
    /// Encodes -- that is: serializes -- an server <see cref="OncRpcGetPortCodec"/>
    /// object into a XDR stream.
    /// </summary>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    ///
    /// <exception cref="OncRpcException">          Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="System.IO.IOException">    Thrown when an I/O error condition occurs. </exception>
    public virtual void Encode( XdrEncodingStreamBase encoder )
    {
        encoder.EncodeInt( this.Port );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an server <see cref="OncRpcGetPortCodec"/>
    /// object from a XDR stream.
    /// </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    ///
    /// <exception cref="OncRpcException">          Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="System.IO.IOException">    Thrown when an I/O error condition occurs. </exception>
    public virtual void Decode( XdrDecodingStreamBase decoder )
    {
        this.Port = decoder.DecodeInt();
    }
}
