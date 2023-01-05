namespace cc.isr.ONC.RPC;

/// <summary>
/// The <see cref="OncRpcClientCallMessage"/> class represents a remote procedure call message on
/// the client side.
/// </summary>
/// <remarks> <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public class OncRpcClientCallMessage : OncRpcCallMessageBase
{
    /// <summary>   Constructs and initializes a new ONC/RPC call message header. </summary>
    /// <param name="messageId">    An identifier chosen by an ONC/RPC client to uniquely identify
    ///                             matching call and reply messages. </param>
    /// <param name="program">      Program number of the remote procedure to call. </param>
    /// <param name="version">      Program version number of the remote procedure to call. </param>
    /// <param name="procedure">    Procedure number (identifier) of the procedure to call. </param>
    /// <param name="auth">         Authentication protocol handling object. </param>
    public OncRpcClientCallMessage( int messageId, int program, int version,
        int procedure, OncRpcClientAuthBase auth ) : base( messageId, program, version, procedure )
    {
        this.Auth = auth;
    }

    /// <summary>
    /// Encodes -- that is: serializes -- a ONC/RPC message header object into a XDR stream according
    /// to RFC 1831.
    /// </summary>
    /// <param name="encoder">  An encoding XDR stream where to put the mess in. </param>
    ///
    /// <exception cref="OncRpcException">          Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="System.IO.IOException">    Thrown when an I/O error condition occurs. </exception>
    public virtual void Encode( XdrEncodingStreamBase encoder )
    {
        encoder.EncodeInt( this.MessageId );
        encoder.EncodeInt( this.MessageType );
        encoder.EncodeInt( this.ProtocolVersion );
        encoder.EncodeInt( this.Program );
        encoder.EncodeInt( this.Version );
        encoder.EncodeInt( this.Procedure );

        // Now encode the authentication data. If we have an authentication
        // protocol handling object at hand, then we let do the dirty work
        // for us. Otherwise, we fall back to 'none' handling.

        if ( this.Auth != null )
            this.Auth.EncodeCredentialAndVerfier( encoder );
        else
        {
            encoder.EncodeInt( OncRpcAuthType.OncRpcAuthTypeNone );
            encoder.EncodeInt( 0 );
            encoder.EncodeInt( OncRpcAuthType.OncRpcAuthTypeNone );
            encoder.EncodeInt( 0 );
        }
    }

    /// <summary>
    /// Client-side authentication protocol handling object to use when decoding the reply message.
    /// </summary>
    /// <value> The authentication. </value>
    internal OncRpcClientAuthBase Auth { get; private set; }
}
