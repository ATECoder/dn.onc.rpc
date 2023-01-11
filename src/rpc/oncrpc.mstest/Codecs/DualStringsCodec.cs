namespace cc.isr.ONC.RPC.MSTest.Codecs;

/// <summary>   (Serializable) a dual strings XBR encoder/decoder. </summary>
[Serializable]
internal class DualStringsCodec : IXdrCodec
{
    /// <summary>   Gets or sets the argument 1. </summary>
    /// <value> The argument 1. </value>
    public string Arg1 { get; set; } = string.Empty;

    /// <summary>   Gets or sets the argument 2. </summary>
    /// <value> The argument 2. </value>
    public string Arg2 { get; set; } = string.Empty;

    /// <summary>
    /// Encodes -- that is: serializes -- an object into a XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <remarks>
    /// Encodes -- that is: serializes -- an object into a XDR stream in compliance to RFC 1832.
    /// </remarks>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase encoder )
    {
        encoder.EncodeString( this.Arg1 );
        encoder.EncodeString( this.Arg2 );
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
        this.Arg1 = decoder.DecodeString();
        this.Arg2 = decoder.DecodeString();
    }
};
