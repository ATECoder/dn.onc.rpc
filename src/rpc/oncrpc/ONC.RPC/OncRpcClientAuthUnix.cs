using static System.Net.WebRequestMethods;

namespace cc.isr.ONC.RPC;

/// <summary>
/// The <see cref="OncRpcClientAuthUnix"/> class handles protocol issues of ONC/RPC 
/// <see cref="OncRpcAuthType.OncRpcAuthTypeUnix"/> (and thus <see cref="OncRpcAuthType.OncRpcAuthTypeShortHandUnix"/>)
/// authentication as defined in <see href="https://www.ibm.com/docs/en/aix/7.1?topic=authentication-unix"/>
/// </summary>
/// <remarks> <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public class OncRpcClientAuthUnix : OncRpcClientAuthBase
{
    /// <summary>
    /// Constructs a new <see cref="OncRpcClientAuthUnix"/> authentication protocol handling object
    /// capable of handling <see cref="OncRpcAuthType.OncRpcAuthTypeUnix"/> and
    /// <see cref="OncRpcAuthType.OncRpcAuthTypeShortHandUnix"/>.
    /// </summary>
    /// <remarks>
    /// Please note that the credential information is typically only unique within a particular
    /// domain of machines, user IDs and group IDs.
    /// </remarks>
    /// <param name="machinename">  Name of the caller's machine (like "ebankruptcy-dot-com", just
    ///                             for instance...). </param>
    /// <param name="uid">          Caller's effective user ID. </param>
    /// <param name="gid">          Caller's effective group ID. </param>
    /// <param name="gids">         Array of group IDs the caller is a member of. </param>
    public OncRpcClientAuthUnix( string machinename, int uid, int gid, int[] gids )
    {
        this.Timestamp = ( int ) (DateTime.Now.Ticks / (TimeSpan.TicksPerMillisecond * 1000));
        this.machinename = machinename;
        this.uid = uid;
        this.gid = gid;
        this.gids = gids;
    }

    /// <summary>
    /// Constructs a new <see cref="OncRpcClientAuthUnix"/> authentication protocol handling object
    /// capable of handling <see cref="OncRpcAuthType.OncRpcAuthTypeUnix"/>
    /// and <see cref="OncRpcAuthType.OncRpcAuthTypeShortHandUnix"/>.
    /// </summary>
    /// <remarks>
    /// Please note that the credential information is typically only
    /// unique within a particular domain of machines, user IDs and group IDs.
    /// </remarks>
    /// <param name="machinename">  Name of the caller's machine (like "ebankruptcy-dot-com", just
    ///                             for instance...). </param>
    /// <param name="uid">          Caller's effective user ID. </param>
    /// <param name="gid">          Caller's effective group ID. </param>
    public OncRpcClientAuthUnix( string machinename, int uid, int gid ) : this( machinename, uid, gid, Array.Empty<int>() )
    {
    }

    /// <summary>
    /// Encodes ONC/RPC authentication information in form of a credential and a verifier when
    /// sending an ONC/RPC call message.
    /// </summary>
    /// <remarks>
    /// The <see cref="OncRpcAuthType.OncRpcAuthTypeUnix"/> authentication method only uses the credential
    /// but no verifier. If the ONC/RPC server sent a <see cref="OncRpcAuthType.OncRpcAuthTypeShortHandUnix"/>
    /// "shorthand" credential together with the previous reply message, it
    /// is used instead of the original credential.
    /// </remarks>
    /// <exception cref="OncRpcAuthenticationException">    Thrown when an ONC/RPC Authentication
    ///                                                     error condition occurs. </exception>
    /// <param name="encoder">  XDR stream where to encode the credential and the verifier to. </param>
    ///
    /// <exception cref="OncRpcException">          Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="System.IO.IOException">    Thrown when an I/O error condition occurs. </exception>
    internal override void EncodeCredentialAndVerfier( XdrEncodingStreamBase encoder )
    {
        if ( this.shorthandCred == null )
        {

            // Encode the credential, which contains some unsecured information
            // about user and group ID, etc. Note that the credential itself
            // is encoded as a variable-sized bunch of octets.

            if ( this.gids.Length > OncRpcAuthConstants.OncRpcMaxAllowedGroups || this.machinename.Length > OncRpcAuthConstants.OncRpcMaxMachineNameLength )
                throw new OncRpcAuthenticationException( OncRpcAuthStatus.OncRpcAuthFailed );
            encoder.EncodeInt( OncRpcAuthType.OncRpcAuthTypeUnix );

            // length = length of timestamp + length of machine name + length of user id + length of group id + length of vector of group ids

            int len = 4 + (this.machinename.Length + 7 & ~3) + 4 + 4 + this.gids.Length * 4 + 4;
            if ( len > OncRpcAuthConstants.OncRpcMaxAuthBytes )
                throw new OncRpcAuthenticationException( OncRpcAuthStatus.OncRpcAuthFailed );
            encoder.EncodeInt( len );
            encoder.EncodeInt( this.Timestamp );
            encoder.EncodeString( this.machinename );
            encoder.EncodeInt( this.uid );
            encoder.EncodeInt( this.gid );
            encoder.EncodeIntVector( this.gids );
        }
        else
        {

            // Use shorthand credential instead of original credential.

            encoder.EncodeInt( OncRpcAuthType.OncRpcAuthTypeShortHandUnix );
            encoder.EncodeDynamicOpaque( this.shorthandCred );
        }

        // We also need to encode the verifier, which is always of type 'none'.

        encoder.EncodeInt( OncRpcAuthType.OncRpcAuthTypeNone );
        encoder.EncodeInt( 0 );
    }

    /// <summary>
    /// Decodes ONC/RPC authentication information in form of a verifier when receiving an ONC/RPC
    /// reply message.
    /// </summary>
    /// <exception cref="OncRpcAuthenticationException">    if the received verifier is not kosher. </exception>
    /// <param name="decoder">  XDR stream from which to receive the verifier sent together with an
    ///                         ONC/RPC reply message. </param>
    ///
    /// <exception cref="OncRpcException">          Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="System.IO.IOException">    Thrown when an I/O error condition occurs. </exception>
    internal override void DecodeVerfier( XdrDecodingStreamBase decoder )
    {
        switch ( decoder.DecodeInt() )
        {
            case OncRpcAuthType.OncRpcAuthTypeNone:
                {

                    // The verifier sent in response to 'UNIX' or 'SHORT' authentication credentials
                    // can only be 'none' or 'short'. In the latter case we drop
                    // any old shorthand credential and use the new one.


                    // Make sure that the verifier does not contain any opaque data.
                    // Anything different from this is not kosher and an authentication
                    // exception will be thrown.

                    if ( decoder.DecodeInt() != 0 )
                        throw new OncRpcAuthenticationException( OncRpcAuthStatus.OncRpcAuthFailed );
                    break;
                }

            case OncRpcAuthType.OncRpcAuthTypeShortHandUnix:
                {

                    // Fetch the credential from the XDR stream and make sure that
                    // it does conform to the length restriction as set forth in
                    // the ONC/RPC protocol.

                    this.shorthandCred = decoder.DecodeDynamicOpaque();
                    if ( this.shorthandCred.Length > OncRpcAuthConstants.OncRpcMaxAuthBytes )
                        throw new OncRpcAuthenticationException( OncRpcAuthStatus.OncRpcAuthFailed );
                    break;
                }

            default:
                {

                    // Do not accept any other kind of verifier sent.

                    throw new OncRpcAuthenticationException( OncRpcAuthStatus.OncRpcAuthInvalidResponse );
                }
        }
    }

    /// <summary>   Indicates whether the ONC/RPC authentication credential can be refreshed. </summary>
    /// <returns>   true, if the credential can be refreshed. </returns>
    public override bool CanRefreshCredential()
    {

        // If we don't use a shorthand credential at this time, then there's
        // no hope to refresh the credentials.

        if ( this.shorthandCred == null )
            return false;

        // Otherwise just dump the shorthand credentials and let the caller
        // retry. This will probably result in the ONC/RPC server replying
        // with a new shorthand credential.

        this.shorthandCred = null;

        // Ah, yes. We need to update the "stamp" (more a timestamp, but
        // Sun coding style is sometimes interesting). As is my style too.

        this.Timestamp = ( int ) (DateTime.Now.Ticks / (TimeSpan.TicksPerMillisecond * 1000));

        // Oh, yes. We can refresh the credential. Maybe.

        return true;
    }

    /// <summary>   Gets or sets the timestamp as supplied through credential. </summary>
    /// <value> The timestamp. </value>
    public int Timestamp { get; set; }

    /// <summary>Contains the machine name of caller supplied through credential.</summary>
    private string machinename;

    /// <summary>   Sets the machine name information in the credential. </summary>
    /// <param name="machinename">  Machine name. </param>
    public virtual void setMachinename( string machinename )
    {
        this.machinename = machinename;
    }

    /// <summary>   Returns the machine name information from the credential. </summary>
    /// <returns>   machine name. </returns>
    public virtual string getMachinename()
    {
        return this.machinename;
    }

    /// <summary>   Sets the user ID in the credential. </summary>
    /// <param name="uid">  User ID. </param>
    public virtual void setUid( int uid )
    {
        this.uid = uid;
    }

    /// <summary>   Returns the user ID from the credential. </summary>
    /// <returns>   user ID. </returns>
    public virtual int getUid()
    {
        return this.uid;
    }

    /// <summary>   Sets the group ID in the credential. </summary>
    /// <param name="gid">  Group ID. </param>
    public virtual void setGid( int gid )
    {
        this.gid = gid;
    }

    /// <summary>   Returns the group ID from the credential. </summary>
    /// <returns>   group ID. </returns>
    public virtual int getGid()
    {
        return this.gid;
    }

    /// <summary>   Sets the group IDs in the credential. </summary>
    /// <param name="gids"> Array of group IDs. </param>
    public virtual void setGids( int[] gids )
    {
        this.gids = gids;
    }

    /// <summary>   Returns the group IDs from the credential. </summary>
    /// <returns>   array of group IDs. </returns>
    public virtual int[] getGids()
    {
        return this.gids;
    }

    /// <summary>Contains the user ID of caller supplied through credential.</summary>
    private int uid;

    /// <summary>Contains the group ID of caller supplied through credential.</summary>
    private int gid;

    /// <summary>
    /// Contains a set of group IDs the caller belongs to, as supplied
    /// through credential.
    /// </summary>
    private int[] gids;

    /// <summary>
    /// Holds the "shorthand" credentials of type <see cref="OncRpcAuthType.OncRpcAuthTypeShortHandUnix"/>
    /// optionally returned by an ONC/RPC server to be used on subsequent
    /// ONC/RPC calls.
    /// </summary>
    private byte[] shorthandCred;

}
