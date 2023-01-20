
using cc.isr.ONC.RPC.EnumExtensions;

namespace cc.isr.ONC.RPC.Server;

/// <summary>
/// The server <see cref="OncRpcServerAuthBase"/> class is the base class and factory for handling all
/// protocol issues of ONC/RPC authentication on the server side.
/// </summary>
/// <remarks> <para>
/// 
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public abstract class OncRpcServerAuthBase
{

    /// <summary>   Specialized constructor for use only by derived class. </summary>
    /// <param name="authType">   Authentication type used by this authentication object. </param>
    protected OncRpcServerAuthBase( OncRpcAuthType authType )
    {
        this.AuthType = authType;
        this.AuthMessageLength = 0;
    }

    /// <summary>
    /// Gets or sets or set (<see langword="private"/>) (<see langword="private"/>) the type (flavor) of <see cref="OncRpcAuthType">authentication type</see>
    /// used.
    /// </summary>
    /// <value>   Authentication type used by this authentication object. </value>
    public OncRpcAuthType AuthType { get; private set; }

    /// <summary>
    /// Gets or sets ( <see langword="protected"/> ) the length of the authentication message for
    /// this authentication class.
    /// </summary>
    /// <value> The length of the authentication message. </value>
    public int AuthMessageLength { get; protected set; } = 0;

    /// <summary>   Gets or sets ( <see langword="protected"/> ) the type of the verifier authentication. </summary>
    /// <value> The type of the verifier authentication. </value>
    public OncRpcAuthType? VerifierAuthType { get; protected set; }

    /// <summary>   Gets or sets ( <see langword="protected"/> ) the length of the verifier authentication message. </summary>
    /// <value> The length of the verifier authentication. </value>
    public int? VerifierAuthMessageLength { get; protected set; }

    /// <summary>   Restores (deserializes) an authentication object from an XDR stream. </summary>
    /// <exception cref="OncRpcAuthException">  Thrown when an ONC/RPC Authentication error condition occurs. </exception>
    /// <exception cref="OncRpcException">      Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="decoder">  XDR stream from which the authentication object is restored. </param>
    /// <param name="recycle">  old authentication object which is intended to be reused in case it
    ///                         is of the same authentication type as the new one just arriving from
    ///                         the XDR stream. </param>
    /// <returns>
    /// Authentication information encapsulated in an object, whose class is derived from 
    /// <see cref="OncRpcServerAuthBase"/>.
    /// </returns>
    public static OncRpcServerAuthBase NewOncRpcServerAuth( XdrDecodingStreamBase decoder, OncRpcServerAuthBase recycle )
    {
        OncRpcServerAuthBase auth;

        // In case we got an old authentication object and we are just about
        // to receive an authentication with the same type, we reuse the old
        // object.

        OncRpcAuthType authType = decoder.DecodeInt().ToAuthType();
        if ( recycle is not null && recycle.AuthType == authType )
        {

            // Simply recycle authentication object and pull its new state
            // of the XDR stream.

            auth = recycle;
            auth.DecodeCredentialAndVerfier( decoder );
        }
        else
            switch ( authType )
            {
                case OncRpcAuthType.OncRpcAuthTypeNone:
                    {

                        // Create a new authentication object and pull its state off
                        // the XDR stream.

                        auth = new OncRpcServerAuthNone();
                        auth.DecodeCredentialAndVerfier( decoder );
                        break;
                    }

                case OncRpcAuthType.OncRpcAuthTypeShortHandUnix:
                    {
                        auth = new OncRpcServerAuthShort( decoder );
                        break;
                    }

                case OncRpcAuthType.OncRpcAuthTypeUnix:
                    {
                        auth = new OncRpcServerAuthUnix( decoder );
                        break;
                    }

                default:
                    {

                        // In case of an unknown or unsupported type, throw an exception.
                        // Note: using 'rejected credentials' status is in sync with the way Sun's
                        // ONC/RPC implementation does it. But don't ask me why they do
                        // it this way...!

                        throw new OncRpcAuthException( OncRpcAuthStatus.OncRpcAuthRejectedCredential );
                    }
            }
        return auth;
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an ONC/RPC authentication object (credential and
    /// verifier) on the server side.
    /// </summary>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="decoder">  XDR stream from which the authentication object is restored. </param>
    public abstract void DecodeCredentialAndVerfier( XdrDecodingStreamBase decoder );

    /// <summary>
    /// Encodes -- that is: serializes -- an ONC/RPC authentication object (its verifier) on the
    /// server side.
    /// </summary>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="encoder">  XDR stream from which the authentication object is restored. </param>
    public abstract void EncodeVerfier( XdrEncodingStreamBase encoder );
}
