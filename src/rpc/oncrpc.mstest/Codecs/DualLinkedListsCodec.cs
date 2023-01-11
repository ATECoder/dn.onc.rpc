namespace cc.isr.ONC.RPC.MSTest.Codecs;

/// <summary>   (Serializable) a dual linked lists codec. </summary>
[Serializable]
internal class DualLinkedListsCodec : IXdrCodec
{
    /// <summary>   Gets or sets the list 1. </summary>
    /// <value> The list 1. </value>
    public LinkedListCodec List1 { get; set; } = new();

    /// <summary>   Gets or sets the list 2. </summary>
    /// <value> The list 2. </value>
    public LinkedListCodec List2 { get; set; } = new();

    /// <summary>
    /// Encodes -- that is: serializes -- an object into a XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <remarks>
    /// Encodes -- that is: serializes -- an object into a XDR stream in compliance to RFC 1832.
    /// </remarks>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase encoder )
    {
        this.List1.Encode( encoder );
        this.List2.Encode( encoder );
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
        this.List1 = new LinkedListCodec( decoder );
        this.List2 = new LinkedListCodec( decoder );
    }
};
