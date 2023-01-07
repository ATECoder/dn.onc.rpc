namespace cc.isr.ONC.RPC.Server;

/// <summary>
/// The server <see cref="OncRpcServerAuthShort"/> class handles all protocol issues of the ONC/RPC
/// authentication <see cref="OncRpcAuthType.OncRpcAuthTypeShortHandUnix"/> on the server side.
/// </summary>
/// <remarks> <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public sealed class OncRpcServerAuthShort : OncRpcServerAuthBase
{
    /// <summary>
    /// Constructs an server <see cref="OncRpcServerAuthShort"/> object and pulls its state off an XDR stream.
    /// </summary>
    /// <param name="decoder">  XDR stream to retrieve the object state from. </param>
    ///
    /// <exception cref="OncRpcException">          Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="System.IO.IOException">    Thrown when an I/O error condition occurs. </exception>
    public OncRpcServerAuthShort( XdrDecodingStreamBase decoder ) : base( OncRpcAuthType.OncRpcAuthTypeShortHandUnix )
    {
        this.DecodeCredentialAndVerfier( decoder );
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
    /// calls. If you don't set the verifier or set it to <see langword="null"/>, then the verifier
    /// returned to the caller will be of type <see cref="OncRpcAuthType.OncRpcAuthTypeNone"/>.
    /// </remarks>
    /// <param name="shorthandVerf">    Contains the shorthand authentication verifier (credential)
    ///                                 to return to the caller to be used with the next ONC/RPC calls. </param>
    public void SetShorthandVerifier( byte[] shorthandVerf )
    {
        this._shorthandVerfier = shorthandVerf;
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
    /// <param name="decoder">  XDR stream from which the authentication object is restored. </param>
    ///
    /// <exception cref="OncRpcAuthException">    Thrown when an ONC/RPC Authentication
    ///                                                     error condition occurs. </exception>
    /// <exception cref="OncRpcException">                  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="System.IO.IOException">            Thrown when an I/O error condition occurs. </exception>
    public sealed override void DecodeCredentialAndVerfier( XdrDecodingStreamBase decoder )
    {

        // Reset the authentication object's state properly...

        this._shorthandCredential = null;
        this._shorthandVerfier = null;

        // Pull off the shorthand credential information (opaque date) of
        // the XDR stream...

        this._shorthandCredential = decoder.DecodeDynamicOpaque();
        if ( this._shorthandCredential.Length > OncRpcAuthConstants.OncRpcMaxAuthBytes )
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
        if ( this._shorthandVerfier != null )
        {

            // Encode 'short' shorthand verifier (credential).

            encoder.EncodeInt( OncRpcAuthType.OncRpcAuthTypeShortHandUnix );
            encoder.EncodeDynamicOpaque( this._shorthandVerfier );
        }
        else
        {

            // Encode an 'none' verifier with zero length, if no shorthand
            // verifier (credential) has been supplied by now.

            encoder.EncodeInt( OncRpcAuthType.OncRpcAuthTypeNone );
            encoder.EncodeInt( 0 );
        }
    }

}
