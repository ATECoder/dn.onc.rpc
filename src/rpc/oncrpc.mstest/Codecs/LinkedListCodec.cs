namespace cc.isr.ONC.RPC.MSTest.Codecs;

/// <summary>   (Serializable) linked list  XBR encoder/decoder. </summary>
[Serializable]
public class LinkedListCodec : IXdrCodec
{

    /* Remote Tea leftover:
     The serialization runtime associates with each serializable class a version number, called a serialVersionUID, 
     which is used during deserialization to verify that the sender and receiver of a serialized object have loaded 
     classes for that object that are compatible with respect to serialization. If the receiver has loaded a class 
     for the object that has a different serialVersionUID than that of the corresponding sender's class, then deserialization 
     will result in an InvalidClassException. A serializable class can declare its own serialVersionUID explicitly by declaring 
     a field named serialVersionUID that must be static, final, and of type 
     private long serialVersionUID = -9187504517170663946L;
    */

    /// <summary>   Default constructor. </summary>
    public LinkedListCodec()
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public LinkedListCodec( XdrDecodingStreamBase decoder ) : this()
    {
        this.Decode( decoder );
    }

    /// <summary>   Copy constructor. </summary>
    /// <param name="linkedListCodec">  The linked list codec. </param>
    public LinkedListCodec( LinkedListCodec linkedListCodec )
    {
        LinkedListCodec? expected = linkedListCodec;
        while ( expected != null )
        {
            this.Foo = expected.Foo;
            this.Next = expected.Next;
            expected = expected.Next;
        }
    }


    /// <summary>   Gets or sets the foo. </summary>
    /// <value> The foo. </value>
    public virtual int Foo { get; set; }

    /// <summary>   Gets or sets the next. </summary>
    /// <value> The next. </value>
    public virtual LinkedListCodec? Next { get; set; }


    /// <summary>
    /// Encodes -- that is: serializes -- an object into a XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <remarks>
    /// Encodes -- that is: serializes -- an object into a XDR stream in compliance to RFC 1832.
    /// </remarks>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public virtual void Encode( XdrEncodingStreamBase encoder )
    {
        LinkedListCodec? current = this;
        do
        {
            encoder.EncodeInt( current.Foo );
            current = current.Next;
            encoder.EncodeBoolean( current is not null );
        } while ( current is not null );
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
        LinkedListCodec? current = this;
        LinkedListCodec? nextItem;
        do
        {
            current.Foo = decoder.DecodeInt();
            nextItem = decoder.DecodeBoolean() ? new LinkedListCodec() : null;
            current.Next = nextItem;
            current = nextItem;
        } while ( current is not null );
    }

}
