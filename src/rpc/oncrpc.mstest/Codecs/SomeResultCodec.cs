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
    /// <remarks>   2022-12-22. </remarks>
    /// <param name="x">    The x coordinate. </param>
    public virtual void SetData( byte[] x )
    {
        this._data = x;
    }
    /// <summary>   Sets a data. </summary>
    /// <remarks>   2022-12-22. </remarks>
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
    /// <remarks>   2022-12-22. </remarks>
    /// <param name="index">    Zero-based index of the. </param>
    /// <returns>   The data. </returns>
    public virtual byte GetData( int index )
    {
        return this._data[index];
    }

    /// <summary>   Default constructor. </summary>
    /// <remarks>   2022-12-22. </remarks>
    public SomeResultCodec()
    {
    }

    /// <summary>   Constructor. </summary>
    /// <remarks>   2022-12-22. </remarks>
    /// <param name="xdr">  XDR stream from which decoded information is retrieved. </param>
    public SomeResultCodec( XdrDecodingStreamBase xdr )
    {
        this.Decode( xdr );
    }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into a XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <remarks>
    /// Encodes -- that is: serializes -- an object into a XDR stream in compliance to RFC 1832.
    /// </remarks>
    /// <param name="xdr">  XDR stream to which information is sent for encoding. </param>
    public virtual void Encode( XdrEncodingStreamBase xdr )
    {
        xdr.EncodeInt( this._error );
        xdr.EncodeString( this._typeDesc );
        xdr.EncodeDynamicOpaque( this._data );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from a XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <remarks>
    /// Decodes -- that is: deserializes -- an object from a XDR stream in compliance to RFC 1832.
    /// </remarks>
    /// <param name="xdr">  XDR stream from which decoded information is retrieved. </param>
    public virtual void Decode( XdrDecodingStreamBase xdr )
    {
        this._error = xdr.DecodeInt();
        this._typeDesc = xdr.DecodeString();
        this._data = xdr.DecodeDynamicOpaque();
    }

}
