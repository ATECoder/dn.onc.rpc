namespace cc.isr.ONC.RPC.MSTest.Codecs;

/// <summary>   (Serializable) a silly structure XBR encoder/decoder. </summary>
[Serializable]
public class SillyStructCodec : IXdrCodec
{

    /* Remote Tea leftover:
     The serialization runtime associates with each serializable class a version number, called a serialVersionUID, 
     which is used during deserialization to verify that the sender and receiver of a serialized object have loaded 
     classes for that object that are compatible with respect to serialization. If the receiver has loaded a class 
     for the object that has a different serialVersionUID than that of the corresponding sender's class, then deserialization 
     will result in an InvalidClassException. A serializable class can declare its own serialVersionUID explicitly by declaring 
     a field named serialVersionUID that must be static, final, and of type 
     private long serialVersionUID = 4109809708757592008L;
    */

    /// <summary>   Default constructor. </summary>
    public SillyStructCodec()
    {
        this._fixedBuffer = Array.Empty<byte>();
        this._buffer = Array.Empty<byte>();
        this._bytes = Array.Empty<byte>();
        this._fixedBytes = Array.Empty<byte>();
        this.Nonsense = string.Empty;
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public SillyStructCodec( XdrDecodingStreamBase decoder ) : this()
    {
        this.Decode( decoder );
    }

    private byte[] _fixedBuffer;

    /// <summary>   Sets fixed buffer. </summary>
    /// <param name="x">    The x coordinate. </param>
    public virtual void SetFixedBuffer( byte[] x )
    {
        this._fixedBuffer = x ?? Array.Empty<byte>();
    }
    /// <summary>   Sets fixed buffer. </summary>
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
    /// <param name="index">    Zero-based index of the. </param>
    /// <returns>   The fixed buffer value. </returns>
    public virtual byte GetFixedbuffer( int index )
    {
        return this._fixedBuffer[index];
    }

    private byte[] _buffer;

    /// <summary>   Sets a buffer. </summary>
    /// <param name="x">    The x coordinate. </param>
    public virtual void SetBuffer( byte[] x )
    {
        this._buffer = x ?? Array.Empty<byte>();
    }
    /// <summary>   Sets a buffer. </summary>
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
    /// <param name="index">    Zero-based index of the. </param>
    /// <returns>   The buffer. </returns>
    public virtual byte GetBuffer( int index )
    {
        return this._buffer[index];
    }

    private byte[] _fixedBytes;

    /// <summary>   Sets fixed bytes. </summary>
    /// <param name="x">    The x coordinate. </param>
    public virtual void SetFixedBytes( byte[] x )
    {
        this._fixedBytes = x ?? Array.Empty<byte>();
    }
    /// <summary>   Sets fixed bytes. </summary>
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
    /// <param name="index">    Zero-based index of the. </param>
    /// <returns>   The fixed bytes. </returns>
    public virtual byte GetFixedBytes( int index )
    {
        return this._fixedBytes[index];
    }

    private byte[] _bytes;

    /// <summary>   Sets the bytes. </summary>
    /// <param name="x">    The x coordinate. </param>
    public virtual void SetBytes( byte[] x )
    {
        this._bytes = x ?? Array.Empty<byte>();
    }
    /// <summary>   Sets the bytes. </summary>
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
    /// <param name="index">    Zero-based index of the. </param>
    /// <returns>   The bytes. </returns>
    public virtual byte GetBytes( int index )
    {
        return this._bytes[index];
    }

    /// <summary>   Gets or sets the 1. </summary>
    /// <value> The user interface 1. </value>
    public virtual int Ui1 { get; set; }

    /// <summary>   Gets or sets the 2. </summary>
    /// <value> The user interface 2. </value>
    public virtual int Ui2 { get; set; }

    /// <summary>   Gets or sets the nonsense. </summary>
    /// <value> The nonsense. </value>
    public virtual string Nonsense { get; set; }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into a XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <remarks>
    /// Encodes -- that is: serializes -- an object into a XDR stream in compliance to RFC 1832.
    /// </remarks>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public virtual void Encode( XdrEncodingStreamBase encoder )
    {
        encoder.EncodeByteVector( this._fixedBuffer, 0, 512 );
        encoder.EncodeByteVector( this._buffer );
        encoder.EncodeOpaque( this._fixedBytes, MiscConstants.FixedBufferLength );
        encoder.EncodeDynamicOpaque( this._bytes );
        encoder.EncodeInt( this.Ui1 );
        encoder.EncodeInt( this.Ui2 );
        encoder.EncodeString( this.Nonsense );
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
        this.Ui1 = decoder.DecodeInt();
        this.Ui2 = decoder.DecodeInt();
        this.Nonsense = decoder.DecodeString();
    }

}
