
namespace cc.isr.ONC.RPC.Server;

/// <summary>
/// The server <see cref="OncRpcServerAuthBase"/> class is the base class and factory for handling all
/// protocol issues of ONC/RPC authentication on the server side.
/// </summary>
/// <remarks> <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public abstract class OncRpcServerAuthBase
{

    /// <summary>   Specialized constructor for use only by derived class. </summary>
    /// <param name="authenticationType">   Authentication type used by this authentication object. </param>
    protected OncRpcServerAuthBase( int authenticationType )
    {
        this.AuthenticationType = authenticationType;
    }

    /// <summary>
    /// Gets or sets or set (private) the type (flavor) of <see cref="OncRpcAuthType">authentication</see>
    /// used.
    /// </summary>
    /// <value>   Authentication type used by this authentication object. </value>
    public int AuthenticationType { get; private set; }

    /// <summary>   Restores (deserializes) an authentication object from an XDR stream. </summary>
    /// <param name="decoder">  XDR stream from which the authentication object is restored. </param>
    /// <param name="recycle">  old authentication object which is intended to be reused in case it
    ///                         is of the same authentication type as the new one just arriving from
    ///                         the XDR stream. </param>
    /// <returns>
    /// Authentication information encapsulated in an object, whose class is derived from 
    /// <see cref="OncRpcServerAuthBase"/>.
    /// </returns>
    ///
    /// <exception cref="OncRpcException">                Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="System.IO.IOException">          Thrown when an I/O error condition occurs. </exception>
    /// <exception cref="OncRpcAuthException">  Thrown when an ONC/RPC Authentication
    ///                                                   error condition occurs. </exception>
    public static OncRpcServerAuthBase NewOncRpcServerAuto( XdrDecodingStreamBase decoder, OncRpcServerAuthBase recycle )
    {
        OncRpcServerAuthBase auth;

        // In case we got an old authentication object and we are just about
        // to receive an authentication with the same type, we reuse the old
        // object.

        int authType = decoder.DecodeInt();
        if ( recycle is not null && recycle.AuthenticationType == authType )
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

                        auth = OncRpcServerAuthNone.AuthNoneInstance;
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
    /// <param name="decoder">  XDR stream from which the authentication object is restored. </param>
    /// <exception cref="OncRpcException">          Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="System.IO.IOException">    Thrown when an I/O error condition occurs. </exception>
    public abstract void DecodeCredentialAndVerfier( XdrDecodingStreamBase decoder );

    /// <summary>
    /// Encodes -- that is: serializes -- an ONC/RPC authentication object (its verifier) on the
    /// server side.
    /// </summary>
    /// <param name="encoder">  XDR stream from which the authentication object is restored. </param>
    /// <exception cref="OncRpcException">          Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="System.IO.IOException">    Thrown when an I/O error condition occurs. </exception>
    public abstract void EncodeVerfier( XdrEncodingStreamBase encoder );
}
