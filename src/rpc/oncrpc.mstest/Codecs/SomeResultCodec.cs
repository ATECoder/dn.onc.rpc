namespace cc.isr.ONC.RPC.MSTest.Codecs;
#nullable disable 

/// <summary>   (Serializable) encapsulates the result of an XBR encoder/decoder. </summary>
/// <remarks>   2022-12-22. </remarks>
[Serializable]
public class SomeResultCodec : IXdrCodec
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
    private const long serialVersionUID = -6867149493429413131L;

    private int _error;
    private string _typeDesc;
    private byte[] _data;

    /// <summary>   Gets or sets the error. </summary>
    /// <value> The error. </value>
    public virtual int Error
    {
        set {
            this._error = value;
        }
        get {
            return this._error;
        }
    }

    /// <summary>   Gets or sets information describing the type. </summary>
    /// <value> Information describing the type. </value>
    public virtual string TypeDesc
    {
        set {
            this._typeDesc = value;
        }
        get {
            return this._typeDesc;
        }
    }

    /// <summary>   Sets a data. </summary>
    /// <param name="x">    The x coordinate. </param>
    public virtual void SetData( byte[] x )
    {
        this._data = x;
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

    /// <summary>   Default constructor. </summary>
    public SomeResultCodec()
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public SomeResultCodec( XdrDecodingStreamBase decoder )
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
        encoder.EncodeInt( this._error );
        encoder.EncodeString( this._typeDesc );
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
        this._error = decoder.DecodeInt();
        this._typeDesc = decoder.DecodeString();
        this._data = decoder.DecodeDynamicOpaque();
    }

}
