using cc.isr.ONC.RPC.EnumExtensions;

namespace cc.isr.ONC.RPC.Server;

/// <summary>
/// The server <see cref="OncRpcServerAuthShort"/> class handles all protocol issues of the ONC/RPC
/// authentication <see cref="OncRpcAuthType.OncRpcAuthTypeShortHandUnix"/> on the server side.
/// </summary>
/// <remarks> <para>
///  
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public sealed class OncRpcServerAuthShort : OncRpcServerAuthBase
{

    /// <summary>   (Immutable) the default type of type the 'SHORT UNIX' authentication. </summary>
    public const OncRpcAuthType AuthTypeDefault = OncRpcAuthType.OncRpcAuthTypeShortHandUnix;

    /// <summary>   (Immutable) the default authentication type of the verifier. </summary>
    public const OncRpcAuthType VerifierAuthTypeDefault = OncRpcAuthType.OncRpcAuthTypeShortHandUnix;

    /// <summary>   (Immutable) the default alternate authentication type of the verifier. </summary>
    public const OncRpcAuthType AlternateVerifierAuthTypeDefault = OncRpcAuthType.OncRpcAuthTypeNone;

    /// <summary>   (Immutable) the default length of the alternate verifier authentication message. </summary>
    public const int AlternateVerifierAuthMessageLengthDefault = 0;

    /// <summary>
    /// Constructs an server <see cref="OncRpcServerAuthShort"/> object and pulls its state off an XDR stream.
    /// </summary>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="decoder">  XDR stream to retrieve the object state from. </param>
    public OncRpcServerAuthShort( XdrDecodingStreamBase decoder ) : base( OncRpcServerAuthShort.AuthTypeDefault )
    {
        this.DecodeCredentialAndVerfier( decoder );
        this._shorthandVerfier = Array.Empty<byte>();
        this._shorthandCredential = Array.Empty<byte>();
    }

    /// <summary> Contains the shorthand credential sent by the caller.</summary>
    private byte[] _shorthandCredential;

    /// <summary>   Returns the shorthand credential sent by the caller. </summary>
    /// <returns>   An array of byte. </returns>
    public byte[] GetShorthandCredential()
    {
        return this._shorthandCredential;
    }

    /// <summary>
    /// Contains the shorthand authentication verifier (credential) to return
    /// to the caller to be used with the next ONC/RPC calls.
    /// </summary>
    private byte[] _shorthandVerfier;

    /// <summary>   Sets shorthand verifier to be sent back to the caller. </summary>
    /// <remarks>
    /// The caller then can use this shorthand verifier as the new credential with the next ONC/RPC
    /// calls. If you don't set the verifier or set it to (<see langword="null"/>), then the verifier
    /// returned to the caller will be of type <see cref="OncRpcAuthType.OncRpcAuthTypeNone"/>.
    /// </remarks>
    /// <param name="verifier">    Contains the shorthand authentication verifier (credential)
    ///                            to return to the caller to be used with the next ONC/RPC calls. </param>
    public void SetShorthandVerifier( byte[] verifier )
    {
        this._shorthandVerfier = verifier ?? Array.Empty<byte>();
    }

    /// <summary>   Returns the shorthand verifier to be sent back to the caller. </summary>
    /// <returns>   An array of byte. </returns>
    public byte[] GetShorthandVerifier()
    {
        return this._shorthandVerfier;
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an ONC/RPC authentication object (credential and
    /// verifier) on the server side.
    /// </summary>
    /// <remarks>
    /// The value of the discriminant in the response verifier of the reply message from the server
    /// is either <see cref="OncRpcAuthType.OncRpcAuthTypeNone"/> or <see cref="OncRpcAuthType.OncRpcAuthTypeShortHandUnix"/>.
    /// If the value is the later, the bytes of the response verifier's string encode an opaque structure.
    /// The new opaque structure can then be passed to the server in place of the original 
    /// <see cref="OncRpcAuthType.OncRpcAuthTypeUnix"/> credentials. The server maintains a cache that
    /// maps shorthand opaque structures (passed back by way of an AUTH_SHORT-style response
    /// verifier) to the original credentials of the caller. The caller saves network bandwidth and
    /// server CPU time when the shorthand credentials are used.
    /// </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="decoder">  XDR stream from which the authentication object is restored. </param>
    public sealed override void DecodeCredentialAndVerfier( XdrDecodingStreamBase decoder )
    {

        // Reset the authentication object's state properly...

        this._shorthandCredential = Array.Empty<byte>();
        this._shorthandVerfier = Array.Empty<byte>();

        // Pull off the shorthand credential information (opaque date) of
        // the XDR stream...

        this._shorthandCredential = decoder.DecodeDynamicOpaque();
        if ( this._shorthandCredential.Length > OncRpcAuthConstants.OncRpcMaxAuthBytes )
            throw new OncRpcAuthException( OncRpcAuthStatus.OncRpcAuthBadCredential );

        // We also need to decode the verifier. This must be of type
        // 'none' too. For some obscure historical reasons, we have to
        // deal with credentials and verifiers, although they belong together,
        // according to Sun's specification.

        this.VerifierAuthType = decoder.DecodeInt().ToAuthType();
        this.VerifierAuthMessageLength = decoder.DecodeInt();
        // @ATECoder: was:
        // if ( decoder.DecodeInt() != ( int ) OncRpcAuthType.OncRpcAuthTypeNone || decoder.DecodeInt() != 0 )
        if ( this.VerifierAuthType != Client.OncRpcClientAuthUnix.VerifierAuthTypeDefault
            || this.VerifierAuthMessageLength != Client.OncRpcClientAuthUnix.VerifierAuthMessageLengthDefault )
            throw new OncRpcAuthException( OncRpcAuthStatus.OncRpcAutoBadVerifier );
    }

    /// <summary>
    /// Encodes -- that is: serializes -- an ONC/RPC authentication object (its verifier) on the
    /// server side.
    /// </summary>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="encoder">  XDR stream from which the authentication object is restored. </param>
    public sealed override void EncodeVerfier( XdrEncodingStreamBase encoder )
    {
        if ( this._shorthandVerfier is not null )
        {

            // Encode 'short' shorthand verifier (credential).

            encoder.EncodeInt( ( int ) OncRpcServerAuthShort.VerifierAuthTypeDefault );
            encoder.EncodeDynamicOpaque( this._shorthandVerfier );
        }
        else
        {

            // Encode an 'none' verifier with zero length, if no shorthand
            // verifier (credential) has been supplied by now.

            encoder.EncodeInt( ( int ) OncRpcServerAuthShort.AlternateVerifierAuthTypeDefault );
            encoder.EncodeInt( OncRpcServerAuthShort.AlternateVerifierAuthMessageLengthDefault );
        }
    }

}
