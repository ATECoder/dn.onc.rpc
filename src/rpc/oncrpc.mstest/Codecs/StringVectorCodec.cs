namespace cc.isr.ONC.RPC.MSTest.Codecs;

/// <summary>   (Serializable) a string vector XBR encoder/decoder. </summary>
[Serializable]
public class StringVectorCodec : IXdrCodec
{

    /* Remote Tea leftover:
     The serialization runtime associates with each serializable class a version number, called a serialVersionUID, 
     which is used during deserialization to verify that the sender and receiver of a serialized object have loaded 
     classes for that object that are compatible with respect to serialization. If the receiver has loaded a class 
     for the object that has a different serialVersionUID than that of the corresponding sender's class, then deserialization 
     will result in an InvalidClassException. A serializable class can declare its own serialVersionUID explicitly by declaring 
     a field named serialVersionUID that must be static, final, and of type 
     private long serialVersionUID = -6645878168698853047L;
    */

    /// <summary>   Default constructor. </summary>
    public StringVectorCodec()
    {
        this._values = Array.Empty<StringCodec>();
    }

    /// <summary>   Constructor. </summary>
    /// <param name="value">    The value. </param>
    public StringVectorCodec( StringCodec[] value ) : this()
    {
        this._values = value;
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public StringVectorCodec( XdrDecodingStreamBase decoder ) : this()
    {
        this.Decode( decoder );
    }

    private StringCodec[] _values;

    /// <summary>   Gets the values. </summary>
    /// <returns>   An array of string codec. </returns>
    public StringCodec[] GetValues()
    {
        return this._values;
    }

    /// <summary>   Sets the values. </summary>
    /// <param name="values">   The values. </param>
    public void SetValues( StringCodec[] values )
    {
        this._values = values;
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
        int size = this._values.Length;
        encoder.EncodeInt( this._values.Length );
        foreach ( StringCodec stringCodec in this._values ) 
            stringCodec.Encode( encoder );
#if false
        int size = this._values.Length;
        encoder.EncodeInt( size );
        for ( int idx = 0; idx < size; ++idx )
            this._values[idx].Encode( encoder );
#endif
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
        int size = decoder.DecodeInt();
        this._values = new StringCodec[size];
        for ( int idx = 0; idx < size; ++idx )
            this._values[idx] = new StringCodec( decoder );
    }

}
