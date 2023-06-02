using cc.isr.ONC.RPC.EnumExtensions;

namespace cc.isr.ONC.RPC.Client;

/// <summary>
/// The <see cref="OncRpcClientAuthNone"/> class handles protocol issues of ONC/RPC 
/// <see cref="OncRpcAuthType.OncRpcAuthTypeNone"/> authentication.
/// </summary>
/// <remarks> <para>
///  
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public class OncRpcClientAuthNone : OncRpcClientAuthBase
{

    /// <summary>   (Immutable) the default type of type the 'none' authentication. </summary>
    public const OncRpcAuthType AuthTypeDefault = OncRpcAuthType.OncRpcAuthTypeNone;

    /// <summary>   (Immutable) the default length of the 'none' authentication message. </summary>
    public const int AuthMessageLengthDefault = 0;

    /// <summary>   (Immutable) the default authentication type of the verifier. </summary>
    public const OncRpcAuthType VerifierAuthTypeDefault = OncRpcAuthType.OncRpcAuthTypeNone;

    /// <summary>   (Immutable) the default length of the verifier authentication message. </summary>
    public const int VerifierAuthMessageLengthDefault = 0;

    /// <summary>   Default constructor. </summary>
    public OncRpcClientAuthNone() : base( OncRpcClientAuthNone.AuthTypeDefault )
    {
        this.AuthMessageLength = OncRpcClientAuthNone.AuthMessageLengthDefault;
    }

    /// <summary>
    /// Encodes ONC/RPC authentication information in form of a credential and a verifier when
    /// sending an ONC/RPC call message.
    /// </summary>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="encoder">  XDR stream where to encode the credential and the verifier to. </param>
    internal override void EncodeCredentialAndVerfier( XdrEncodingStreamBase encoder )
    {

        // The credential only consists of the indication of no authentication (none) with
        // no opaque authentication data following.
        encoder.EncodeInt( ( int ) this.AuthType );
        encoder.EncodeInt( this.AuthMessageLength );

        // But we also need to encode the verifier. This is always of type
        // none too. For some obscure historical reasons, we have to
        // deal with credentials and verifiers, although they belong together,
        // according to Sun's specification.
        encoder.EncodeInt( ( int ) OncRpcClientAuthNone.VerifierAuthTypeDefault );
        encoder.EncodeInt( OncRpcClientAuthNone.VerifierAuthMessageLengthDefault );
    }

    /// <summary>
    /// Decodes ONC/RPC authentication information in form of a verifier when receiving an ONC/RPC
    /// reply message.
    /// </summary>
    /// <exception cref="OncRpcAuthException">    if the received verifier is not kosher. </exception>
    /// <param name="decoder">  XDR stream from which to receive the verifier sent together with an
    ///                         ONC/RPC reply message. </param>
    internal override void DecodeVerfier( XdrDecodingStreamBase decoder )
    {
        // Make sure that we received a 'none' verifier and that it
        // does not contain any opaque data. Anything different from this
        // is not kosher and an authentication exception will be thrown.
        this.VerifierAuthType = decoder.DecodeInt().ToAuthType();
        this.VerifierAuthMessageLength = decoder.DecodeInt();
        // @ATECoder: was:
        // if ( decoder.DecodeInt() != ( int ) OncRpcClientAuthNone.AuthType || decoder.DecodeInt() != OncRpcClientAuthNone.AuthMessageLength )
        if ( this.VerifierAuthType != Server.OncRpcServerAuthNone.VerifierAuthTypeDefault
            || this.VerifierAuthMessageLength != Server.OncRpcServerAuthNone.VerifierAuthMessageLengthDefault )
            throw new OncRpcAuthException( OncRpcAuthStatus.OncRpcAuthFailed );
    }

    /// <summary>   Indicates whether the ONC/RPC authentication credential can be refreshed. </summary>
    /// <returns>   true, if the credential can be refreshed. </returns>
    public override bool CanRefreshCredential()
    {
        // Nothing to do here, as 'none' doesn't know anything of credential refreshing. How refreshing...
        return false;
    }

}
