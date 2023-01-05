#nullable disable


namespace cc.isr.ONC.RPC.MSTest.Codecs;
/// <summary>   (Serializable) a silly structure XBR encoder/decoder. </summary>
/// <remarks>   2022-12-22. </remarks>
[Serializable]
public class SillyStructCodec : IXdrCodec
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
    private const long serialVersionUID = 4109809708757592008L;

    private byte[] _fixedBuffer;
    private byte[] _buffer;
    private byte[] _fixedBytes;
    private byte[] _bytes;
    private int _ui1;
    private int _ui2;
    private string _nonsense;

    /// <summary>   Sets fixed buffer. </summary>
    /// <remarks>   2022-12-22. </remarks>
    /// <param name="x">    The x coordinate. </param>
    public virtual void SetFixedBuffer( byte[] x )
    {
        this._fixedBuffer = x;
    }
    /// <summary>   Sets fixed buffer. </summary>
    /// <remarks>   2022-12-22. </remarks>
    /// <param name="index">    Zero-based index of the. </param>
    /// <param name="x">        The x coordinate. </param>
    public virtual void SetFixedBuffer( int index, byte x )
    {
        this._fixedBuffer[index] = x;
    }

    /// <summary>   Gets the buffer for fixed data. </summary>
    /// <value> A buffer for fixed data. </value>
    public virtual byte[] FixedBuffer => this._fixedBuffer;

    /// <summary>   Gets a fixed buffer value. </summary>
    /// <remarks>   2022-12-22. </remarks>
    /// <param name="index">    Zero-based index of the. </param>
    /// <returns>   The fixed buffer value. </returns>
    public virtual byte GetFixedbuffer( int index )
    {
        return this._fixedBuffer[index];
    }

    /// <summary>   Sets a buffer. </summary>
    /// <remarks>   2022-12-22. </remarks>
    /// <param name="x">    The x coordinate. </param>
    public virtual void SetBuffer( byte[] x )
    {
        this._buffer = x;
    }
    /// <summary>   Sets a buffer. </summary>
    /// <remarks>   2022-12-22. </remarks>
    /// <param name="index">    Zero-based index of the. </param>
    /// <param name="x">        The x coordinate. </param>
    public virtual void SetBuffer( int index, byte x )
    {
        this._buffer[index] = x;
    }
    /// <summary>   Gets the buffer. </summary>
    /// <value> The buffer. </value>
    public virtual byte[] Buffer => this._buffer;
    /// <summary>   Gets a buffer. </summary>
    /// <remarks>   2022-12-22. </remarks>
    /// <param name="index">    Zero-based index of the. </param>
    /// <returns>   The buffer. </returns>
    public virtual byte GetBuffer( int index )
    {
        return this._buffer[index];
    }

    /// <summary>   Sets fixed bytes. </summary>
    /// <remarks>   2022-12-22. </remarks>
    /// <param name="x">    The x coordinate. </param>
    public virtual void SetFixedBytes( byte[] x )
    {
        this._fixedBytes = x;
    }
    /// <summary>   Sets fixed bytes. </summary>
    /// <remarks>   2022-12-22. </remarks>
    /// <param name="index">    Zero-based index of the. </param>
    /// <param name="x">        The x coordinate. </param>
    public virtual void SetFixedBytes( int index, byte x )
    {
        this._fixedBytes[index] = x;
    }
    /// <summary>   Gets the fixed bytes. </summary>
    /// <value> The fixed bytes. </value>
    public virtual byte[] FixedBytes => this._fixedBytes;
    /// <summary>   Gets fixed bytes. </summary>
    /// <remarks>   2022-12-22. </remarks>
    /// <param name="index">    Zero-based index of the. </param>
    /// <returns>   The fixed bytes. </returns>
    public virtual byte GetFixedBytes( int index )
    {
        return this._fixedBytes[index];
    }

    /// <summary>   Sets the bytes. </summary>
    /// <remarks>   2022-12-22. </remarks>
    /// <param name="x">    The x coordinate. </param>
    public virtual void SetBytes( byte[] x )
    {
        this._bytes = x;
    }
    /// <summary>   Sets the bytes. </summary>
    /// <remarks>   2022-12-22. </remarks>
    /// <param name="index">    Zero-based index of the. </param>
    /// <param name="x">        The x coordinate. </param>
    public virtual void SetBytes( int index, byte x )
    {
        this._bytes[index] = x;
    }
    /// <summary>   Gets the bytes. </summary>
    /// <value> The bytes. </value>
    public virtual byte[] Bytes => this._bytes;
    /// <summary>   Gets the bytes. </summary>
    /// <remarks>   2022-12-22. </remarks>
    /// <param name="index">    Zero-based index of the. </param>
    /// <returns>   The bytes. </returns>
    public virtual byte GetBytes( int index )
    {
        return this._bytes[index];
    }

    /// <summary>   Gets or sets the 1. </summary>
    /// <value> The user interface 1. </value>
    public virtual int Ui1
    {
        set => this._ui1 = value;
        get => this._ui1;
    }

    /// <summary>   Gets or sets the 2. </summary>
    /// <value> The user interface 2. </value>
    public virtual int Ui2
    {
        set => this._ui2 = value;
        get => this._ui2;
    }

    /// <summary>   Gets or sets the nonsense. </summary>
    /// <value> The nonsense. </value>
    public virtual string Nonsense
    {
        set => this._nonsense = value;
        get => this._nonsense;
    }

    /// <summary>   Default constructor. </summary>
    /// <remarks>   2022-12-22. </remarks>
    public SillyStructCodec()
    {
    }

    /// <summary>   Constructor. </summary>
    /// <remarks>   2022-12-22. </remarks>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public SillyStructCodec( XdrDecodingStreamBase decoder )
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
        encoder.EncodeByteVector( this._fixedBuffer, 512 );
        encoder.EncodeByteVector( this._buffer );
        encoder.EncodeOpaque( this._fixedBytes, MiscConstants.FixedBufferLength );
        encoder.EncodeDynamicOpaque( this._bytes );
        encoder.EncodeInt( this._ui1 );
        encoder.EncodeInt( this._ui2 );
        encoder.EncodeString( this._nonsense );
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
        this._fixedBuffer = decoder.DecodeByteVector( 512 );
        this._buffer = decoder.DecodeByteVector();
        this._fixedBytes = decoder.DecodeOpaque( MiscConstants.FixedBufferLength );
        this._bytes = decoder.DecodeDynamicOpaque();
        this._ui1 = decoder.DecodeInt();
        this._ui2 = decoder.DecodeInt();
        this._nonsense = decoder.DecodeString();
    }

}
