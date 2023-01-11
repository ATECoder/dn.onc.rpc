namespace cc.isr.ONC.RPC.MSTest.Codecs;

/// <summary>   (Serializable) a string triple integer XBR encoder/decoder. </summary>
[Serializable]
internal class StringTripleIntegerCodec : IXdrCodec
{
    /// <summary>   Gets or sets a. </summary>
    /// <value> a. </value>
    public string A { get; set; } = string.Empty;

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
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase encoder )
    {
        encoder.EncodeString( this.A );
        encoder.EncodeInt( this.B );
        encoder.EncodeInt( this.C );
        encoder.EncodeInt( this.D );
    }
    /// <summary>
    /// Decodes -- that is: deserializes -- an object from a XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <remarks>
    /// Decodes -- that is: deserializes -- an object from a XDR stream in compliance to RFC 1832.
    /// </remarks>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase decoder )
    {
        this.A = decoder.DecodeString();
        this.B = decoder.DecodeInt();
        this.C = decoder.DecodeInt();
        this.D = decoder.DecodeInt();
    }
};
