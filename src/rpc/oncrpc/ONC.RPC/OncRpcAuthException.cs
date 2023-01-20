namespace cc.isr.ONC.RPC;

/// <summary>
/// The class <see cref="OncRpcAuthException"/> indicates an authentication exception.
/// </summary>
/// <remarks> <para>
/// 
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
[Serializable]
public class OncRpcAuthException : OncRpcException
{
    /// <summary>
    /// Initializes an <see cref="OncRpcAuthException"/>
    /// with a reason of <see cref="OncRpcExceptionReason.OncRpcAuthenticationError"/>
    /// and the specified <see cref="OncRpcAuthStatus">authentication status</see> failure reason.
    /// </summary>
    /// <param name="authStatus">   The authentication status, which can be any one of the
    ///                             <see cref="OncRpcAuthStatus"/>. </param>
    public OncRpcAuthException( OncRpcAuthStatus authStatus ) : base( OncRpcExceptionReason.OncRpcAuthenticationError )
    {
        this.AuthStatus = authStatus;
    }

    /// <summary>
    /// Constructs an <see cref="OncRpcAuthException"/> with the specified detail reason and message.
    /// </summary>
    /// <param name="suffixMessage">    Message to append to default message. </param>
    /// <param name="authStatus">       The authentication status, which can be any one of the
    ///                                 <see cref="OncRpcAuthStatus"/>. </param>
    public OncRpcAuthException( string suffixMessage, OncRpcAuthStatus authStatus ) : base( suffixMessage, OncRpcExceptionReason.OncRpcAuthenticationError )
    {
        this.AuthStatus = authStatus;
    }

    /// <summary>
    /// Gets or sets pr set the specific <see cref="OncRpcAuthStatus"/>, that is, the reason why this
    /// authentication exception was thrown.
    /// </summary>
    /// <value> The authentication status of this exception. </value>
    public OncRpcAuthStatus AuthStatus { get; private set; }
}
