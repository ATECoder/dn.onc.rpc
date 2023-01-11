#nullable disable


namespace cc.isr.ONC.RPC.MSTest.Codecs;

/// <summary>   (Serializable) a string vector XBR encoder/decoder. </summary>
/// <remarks>   2022-12-22. </remarks>
[Serializable]
public class StringVectorCodec : IXdrCodec
{

    /* Remote Tea leftover:
     * The serialization runtime associates with each serializable class a version number, called a serialVersionUID, 
     * which is used during deserialization to verify that the sender and receiver of a serialized object have loaded 
     * classes for that object that are compatible with respect to serialization. If the receiver has loaded a class 
     * for the object that has a different serialVersionUID than that of the corresponding sender's class, then deserialization 
     * will result in an InvalidClassException. A serializable class can declare its own serialVersionUID explicitly by declaring 
     * a field named serialVersionUID that must be static, final, and of type 
     */
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>" )]
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Style", "IDE1006:Naming Styles", Justification = "<Pending>" )]
    private const long serialVersionUID = -6645878168698853047L;

    /// <summary>   Gets or sets the value. </summary>
    /// <value> The value. </value>
    public StringCodec[] Value { get; set; }

    /// <summary>   Default constructor. </summary>
    public StringVectorCodec()
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="value">    The value. </param>
    public StringVectorCodec( StringCodec[] value )
    {
        this.Value = value;
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public StringVectorCodec( XdrDecodingStreamBase decoder )
    {
        this.Decode( decoder );
    }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into a XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <remarks>
    /// Encodes -- that is: serializes -- an object into a XDR stream in compliance to RFC 1832.
    /// </remarks>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public virtual void Encode( XdrEncodingStreamBase encoder )
    {
        {
            int size = this.Value.Length;
            encoder.EncodeInt( size );
            for ( int idx = 0; idx < size; ++idx )
                this.Value[idx].Encode( encoder );
        }
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from a XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <remarks>
    /// Decodes -- that is: deserializes -- an object from a XDR stream in compliance to RFC 1832.
    /// </remarks>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public virtual void Decode( XdrDecodingStreamBase decoder )
    {
        {
            int size = decoder.DecodeInt();
            this.Value = new StringCodec[size];
            for ( int idx = 0; idx < size; ++idx )
                this.Value[idx] = new StringCodec( decoder );
        }
    }

}
