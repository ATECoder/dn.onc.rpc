namespace cc.isr.ONC.RPC.Server;

/// <summary>
/// The server <see cref="OncRpcServerAuthShort"/> class handles all protocol issues of the ONC/RPC
/// authentication <see cref="OncRpcAuthType.OncRpcAuthShortHandUnix"/> on the server side.
/// </summary>
/// <remarks> <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public sealed class OncRpcServerAuthShort : OncRpcServerAuthBase
{
    /// <summary>
    /// Constructs an server <see cref="OncRpcServerAuthShort"/> object and pulls its state off an XDR stream.
    /// </summary>
    /// <param name="xdr">  XDR stream to retrieve the object state from. </param>
    ///
    /// <exception cref="OncRpcException">          Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="System.IO.IOException">    Thrown when an I/O error condition occurs. </exception>
    public OncRpcServerAuthShort( XdrDecodingStreamBase xdr )
    {
        this.DecodeCredentialAndVerfier( xdr );
    }

    /// <summary>
    /// Returns the type (flavor) of <see cref="OncRpcAuthType">authentication</see> used.
    /// </summary>
    /// <returns>   Authentication type used by this authentication object. </returns>
    public sealed override int GetAuthenticationType()
    {
        return OncRpcAuthType.OncRpcAuthShortHandUnix;
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
    /// <param name="xdr">  XDR stream from which the authentication object is restored. </param>
    ///
    /// <exception cref="OncRpcAuthenticationException">    Thrown when an ONC/RPC Authentication
    ///                                                     error condition occurs. </exception>
    /// <exception cref="OncRpcException">                  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="System.IO.IOException">            Thrown when an I/O error condition occurs. </exception>
    public sealed override void DecodeCredentialAndVerfier( XdrDecodingStreamBase xdr )
    {

        // Reset the authentication object's state properly...

        this._shorthandCredential = null;
        this._shorthandVerfier = null;

        // Pull off the shorthand credential information (opaque date) of
        // the XDR stream...

        this._shorthandCredential = xdr.DecodeDynamicOpaque();
        if ( this._shorthandCredential.Length > OncRpcAuthConstants.OncRpcMaxAuthBytes )
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
        if ( this._shorthandVerfier != null )
        {

            // Encode 'short' shorthand verifier (credential).

            xdr.EncodeInt( OncRpcAuthType.OncRpcAuthShortHandUnix );
            xdr.EncodeDynamicOpaque( this._shorthandVerfier );
        }
        else
        {

            // Encode an 'none' verifier with zero length, if no shorthand
            // verifier (credential) has been supplied by now.

            xdr.EncodeInt( OncRpcAuthType.OncRpcAuthTypeNone );
            xdr.EncodeInt( 0 );
        }
    }

}
