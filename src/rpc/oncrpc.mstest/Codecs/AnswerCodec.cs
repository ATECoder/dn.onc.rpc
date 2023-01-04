namespace cc.isr.ONC.RPC.MSTest.Codecs;

/// <summary>   (Serializable) an answer XBR encoder/decoder. </summary>
/// <remarks>   2022-12-22. </remarks>
[Serializable]
public class AnswerCodec : IXdrCodec
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
    private const long serialVersionUID = 5165359675382683141L;

    /// <summary>   Default constructor. </summary>
    /// <remarks>   2022-12-22. </remarks>
    public AnswerCodec()
    {
    }

    /// <summary>   Constructor. </summary>
    /// <remarks>   2022-12-22. </remarks>
    /// <param name="xdr">  XDR stream from which decoded information is retrieved. </param>
    public AnswerCodec( XdrDecodingStreamBase xdr )
    {
        this.Decode( xdr );
    }

    /// <summary>   Gets or sets the value. </summary>
    /// <value> The value. </value>
    public int Value { get; set; }

    /// <summary>   Gets or sets the wrong. </summary>
    /// <value> The wrong. </value> 
    public int Wrong { get; set; }

    /// <summary>   Gets or sets the answer. </summary>
    /// <value> the answer. </value>
    public int TheAnswer { get; set; }

    /// <summary>   Gets or sets the check hash. </summary>
    /// <value> The check hash. </value>
    public int CheckHash { get; set; }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into a XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <remarks>
    /// Encodes -- that is: serializes -- an object into a XDR stream in compliance to RFC 1832.
    /// </remarks>
    /// <param name="xdr">  XDR stream to which information is sent for encoding. </param>
    public virtual void Encode( XdrEncodingStreamBase xdr )
    {
        xdr.EncodeInt( this.Value );
        switch ( this.Value )
        {
            case 40:
            case 41:
                xdr.EncodeInt( this.Wrong );
                break;
            case 42:
                xdr.EncodeInt( this.TheAnswer );
                break;
            default:
                xdr.EncodeInt( this.CheckHash );
                break;
        }
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
        this.Value = xdr.DecodeInt();
        switch ( this.Value )
        {
            case 40:
            case 41:
                this.Wrong = xdr.DecodeInt();
                break;
            case 42:
                this.TheAnswer = xdr.DecodeInt();
                break;
            default:
                this.CheckHash = xdr.DecodeInt();
                break;
        }
    }

}
