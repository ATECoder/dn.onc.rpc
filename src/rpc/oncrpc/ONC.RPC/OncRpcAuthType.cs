using System.ComponentModel;

namespace cc.isr.ONC.RPC;

/// <summary>
/// A collection of constants used to identify the authentication schemes available for ONC/RPC.
/// </summary>
/// <remarks>
/// Please note that currently only <see cref="OncRpcAuthType.OncRpcAuthTypeNone"/> is supported by
/// this package.  <para>
/// 
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public enum OncRpcAuthType
{
    /// <summary>   No authentication scheme used for this remote procedure call. <para>
    /// 
    /// Renamed from <c>ONCRPC_AUTH_NONE = 0</c>. </para> </summary>
    [Description( "No authentication scheme used for this remote procedure call." )]
    OncRpcAuthTypeNone = 0,

    /// <summary>
    /// The client identifies itself using the users id as well as the user's group identifier. <para>
    /// 
    /// Renamed from <c>ONCRPC_AUTH_UNIX = 1</c>. </para> </summary>
    [Description( "The client identifies itself using the users id as well as the user's group identifier.." )]
    OncRpcAuthTypeUnix = 1,

    /// <summary>   The client identifies itself using the short-hand verifier received from the server
    ///             in response to the <see cref="OncRpcAuthType.OncRpcAuthTypeUnix"/> call.  <para>
    /// 
    /// Renamed from <c>ONCRPC_AUTH_SHORT = 2</c>. </para> </summary>
    [Description( "The client identifies itself using the short-hand verifier received from the server in response to the Unix authentication call" )]
    OncRpcAuthTypeShortHandUnix = 2,

    /// <summary>
    /// The Data Encryption Standard (DES) authentication scheme (using encrypted time stamps). <para>
    /// 
    /// Renamed from <c>ONCRPC_AUTH_DES = 3</c>. </para>
    /// </summary>
    [Description( "The Data Encryption Standard (DES) authentication scheme (using encrypted time stamps)." )]
    OncRpcAuthTypeDes = 3,
}
