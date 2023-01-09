namespace cc.isr.ONC.RPC.Server;

/// <summary>
/// The server <see cref="OncRpcServerAuthNone"/> class handles all protocol issues of the ONC/RPC
/// authentication <see cref="OncRpcAuthType.OncRpcAuthTypeUnix"/> on the server side.
/// </summary>
/// <remarks> <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public sealed class OncRpcServerAuthUnix : OncRpcServerAuthBase
{
    /// <summary>   Gets or sets (private) the timestamp as supplied through credential. </summary>
    /// <value> The timestamp. </value>
    public int Timestamp { get; private set; }

    /// <summary>   Gets or sets (private) the machine name of caller supplied through credential. </summary>
    public string MachineName { get; private set; }

    /// <summary>   Gets or sets (private) the user ID of caller supplied through credential. </summary>
    public int UserId { get; private set; }

    /// <summary>   Gets or sets (private) the group ID of caller supplied through credential. </summary>
    public int GroupId { get; private set; }

    /// <summary>
    /// Contains a set of group IDs the caller belongs to, as supplied through credential.
    /// </summary>
    private int[] _groupIds;

    /// <summary>
    /// Gets the group identifiers the caller belongs to, as supplied through credential.
    /// </summary>
    /// <remarks>   2023-01-03. </remarks>
    /// <returns>   An array of int. </returns>
    public int[] GetGroupIds()
    {
        return this._groupIds;
    }

    /// <summary>
    /// Constructs an server <see cref="OncRpcServerAuthUnix"/> object and pulls its state off an XDR stream.
    /// </summary>
    /// <param name="decoder">  XDR stream to retrieve the object state from. </param>
    ///
    /// <exception cref="OncRpcException">          Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="System.IO.IOException">    Thrown when an I/O error condition occurs. </exception>
    public OncRpcServerAuthUnix( XdrDecodingStreamBase decoder ) : base( OncRpcAuthType.OncRpcAuthTypeUnix )
    {
        this.DecodeCredentialAndVerfier( decoder );
    }

    /// <summary>
    /// Contains the shorthand authentication verifier (credential) to return
    /// to the caller to be used with the next ONC/RPC calls.
    /// </summary>
    private byte[] _shorthandVerfier;

    /// <summary>   Sets the shorthand verifier to be sent back to the caller. </summary>
    /// <remarks>
    /// The caller then can use this shorthand verifier as the new credential with the next ONC/RPC
    /// calls to speed up things up (hopefully).
    /// </remarks>
    /// <param name="shorthandVerfier">    Contains the shorthand authentication verifier (credential)
    ///                                 to return to the caller to be used with the next ONC/RPC calls. </param>
    public void SetShorthandVerifier( byte[] shorthandVerfier )
    {
        this._shorthandVerfier = shorthandVerfier;
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
    /// <exception cref="OncRpcException">                  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="OncRpcAuthException">    Thrown when an ONC/RPC Authentication
    ///                                                     error condition occurs. </exception>
    /// <exception cref="System.IO.IOException">            Thrown when an I/O error condition occurs. </exception>
    /// <param name="decoder">  XDR stream from which the authentication object is restored. </param>
    public sealed override void DecodeCredentialAndVerfier( XdrDecodingStreamBase decoder )
    {

        // Reset some part of the object's state...

        this._shorthandVerfier = null;

        // Now pull off the object state of the XDR stream...

        int realLen = decoder.DecodeInt();
        this.Timestamp = decoder.DecodeInt();
        this.MachineName = decoder.DecodeString();
        this.UserId = decoder.DecodeInt();
        this.GroupId = decoder.DecodeInt();
        this._groupIds = decoder.DecodeIntVector();

        // Make sure that the indicated length of the opaque data is kosher.
        // If not, throw an exception, as there is something strange going on!

        // length = length of timestamp + length of machine name + length of user id +
        //          length of group id + length of the vector of group identities. 

        int len = 4 + (this.MachineName.Length + 7 & ~3) + 4 + 4 + this._groupIds.Length * 4 + 4;
        if ( realLen != len )
            if ( realLen < len )
                throw new OncRpcException( OncRpcExceptionReason.OncRpcBufferUnderflow );
            else
                throw new OncRpcException( OncRpcExceptionReason.OncRpcAuthenticationError );

        // We also need to decode the verifier. This must be of type
        // 'none' too. For some obscure historical reasons, we have to
        // deal with credentials and verifiers, although they belong together,
        // according to Sun's specification.

        if ( decoder.DecodeInt() != ( int ) OncRpcAuthType.OncRpcAuthTypeNone || decoder.DecodeInt() != 0 )
            throw new OncRpcAuthException( OncRpcAuthStatus.OncRpcAutoBadVerifier );
    }

    /// <summary>
    /// Encodes -- that is: serializes -- an ONC/RPC authentication object (its verifier) on the
    /// server side.
    /// </summary>
    /// <remarks>   2023-01-07. </remarks>
    /// <param name="encoder">  XDR stream from which the authentication object is restored. </param>
    ///
    /// ### <exception cref="OncRpcException">          Thrown when an ONC/RPC error condition
    ///                                                 occurs. </exception>
    /// ### <exception cref="System.IO.IOException">    Thrown when an I/O error condition occurs. </exception>
    public sealed override void EncodeVerfier( XdrEncodingStreamBase encoder )
    {
        if ( this._shorthandVerfier is not null )
        {

            // Encode 'short' shorthand verifier (credential).

            encoder.EncodeInt( ( int ) OncRpcAuthType.OncRpcAuthTypeShortHandUnix );
            encoder.EncodeDynamicOpaque( this._shorthandVerfier );
        }
        else
        {

            // Encode an 'none' verifier with zero length, if no shorthand
            // verifier (credential) has been supplied by now.

            encoder.EncodeInt( ( int ) OncRpcAuthType.OncRpcAuthTypeNone );
            encoder.EncodeInt( 0 );
        }
    }
}
