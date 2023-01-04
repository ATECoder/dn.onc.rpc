namespace cc.isr.ONC.RPC;

/// <summary>
/// A collection of constants used to identify the authentication status (or any authentication
/// errors) in ONC/RPC replies of the corresponding ONC/RPC calls.
/// </summary>
/// <remarks> <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public class OncRpcAuthStatus
{
    /// <summary>   (Immutable) There is no authentication problem or error. <para>
    /// Renamed from ONCRPC_AUTH_OK = 0. </para> </summary>
    public const int OncRpcAuthOkay = 0;

    /// <summary>
    /// (Immutable)
    /// The ONC/RPC server detected a bad credential (that is, the seal was broken). <para>
    /// Renamed from ONCRPC_AUTH_BADCRED = 1. </para>
    /// </summary>
    public const int OncRpcAuthBadCredential = 1;

    /// <summary>
    /// (Immutable)
    /// The ONC/RPC server has rejected the credential and forces the caller to begin a new session. <para>
    /// Renamed from ONCRPC_AUTH_REJECTEDCRED = 2. </para>
    /// </summary>
    public const int OncRpcAuthRejectedCredential = 2;

    /// <summary>
    /// (Immutable)
    /// The ONC/RPC server detected a bad verifier (that is, the seal was broken). <para>
    /// Renamed from ONCRPC_AUTH_BADVERF = 3. </para>
    /// </summary>
    public const int OncRpcAutoBadVerifier = 3;

    /// <summary>
    /// (Immutable)
    /// The ONC/RPC server detected an expired verifier (which can also happen if the verifier was
    /// replayed). <para>
    /// Renamed from ONCRPC_AUTH_REJECTEDVERF = 4. </para>
    /// </summary>
    public const int OncRpcAuthRejectedVerifier = 4;

    /// <summary>
    /// (Immutable) The ONC/RPC server rejected the authentication for security reasons. <para>
    /// Renamed from ONCRPC_AUTH_TOOWEAK = 5. </para>
    /// </summary>
    public const int OncRpcAuthTooWeak = 5;

    /// <summary>   (Immutable) The ONC/RPC client detected a bogus response verifier. <para>
    /// Renamed from ONCRPC_AUTH_INVALIDRESP = 6. </para> </summary>
    public const int OncRpcAuthInvalidResponse = 6;

    /// <summary>
    /// (Immutable) Authentication at the ONC/RPC client failed for an unknown reason. <para>
    /// Renamed from ONCRPC_AUTH_FAILED = 7. </para>
    /// </summary>
    public const int OncRpcAuthFailed = 7;
}
