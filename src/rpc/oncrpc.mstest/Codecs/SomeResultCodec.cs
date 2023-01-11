namespace cc.isr.ONC.RPC.MSTest.Codecs;

/// <summary>   (Serializable) encapsulates the result of an XBR encoder/decoder. </summary>
[Serializable]
public class SomeResultCodec : IXdrCodec
{

    /* Remote Tea leftover:
     The serialization runtime associates with each serializable class a version number, called a serialVersionUID, 
     which is used during deserialization to verify that the sender and receiver of a serialized object have loaded 
     classes for that object that are compatible with respect to serialization. If the receiver has loaded a class 
     for the object that has a different serialVersionUID than that of the corresponding sender's class, then deserialization 
     will result in an InvalidClassException. A serializable class can declare its own serialVersionUID explicitly by declaring 
     a field named serialVersionUID that must be static, final, and of type 
     private long serialVersionUID = -6867149493429413131L;
    */


    /// <summary>   Default constructor. </summary>
    public SomeResultCodec()
    {
        this.TypeDesc = string.Empty;
        this._data = Array.Empty<byte>();
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public SomeResultCodec( XdrDecodingStreamBase decoder ) : this()
    {
        this.Decode( decoder );
    }
    /// <summary>   Gets or sets the error. </summary>
    /// <value> The error. </value>
    public virtual int Error { get; set; }

    /// <summary>   Gets or sets information describing the type. </summary>
    /// <value> Information describing the type. </value>
    public virtual string TypeDesc { get; set; }

    private byte[] _data;

    /// <summary>   Sets a data. </summary>
    /// <param name="x">    The x coordinate. </param>
    public virtual void SetData( byte[] x )
    {
        this._data = x ?? Array.Empty<byte>();
    }
    /// <summary>   Sets a data. </summary>
    /// <param name="index">    Zero-based index of the. </param>
    /// <param name="x">        The x coordinate. </param>
    public virtual void SetData( int index, byte x )
    {
        this._data[index] = x;
    }
    /// <summary>   Gets the data. </summary>
    /// <value> The data. </value>
    public virtual byte[] Data => this._data;

    /// <summary>   Gets a data. </summary>
    /// <param name="index">    Zero-based index of the. </param>
    /// <returns>   The data. </returns>
    public virtual byte GetData( int index )
    {
        return this._data[index];
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
        encoder.EncodeInt( this.Error );
        encoder.EncodeString( this.TypeDesc );
        encoder.EncodeDynamicOpaque( this._data );
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
        this.Error = decoder.DecodeInt();
        this.TypeDesc = decoder.DecodeString();
        this._data = decoder.DecodeDynamicOpaque();
    }

}
