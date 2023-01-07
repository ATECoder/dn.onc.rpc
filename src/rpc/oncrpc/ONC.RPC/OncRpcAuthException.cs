namespace cc.isr.ONC.RPC;

/// <summary>
/// The class <see cref="OncRpcAuthException"/> indicates an authentication exception.
/// </summary>
/// <remarks> <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
[Serializable]
public class OncRpcAuthException : OncRpcException
{
    /// <summary>
    /// Initializes an <see cref="OncRpcAuthException"/>
    /// with a reason of <see cref="OncRpcException.OncRpcAuthenticationError"/>
    /// and the specified <see cref="OncRpcAuthStatus">authentication status</see> failure reason.
    /// </summary>
    /// <param name="authStatus">   The authentication status, which can be any one of the
    ///                             <see cref="OncRpcAuthStatus"/>. </param>
    public OncRpcAuthException( int authStatus ) : base( OncRpcException.OncRpcAuthenticationError )
    {
        this.AuthStatus = authStatus;
    }

    /// <summary>
    /// Gets or sets pr set the specific authentication status (reason why this authentication
    /// exception was thrown).
    /// </summary>
    /// <value> The authentication status of this exception. </value>
    public int AuthStatus { get; private set; }
}