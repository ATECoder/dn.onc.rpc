
namespace cc.isr.ONC.RPC.Client;

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
    /// <param name="machinename">  Name of the caller's machine (like "coder-dot-com", just
    ///                             for instance...). </param>
    /// <param name="userId">          Caller's effective user ID. </param>
    /// <param name="groupId">         Caller's effective group ID. </param>
    /// <param name="groupIds">        Array of group IDs the caller is a member of. </param>
    public OncRpcClientAuthUnix( string machinename, int userId, int groupId, int[] groupIds )
    {
        this.Timestamp = ( int ) (DateTime.Now.Ticks / (TimeSpan.TicksPerMillisecond * 1000));
        this.MachineName = machinename;
        this.UserId = userId;
        this.GroupId = groupId;
        this._groupIds = groupIds;
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
    /// <param name="machinename">  Name of the caller's machine (like "coder-dot-com", just
    ///                             for instance...). </param>
    /// <param name="userId">       Caller's effective user ID. </param>
    /// <param name="groupId">      Caller's effective group ID. </param>
    public OncRpcClientAuthUnix( string machinename, int userId, int groupId ) : this( machinename, userId, groupId, Array.Empty<int>() )
    {
    }

    /// <summary>
    /// Encodes ONC/RPC authentication information in form of a credential and a verifier when
    /// sending an ONC/RPC call message.
    /// </summary>
    /// <remarks>
    /// The <see cref="OncRpcAuthType.OncRpcAuthTypeUnix"/> authentication method only uses the credential
    /// but no verifier. If the ONC/RPC server sent a <see cref="OncRpcAuthType.OncRpcAuthTypeShortHandUnix"/>
    /// "shorthand" credential together with the previous reply message, it is used instead of the original credential.
    /// </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    internal override void EncodeCredentialAndVerfier( XdrEncodingStreamBase encoder )
    {
        if ( this._shorthandCredentials == null )
        {

            // Encode the credential, which contains some unsecured information
            // about user and group ID, etc. Note that the credential itself
            // is encoded as a variable-sized bunch of octets.

            if ( this._groupIds.Length > OncRpcAuthConstants.OncRpcMaxAllowedGroups || this.MachineName.Length > OncRpcAuthConstants.OncRpcMaxMachineNameLength )
                throw new OncRpcAuthException( OncRpcAuthStatus.OncRpcAuthFailed );
            encoder.EncodeInt( ( int ) OncRpcAuthType.OncRpcAuthTypeUnix );

            // length = length of timestamp + length of machine name + length of user id + length of group id + length of vector of group ids

            int len = 4 + (this.MachineName.Length + 7 & ~3) + 4 + 4 + this._groupIds.Length * 4 + 4;
            if ( len > OncRpcAuthConstants.OncRpcMaxAuthBytes )
                throw new OncRpcAuthException( OncRpcAuthStatus.OncRpcAuthFailed );
            encoder.EncodeInt( len );
            encoder.EncodeInt( this.Timestamp );
            encoder.EncodeString( this.MachineName );
            encoder.EncodeInt( this.UserId );
            encoder.EncodeInt( this.GroupId );
            encoder.EncodeIntVector( this._groupIds );
        }
        else
        {

            // Use shorthand credential instead of original credential.

            encoder.EncodeInt( ( int ) OncRpcAuthType.OncRpcAuthTypeShortHandUnix );
            encoder.EncodeDynamicOpaque( this._shorthandCredentials );
        }

        // We also need to encode the verifier, which is always of type 'none'.

        encoder.EncodeInt( ( int ) OncRpcAuthType.OncRpcAuthTypeNone );
        // and the length the 'none' credentials.
        encoder.EncodeInt( 0 );
    }

    /// <summary>
    /// Decodes ONC/RPC authentication information in form of a verifier when receiving an ONC/RPC
    /// reply message.
    /// </summary>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="decoder">  XDR stream from which to receive the verifier sent together with an
    ///                         ONC/RPC reply message. </param>
    internal override void DecodeVerfier( XdrDecodingStreamBase decoder )
    {
        switch ( ( OncRpcAuthType ) decoder.DecodeInt() )
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
                        throw new OncRpcAuthException( OncRpcAuthStatus.OncRpcAuthFailed );
                    break;
                }

            case OncRpcAuthType.OncRpcAuthTypeShortHandUnix:
                {

                    // Fetch the credential from the XDR stream and make sure that
                    // it does conform to the length restriction as set forth in
                    // the ONC/RPC protocol.

                    this._shorthandCredentials = decoder.DecodeDynamicOpaque();
                    if ( this._shorthandCredentials.Length > OncRpcAuthConstants.OncRpcMaxAuthBytes )
                        throw new OncRpcAuthException( OncRpcAuthStatus.OncRpcAuthFailed );
                    break;
                }

            default:
                {

                    // Do not accept any other kind of verifier sent.

                    throw new OncRpcAuthException( OncRpcAuthStatus.OncRpcAuthInvalidResponse );
                }
        }
    }

    /// <summary>   Indicates whether the ONC/RPC authentication credential can be refreshed. </summary>
    /// <returns>   true, if the credential can be refreshed. </returns>
    public override bool CanRefreshCredential()
    {

        // If we don't use a shorthand credential at this time, then there's
        // no hope to refresh the credentials.

        if ( this._shorthandCredentials == null )
            return false;

        // Otherwise just dump the shorthand credentials and let the caller
        // retry. This will probably result in the ONC/RPC server replying
        // with a new shorthand credential.

        this._shorthandCredentials = null;

        // Ah, yes. We need to update the "stamp" (more a timestamp, but
        // Sun coding style is sometimes interesting). As is my style too.

        this.Timestamp = ( int ) (DateTime.Now.Ticks / (TimeSpan.TicksPerMillisecond * 1000));

        // Oh, yes. We can refresh the credential. Maybe.

        return true;
    }

    /// <summary>   Gets or sets the timestamp as supplied through credential. </summary>
    /// <value> The timestamp. </value>
    public int Timestamp { get; set; }

    /// <summary>   Gets or sets the machine name of caller supplied through credential. </summary>
    /// <value> The machine name. </value>
    public string MachineName { get; set; }

    /// <summary>   Gets or sets the user ID of caller supplied through credential. </summary>
    /// <value> The identifier of the user. </value>
    private int UserId { get; set; }

    /// <summary>Contains the group ID of caller supplied through credential.</summary>
    private int GroupId { get; set; }

    private int[] _groupIds;

    /// <summary>   Sets the group IDs in the credential. </summary>
    /// <param name="gids"> Array of group IDs. </param>
    public virtual void SetGroupIds( int[] gids )
    {
        this._groupIds = gids;
    }

    /// <summary>   Returns the group IDs from the credential. </summary>
    /// <returns>   array of group IDs. </returns>
    public virtual int[] GetGroupIds()
    {
        return this._groupIds;
    }

    /// <summary>
    /// Holds the "shorthand" credentials of type <see cref="OncRpcAuthType.OncRpcAuthTypeShortHandUnix"/>
    /// optionally returned by an ONC/RPC server to be used on subsequent
    /// ONC/RPC calls.
    /// </summary>
    private byte[] _shorthandCredentials;

}
