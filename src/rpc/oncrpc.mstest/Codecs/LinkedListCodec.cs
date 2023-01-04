namespace cc.isr.ONC.RPC.MSTest.Codecs;

using System;

#nullable disable

/// <summary>   (Serializable) linked list  XBR encoder/decoder. </summary>
/// <remarks>   2022-12-22. </remarks>
[Serializable]
public class LinkedListCodec : IXdrCodec
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
    private const long serialVersionUID = -9187504517170663946L;

    /// <summary>   Gets or sets the foo. </summary>
    /// <value> The foo. </value>
    public virtual int Foo { get; set; }

    /// <summary>   Gets or sets the next. </summary>
    /// <value> The next. </value>
    public virtual LinkedListCodec Next { get; set; }

    /// <summary>   Default constructor. </summary>
    /// <remarks>   2022-12-22. </remarks>
    public LinkedListCodec()
    {
    }

    /// <summary>   Constructor. </summary>
    /// <remarks>   2022-12-22. </remarks>
    /// <param name="xdr">  XDR stream from which decoded information is retrieved. </param>
    public LinkedListCodec( XdrDecodingStreamBase xdr )
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
        LinkedListCodec current = this;
        do
        {
            xdr.EncodeInt( current.Foo );
            current = current.Next;
            xdr.EcodeBoolean( current != null );
        } while ( current != null );
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
        LinkedListCodec current = this;
        LinkedListCodec nextItem;
        do
        {
            current.Foo = xdr.DecodeInt();
            nextItem = xdr.DecodeBoolean() ? new LinkedListCodec() : null;
            current.Next = nextItem;
            current = nextItem;
        } while ( current != null );
    }

}
