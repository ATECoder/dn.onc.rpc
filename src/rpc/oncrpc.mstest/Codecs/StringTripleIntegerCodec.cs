#nullable disable

namespace cc.isr.ONC.RPC.MSTest.Codecs;

/// <summary>   (Serializable) a string triple integer XBR encoder/decoder. </summary>
/// <remarks>   2022-12-30. </remarks>
[Serializable]
internal class StringTripleIntegerCodec : IXdrCodec
{
    /// <summary>   Gets or sets a. </summary>
    /// <value> a. </value>
    public string A { get; set; }
    /// <summary>   Gets or sets the b. </summary>
    /// <value> The b. </value>
    public int B { get; set; }
    /// <summary>   Gets or sets the c. </summary>
    /// <value> The c. </value>
    public int C { get; set; }
    /// <summary>   Gets or sets the d. </summary>
    /// <value> The d. </value>
    public int D { get; set; }
    /// <summary>
    /// Encodes -- that is: serializes -- an object into a XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <remarks>
    /// Encodes -- that is: serializes -- an object into a XDR stream in compliance to RFC 1832.
    /// </remarks>
    /// <param name="xdr">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase xdr )
    {
        xdr.EncodeString( this.A );
        xdr.EncodeInt( this.B );
        xdr.EncodeInt( this.C );
        xdr.EncodeInt( this.D );
    }
    /// <summary>
    /// Decodes -- that is: deserializes -- an object from a XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <remarks>
    /// Decodes -- that is: deserializes -- an object from a XDR stream in compliance to RFC 1832.
    /// </remarks>
    /// <param name="xdr">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase xdr )
    {
        this.A = xdr.DecodeString();
        this.B = xdr.DecodeInt();
        this.C = xdr.DecodeInt();
        this.D = xdr.DecodeInt();
    }
};
