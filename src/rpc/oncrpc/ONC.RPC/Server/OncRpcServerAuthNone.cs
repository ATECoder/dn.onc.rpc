namespace cc.isr.ONC.RPC.Server;

/// <summary>
/// The server <see cref="OncRpcServerAuthNone"/> class handles all protocol issues of the ONC/RPC
/// authentication <see cref="OncRpcAuthType.OncRpcAuthTypeNone"/> on the server side.
/// </summary>
/// <remarks> <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public sealed class OncRpcServerAuthNone : OncRpcServerAuthBase
{

    /// <summary>   Default constructor. </summary>
    public OncRpcServerAuthNone() : base( OncRpcAuthType.OncRpcAuthTypeNone )
    {
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an ONC/RPC authentication object (credential and
    /// verifier) on the server side.
    /// </summary>
    /// <exception cref="OncRpcAuthException">    Thrown when an ONC/RPC Authentication
    ///                                                     error condition occurs. </exception>
    /// <param name="decoder">  XDR stream from which the authentication object is restored. </param>
    ///
    /// <exception cref="OncRpcException">          Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="System.IO.IOException">    Thrown when an I/O error condition occurs. </exception>
    public sealed override void DecodeCredentialAndVerfier( XdrDecodingStreamBase decoder )
    {

        // As the authentication type has already been pulled off the XDR
        // stream, we only need to make sure that really no opaque data follows.

        if ( decoder.DecodeInt() != 0 )
            throw new OncRpcAuthException( OncRpcAuthStatus.OncRpcAuthBadCredential );

        // We also need to decode the verifier. This must be of type
        // 'none' too. For some obscure historical reasons, we have to
        // deal with credentials and verifiers, although they belong together,
        // according to Sun's specification.

        if ( decoder.DecodeInt() != OncRpcAuthType.OncRpcAuthTypeNone || decoder.DecodeInt() != 0 )
            throw new OncRpcAuthException( OncRpcAuthStatus.OncRpcAutoBadVerifier );
    }

    /// <summary>
    /// Encodes -- that is: serializes -- an ONC/RPC authentication object (its verifier) on the
    /// server side.
    /// </summary>
    /// <param name="encoder">  XDR stream from which the authentication object is restored. </param>
    ///
    /// <exception cref="OncRpcException">          Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="System.IO.IOException">    Thrown when an I/O error condition occurs. </exception>
    public sealed override void EncodeVerfier( XdrEncodingStreamBase encoder )
    {
        // Encode an 'none' verifier with zero length.
        encoder.EncodeInt( OncRpcAuthType.OncRpcAuthTypeNone );
        encoder.EncodeInt( 0 );
    }

    /// <summary>
    /// (Immutable)
    /// Singleton to use when an authentication object for <see cref="OncRpcAuthType.OncRpcAuthTypeNone"/>
    /// is needed.
    /// </summary>
    public static readonly OncRpcServerAuthNone AuthNoneInstance = new();
}
