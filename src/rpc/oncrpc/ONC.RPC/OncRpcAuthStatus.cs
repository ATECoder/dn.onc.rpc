using System.ComponentModel;

namespace cc.isr.ONC.RPC;

/// <summary>
/// A collection of constants used to identify the authentication status (or any authentication
/// errors) in ONC/RPC replies of the corresponding ONC/RPC calls.
/// </summary>
/// <remarks> <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public enum OncRpcAuthStatus
{
    /// <summary>   There is no authentication problem or error. <para>
    /// Renamed from <c>ONCRPC_AUTH_OK = 0</c>. </para> </summary>
    [Description( "There is no authentication problem or error." )]
    OncRpcAuthOkay = 0,

    /// <summary>
    /// (Immutable)
    /// The ONC/RPC server detected a bad credential (that is, the seal was broken). <para>
    /// Renamed from <c>ONCRPC_AUTH_BADCRED = 1</c>. </para>
    /// </summary>
    [Description( "The ONC/RPC server detected a bad credential (that is, the seal was broken)." )]
    OncRpcAuthBadCredential = 1,

    /// <summary>
    /// (Immutable)
    /// The ONC/RPC server has rejected the credential and forces the caller to begin a new session. <para>
    /// Renamed from <c>ONCRPC_AUTH_REJECTEDCRED = 2</c>. </para>
    /// </summary>
    [Description( "The ONC/RPC server has rejected the credential and forces the caller to begin a new session." )]
    OncRpcAuthRejectedCredential = 2,

    /// <summary>
    /// (Immutable)
    /// The ONC/RPC server detected a bad verifier (that is, the seal was broken). <para>
    /// Renamed from <c>ONCRPC_AUTH_BADVERF = 3</c>. </para>
    /// </summary>
    [Description( "The ONC/RPC server detected a bad verifier (that is, the seal was broken)." )]
    OncRpcAutoBadVerifier = 3,

    /// <summary>
    /// (Immutable)
    /// The ONC/RPC server detected an expired verifier (which can also happen if the verifier was replayed). <para>
    /// Renamed from <c>ONCRPC_AUTH_REJECTEDVERF = 4</c>. </para>
    /// </summary>
    [Description( "The ONC/RPC server detected an expired verifier (which can also happen if the verifier was replayed)" )]
    OncRpcAuthRejectedVerifier = 4,

    /// <summary>
    /// The ONC/RPC server rejected the authentication for security reasons. <para>
    /// Renamed from <c>ONCRPC_AUTH_TOOWEAK = 5</c>. </para>
    /// </summary>
    [Description( "The ONC/RPC server rejected the authentication for security reasons." )]
    OncRpcAuthTooWeak = 5,

    /// <summary>   The ONC/RPC client detected a bogus response verifier. <para>
    /// Renamed from <c>ONCRPC_AUTH_INVALIDRESP = 6</c>. </para> </summary>
    [Description( "The ONC/RPC client detected a bogus response verifier." )]
    OncRpcAuthInvalidResponse = 6,

    /// <summary>
    /// Authentication at the ONC/RPC client failed for an unknown reason. <para>
    /// Renamed from <c>ONCRPC_AUTH_FAILED = 7</c>. </para>
    /// </summary>
    [Description( "Authentication at the ONC/RPC client failed for an unknown reason." )]
    OncRpcAuthFailed = 7,

}
