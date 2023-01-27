namespace cc.isr.ONC.RPC.MSTest.Codecs;

/// <summary>   (Serializable) a dual strings XBR encoder/decoder. </summary>
[Serializable]
internal class DualStringsCodec : IXdrCodec
{
    /// <summary>   Gets or sets the first value. </summary>
    /// <value> The first value. </value>
    public string FirstValue { get; set; } = string.Empty;

    /// <summary>   Gets or sets the second value. </summary>
    /// <value> The second value. </value>
    public string SecondValue { get; set; } = string.Empty;

    /// <summary>
    /// Encodes -- that is: serializes -- an object into a XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <remarks>
    /// Encodes -- that is: serializes -- an object into a XDR stream in compliance to RFC 1832.
    /// </remarks>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase encoder )
    {
        encoder.EncodeString( this.FirstValue );
        encoder.EncodeString( this.SecondValue );
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
        this.FirstValue = decoder.DecodeString();
        this.SecondValue = decoder.DecodeString();
    }
};
