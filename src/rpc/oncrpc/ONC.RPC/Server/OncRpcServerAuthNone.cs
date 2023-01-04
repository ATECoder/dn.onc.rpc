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
    /// <summary>
    /// Returns the type (flavor) of <see cref="OncRpcAuthType">authentication</see> used.
    /// </summary>
    /// <returns>   Authentication type used by this authentication object. </returns>
    public sealed override int GetAuthenticationType()
    {
        return OncRpcAuthType.OncRpcAuthTypeNone;
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an ONC/RPC authentication object (credential and
    /// verifier) on the server side.
    /// </summary>
    /// <exception cref="OncRpcAuthenticationException">    Thrown when an ONC/RPC Authentication
    ///                                                     error condition occurs. </exception>
    /// <param name="xdr">  XDR stream from which the authentication object is restored. </param>
    ///
    /// <exception cref="OncRpcException">          Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="System.IO.IOException">    Thrown when an I/O error condition occurs. </exception>
    public sealed override void DecodeCredentialAndVerfier( XdrDecodingStreamBase xdr )
    {

        // As the authentication type has already been pulled off the XDR
        // stream, we only need to make sure that really no opaque data follows.

        if ( xdr.DecodeInt() != 0 )
            throw new OncRpcAuthenticationException( OncRpcAuthStatus.OncRpcAuthBadCredential );

        // We also need to decode the verifier. This must be of type
        // 'none' too. For some obscure historical reasons, we have to
        // deal with credentials and verifiers, although they belong together,
        // according to Sun's specification.

        if ( xdr.DecodeInt() != OncRpcAuthType.OncRpcAuthTypeNone || xdr.DecodeInt() != 0 )
            throw new OncRpcAuthenticationException( OncRpcAuthStatus.OncRpcAutoBadVerifier );
    }

    /// <summary>
    /// Encodes -- that is: serializes -- an ONC/RPC authentication object (its verifier) on the
    /// server side.
    /// </summary>
    /// <param name="xdr">  XDR stream from which the authentication object is restored. </param>
    ///
    /// <exception cref="OncRpcException">          Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="System.IO.IOException">    Thrown when an I/O error condition occurs. </exception>
    public sealed override void EncodeVerfier( XdrEncodingStreamBase xdr )
    {
        // Encode an 'none' verifier with zero length.
        xdr.EncodeInt( OncRpcAuthType.OncRpcAuthTypeNone );
        xdr.EncodeInt( 0 );
    }

    /// <summary>
    /// (Immutable)
    /// Singleton to use when an authentication object for <see cref="OncRpcAuthType.OncRpcAuthTypeNone"/>
    /// is needed.
    /// </summary>
    public static readonly OncRpcServerAuthNone AuthNoneInstance = new();
}
