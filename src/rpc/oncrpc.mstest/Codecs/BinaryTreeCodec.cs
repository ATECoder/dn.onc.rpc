namespace cc.isr.ONC.RPC.MSTest.Codecs;

/// <summary>   (Serializable) a binary tree XBR encoder/decoder. </summary>
[Serializable]
public class BinaryTreeCodec : IXdrCodec
{

    /* Remote Tea leftover:
     The serialization runtime associates with each serializable class a version number, called a serialVersionUID, 
     which is used during deserialization to verify that the sender and receiver of a serialized object have loaded 
     classes for that object that are compatible with respect to serialization. If the receiver has loaded a class 
     for the object that has a different serialVersionUID than that of the corresponding sender's class, then deserialization 
     will result in an InvalidClassException. A serializable class can declare its own serialVersionUID explicitly by declaring 
     a field named serialVersionUID that must be static, final, and of type 
     private long serialVersionUID = 2403962346676670641L;
    */

    /// <summary>   Default constructor. </summary>
    public BinaryTreeCodec()
    {
        this.Key = string.Empty;
        this.Value = string.Empty;
        this.Left = new BinaryTreeCodec();
        this.Right = null;
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public BinaryTreeCodec( XdrDecodingStreamBase decoder ) : this()
    {
        this.Decode( decoder );
    }

    /// <summary>   Gets or sets the key. </summary>
    /// <value> The key. </value>
    public virtual string Key { get; set; }

    /// <summary>   Gets or sets the value. </summary>
    /// <value> The value. </value>
    public virtual string Value { get; set; }

    /// <summary>   Gets or sets the left. </summary>
    /// <value> The left. </value>
    public virtual BinaryTreeCodec Left { get; set; }

    /// <summary>   Gets or sets the right. </summary>
    /// <value> The right. </value>
    public virtual BinaryTreeCodec? Right { get; set; }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into a XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <remarks>
    /// Encodes -- that is: serializes -- an object into a XDR stream in compliance to RFC 1832.
    /// </remarks>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public virtual void Encode( XdrEncodingStreamBase encoder )
    {
        BinaryTreeCodec? currentBinaryTree = this;
        do
        {
            encoder.EncodeString( currentBinaryTree.Key );
            encoder.EncodeString( currentBinaryTree.Value );
            if ( currentBinaryTree.Left is not null )
            {
                encoder.EcodeBoolean( true );
                currentBinaryTree.Left.Encode( encoder );
            }
            else
                encoder.EcodeBoolean( false );
            ;
            currentBinaryTree = currentBinaryTree.Right;
            encoder.EcodeBoolean( currentBinaryTree is not null );
        } while ( currentBinaryTree is not null );
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
        BinaryTreeCodec? currentBinaryTree = this;
        BinaryTreeCodec? nextBinaryTree;
        do
        {
            currentBinaryTree.Key = decoder.DecodeString();
            currentBinaryTree.Value = decoder.DecodeString();
            if ( decoder.DecodeBoolean() )
                currentBinaryTree.Left.Decode( decoder );
            nextBinaryTree = decoder.DecodeBoolean() ? new BinaryTreeCodec() : null;
            currentBinaryTree.Right = nextBinaryTree;
            currentBinaryTree = nextBinaryTree;
        } while ( currentBinaryTree is not null );
    }

}
