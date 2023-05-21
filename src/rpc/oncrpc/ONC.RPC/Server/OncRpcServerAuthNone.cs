using cc.isr.ONC.RPC.EnumExtensions;

namespace cc.isr.ONC.RPC.Server;

/// <summary>
/// The server <see cref="OncRpcServerAuthNone"/> class handles all protocol issues of the ONC/RPC
/// authentication <see cref="OncRpcAuthType.OncRpcAuthTypeNone"/> on the server side.
/// </summary>
/// <remarks> <para>
/// 
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public sealed class OncRpcServerAuthNone : OncRpcServerAuthBase
{

    /// <summary>   (Immutable) the default type of type the 'NONE' authentication. </summary>
    public const OncRpcAuthType AuthTypeDefault = OncRpcAuthType.OncRpcAuthTypeNone;

    /// <summary>   (Immutable) the default length of the 'none' authentication message. </summary>
    public const int AuthMessageLengthDefault = 0;

    /// <summary>   (Immutable) the default authentication type of the verifier. </summary>
    public const OncRpcAuthType VerifierAuthTypeDefault = OncRpcAuthType.OncRpcAuthTypeNone;

    /// <summary>   (Immutable) the default length of the verifier authentication message. </summary>
    public const int VerifierAuthMessageLengthDefault = 0;

    #region " construction "

    /// <summary>   Default constructor. </summary>
    public OncRpcServerAuthNone() : base( OncRpcServerAuthNone.AuthTypeDefault )
    { }
    /// <summary>
    /// Decodes -- that is: deserializes -- an ONC/RPC authentication object (credential and
    /// verifier) on the server side.
    /// </summary>
    /// <exception cref="OncRpcException">      Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="OncRpcAuthException">  Thrown when an ONC/RPC Authentication
    ///                                         error condition occurs. </exception>
    /// <param name="decoder">  XDR stream from which the authentication object is restored. </param>
    public sealed override void DecodeCredentialAndVerfier( XdrDecodingStreamBase decoder )
    {

        // As the authentication type has already been pulled off the XDR
        // stream, we only need to make sure that really no opaque data follows.
        this.AuthMessageLength = decoder.DecodeInt();
        if ( this.AuthMessageLength != 0 )
            throw new OncRpcAuthException( OncRpcAuthStatus.OncRpcAuthBadCredential );

        // We also need to decode the verifier and length. The verifier must be of type
        // 'none' too with a length of 0. For some obscure historical reasons, we have to
        // deal with credentials and verifiers, although they belong together,
        // according to Sun's specification.

        this.VerifierAuthType = decoder.DecodeInt().ToAuthType();
        this.VerifierAuthMessageLength = decoder.DecodeInt();
        // @atecoder was:
        // if ( decoder.DecodeInt() != ( int ) OncRpcServerAuthNone.AuthType || decoder.DecodeInt() != OncRpcServerAuthNone.AuthMessageLength )

        if ( this.VerifierAuthType != this.AuthType || this.VerifierAuthMessageLength != this.AuthMessageLength )
            throw new OncRpcAuthException( OncRpcAuthStatus.OncRpcAutoBadVerifier );
    }

    #endregion

    #region " members "

    /// <summary>
    /// Encodes -- that is: serializes -- an ONC/RPC authentication object (its verifier) on the
    /// server side.
    /// </summary>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="encoder">  XDR stream from which the authentication object is restored. </param>
    public sealed override void EncodeVerfier( XdrEncodingStreamBase encoder )
    {
        // Encode an 'none' verifier with zero length.
        encoder.EncodeInt( ( int ) OncRpcAuthType.OncRpcAuthTypeNone );
        encoder.EncodeInt( 0 );
    }

    #endregion

}
