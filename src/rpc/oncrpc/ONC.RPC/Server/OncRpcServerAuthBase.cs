
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
    /// <summary>
    /// Returns the type (flavor) of <see cref="OncRpcAuthType">authentication</see> used.
    /// </summary>
    /// <returns>   Authentication type used by this authentication object. </returns>
    public abstract int GetAuthenticationType();

    /// <summary>   Restores (deserializes) an authentication object from an XDR stream. </summary>
    /// <param name="xdr">      XDR stream from which the authentication object is restored. </param>
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
    /// <exception cref="OncRpcAuthenticationException">  Thrown when an ONC/RPC Authentication
    ///                                                   error condition occurs. </exception>
    public static OncRpcServerAuthBase NewOncRpcServerAuto( XdrDecodingStreamBase xdr, OncRpcServerAuthBase recycle )
    {
        OncRpcServerAuthBase auth;

        // In case we got an old authentication object and we are just about
        // to receive an authentication with the same type, we reuse the old
        // object.

        int authType = xdr.DecodeInt();
        if ( recycle != null && recycle.GetAuthenticationType() == authType )
        {

            // Simply recycle authentication object and pull its new state
            // of the XDR stream.

            auth = recycle;
            auth.DecodeCredentialAndVerfier( xdr );
        }
        else
            switch ( authType )
            {
                case OncRpcAuthType.OncRpcAuthTypeNone:
                    {

                        // Create a new authentication object and pull its state off
                        // the XDR stream.

                        auth = OncRpcServerAuthNone.AuthNoneInstance;
                        auth.DecodeCredentialAndVerfier( xdr );
                        break;
                    }

                case OncRpcAuthType.OncRpcAuthShortHandUnix:
                    {
                        auth = new OncRpcServerAuthShort( xdr );
                        break;
                    }

                case OncRpcAuthType.OncRpcAuthTypeUnix:
                    {
                        auth = new OncRpcServerAuthUnix( xdr );
                        break;
                    }

                default:
                    {

                        // In case of an unknown or unsupported type, throw an exception.
                        // Note: using 'rejected credentials' status is in sync with the way Sun's
                        // ONC/RPC implementation does it. But don't ask me why they do
                        // it this way...!

                        throw new OncRpcAuthenticationException( OncRpcAuthStatus.OncRpcAuthRejectedCredential );
                    }
            }
        return auth;
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an ONC/RPC authentication object (credential and
    /// verifier) on the server side.
    /// </summary>
    /// <param name="xdr">  XDR stream from which the authentication object is restored. </param>
    /// <exception cref="OncRpcException">          Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="System.IO.IOException">    Thrown when an I/O error condition occurs. </exception>
    public abstract void DecodeCredentialAndVerfier( XdrDecodingStreamBase xdr );

    /// <summary>
    /// Encodes -- that is: serializes -- an ONC/RPC authentication object (its verifier) on the
    /// server side.
    /// </summary>
    /// <param name="xdr">  XDR stream from which the authentication object is restored. </param>
    /// <exception cref="OncRpcException">          Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="System.IO.IOException">    Thrown when an I/O error condition occurs. </exception>
    public abstract void EncodeVerfier( XdrEncodingStreamBase xdr );
}
