#nullable disable


namespace cc.isr.ONC.RPC.MSTest.Codecs;


/// <summary>   (Serializable) a binary tree 2 XBR encoder/decoder. </summary>
/// <remarks>   2022-12-30. </remarks>
[Serializable]
public class BinaryTree2Codec : IXdrCodec
{
    /// <summary>   Default constructor. </summary>
    /// <remarks>   2022-12-22. </remarks>
    public BinaryTree2Codec()
    {
    }

    /// <summary>   Constructor. </summary>
    /// <remarks>   2022-12-22. </remarks>
    /// <param name="xdr">  XDR stream from which decoded information is retrieved. </param>
    public BinaryTree2Codec( XdrDecodingStreamBase xdr )
    {
        this.Decode( xdr );
    }

    /// <summary>   Gets or sets the key. </summary>
    /// <value> The key. </value>
    public string Key { get; set; }

    /// <summary>   Gets or sets the value. </summary>
    /// <value> The value. </value>
    public string Value { get; set; }

    /// <summary>   Gets or sets the left. </summary>
    /// <value> The left. </value>
    public BinaryTree2Codec Left { get; set; }

    /// <summary>   Gets or sets the right. </summary>
    /// <value> The right. </value>
    public BinaryTree2Codec Right { get; set; }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into a XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <remarks>
    /// Encodes -- that is: serializes -- an object into a XDR stream in compliance to RFC 1832.
    /// </remarks>
    /// <param name="xdr">  XDR stream to which information is sent for encoding. </param>
    public virtual void Encode( XdrEncodingStreamBase xdr )
    {
        BinaryTree2Codec currentBinaryTree = this;
        do
        {
            xdr.EncodeString( currentBinaryTree.Key );
            xdr.EncodeString( currentBinaryTree.Value );
            if ( currentBinaryTree.Left != null )
            {
                xdr.EcodeBoolean( true );
                currentBinaryTree.Left.Encode( xdr );
            }
            else
                xdr.EcodeBoolean( false );
;
            currentBinaryTree = currentBinaryTree.Right;
            xdr.EcodeBoolean( currentBinaryTree != null );
        } while ( currentBinaryTree != null );
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
        BinaryTree2Codec currentBinaryTree = this;
        BinaryTree2Codec nextBinaryTree;
        do
        {
            currentBinaryTree.Key = xdr.DecodeString();
            currentBinaryTree.Value = xdr.DecodeString();
            currentBinaryTree.Left = xdr.DecodeBoolean() ? new BinaryTree2Codec( xdr ) : null;
            nextBinaryTree = xdr.DecodeBoolean() ? new BinaryTree2Codec() : null;
            currentBinaryTree.Right = nextBinaryTree;
            currentBinaryTree = nextBinaryTree;
        } while ( currentBinaryTree != null );
    }

}
