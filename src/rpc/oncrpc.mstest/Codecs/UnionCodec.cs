namespace cc.isr.ONC.RPC.MSTest.Codecs;

/// <summary>   (Serializable) an union XBR encoder/decoder. </summary>
[Serializable]
public class UnionCodec : IXdrCodec
{


    /* Remote Tea leftover:
     The serialization runtime associates with each serializable class a version number, called a serialVersionUID, 
     which is used during deserialization to verify that the sender and receiver of a serialized object have loaded 
     classes for that object that are compatible with respect to serialization. If the receiver has loaded a class 
     for the object that has a different serialVersionUID than that of the corresponding sender's class, then deserialization 
     will result in an InvalidClassException. A serializable class can declare its own serialVersionUID explicitly by declaring 
     a field named serialVersionUID that must be static, final, and of type 
     private long serialVersionUID = -5038378683068054714L;
    */

    /// <summary>   Default constructor. </summary>
    public UnionCodec()
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public UnionCodec( XdrDecodingStreamBase decoder ) : this()
    {
        this.Decode( decoder );
    }

    /// <summary>   The Okay. </summary>
    public bool Okay { get; set; }

    /// <summary>   The list. </summary>
    public LinkedListCodec? List { get; set; }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into a XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <remarks>
    /// Encodes -- that is: serializes -- an object into a XDR stream in compliance to RFC 1832.
    /// </remarks>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public virtual void Encode( XdrEncodingStreamBase encoder )
    {
        encoder.EncodeBoolean( this.Okay );
        if ( this.Okay == true )
        {
            if ( this.List is not null )
            {
                encoder.EncodeBoolean( true );
                this.List.Encode( encoder );
            }
            else
                encoder.EncodeBoolean( false );
            ;
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
        this.Okay = decoder.DecodeBoolean();
        if ( this.Okay == true )
            this.List = decoder.DecodeBoolean() ? new LinkedListCodec( decoder ) : null;
    }

}
