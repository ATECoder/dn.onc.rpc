namespace cc.isr.ONC.RPC.Client;

/// <summary>
/// The <see cref="OncRpcClientAuthBase"/> class is the base class for handling all protocol issues of
/// ONC/RPC authentication on the client side.
/// </summary>
/// <remarks>
/// As it stands, it does not do very much with the exception of defining the contract for the
/// behavior of derived classes with respect to protocol handling issues. <para>
/// 
/// Authentication on the client side can be done as follows: just
/// create an authentication object and hand it over to the ONC/RPC client object.
/// <code>
/// OncRpcClientAuthBase auth = new OncRpcClientAuthUnix( "marvin@ford.prefect", 42, 1001, Array.Empty&lt;int&gt;() );
/// client.SetAuth( auth );
/// </code>
/// The <see cref="OncRpcClientAuthUnix"/> <see cref="OncRpcAuthType.OncRpcAuthTypeUnix"/>
/// will handle shorthand credentials (of type <see cref="OncRpcAuthType.OncRpcAuthTypeShortHandUnix"/> transparently). If you do
/// not set any authentication object after creating an ONC/RPC client object, <see cref="OncRpcAuthType.OncRpcAuthTypeNone"/>
/// is used automatically.  </para> <para>
/// 
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public abstract class OncRpcClientAuthBase
{

    /// <summary>   Specialized constructor for use only by derived class. </summary>
    /// <param name="authType"> The type of the authentication. </param>
    protected OncRpcClientAuthBase( OncRpcAuthType authType )
    {
        this.AuthType = authType;
        this.AuthMessageLength = 0;
    }

    /// <summary>
    /// Gets or sets or set ( <see langword="private"/>) type of the authentication for this
    /// authentication class.
    /// </summary>
    /// <value> The type of the authentication. </value>
    public OncRpcAuthType AuthType { get; private set; } = OncRpcAuthType.OncRpcAuthTypeNone;

    /// <summary>
    /// Gets or sets ( <see langword="protected"/> ) the length of the authentication message for
    /// this authentication class.
    /// </summary>
    /// <value> The length of the authentication message. </value>
    public int AuthMessageLength { get; protected set; } = 0;

    /// <summary>   Gets or sets ( <see langword="protected"/> ) the type of the verifier authentication. </summary>
    /// <value> The type of the verifier authentication. </value>
    public OncRpcAuthType? VerifierAuthType { get; protected set; }

    /// <summary>
    /// Gets or sets ( <see langword="protected"/> ) the length of the verifier authentication
    /// message.
    /// </summary>
    /// <value> The length of the verifier authentication. </value>
    public int? VerifierAuthMessageLength { get; protected set; }

    /// <summary>
    /// Encodes ONC/RPC authentication information in form of a credential and a verifier when
    /// sending an ONC/RPC call message.
    /// </summary>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="encoder">  XDR stream where to encode the credential and the verifier to. </param>
    internal abstract void EncodeCredentialAndVerfier( XdrEncodingStreamBase encoder );

    /// <summary>
    /// Decodes ONC/RPC authentication information in form of a verifier when receiving an ONC/RPC
    /// reply message.
    /// </summary>
    /// <exception cref="OncRpcAuthException">    Thrown if the received verifier is not kosher. </exception>
    /// <exception cref="OncRpcException">        Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="decoder">  XDR stream from which to receive the verifier sent together with an
    ///                     ONC/RPC reply message. </param>
    internal abstract void DecodeVerfier( XdrDecodingStreamBase decoder );

    /// <summary>   Indicates whether the ONC/RPC authentication credential can be refreshed. </summary>
    /// <returns>   true, if the credential can be refreshed. </returns>
    public abstract bool CanRefreshCredential();
}
