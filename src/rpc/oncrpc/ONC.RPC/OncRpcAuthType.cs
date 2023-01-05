namespace cc.isr.ONC.RPC;

/// <summary>
/// A collection of constants used to identify the authentication schemes available for ONC/RPC.
/// </summary>
/// <remarks>
/// Please note that currently only <see cref="OncRpcAuthType.OncRpcAuthTypeNone"/> is supported by
/// this package.  <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public class OncRpcAuthType
{
    /// <summary>   (Immutable) No authentication scheme used for this remote procedure call. <para>
    /// Renamed from <c>ONCRPC_AUTH_NONE = 0</c>. </para> </summary>
    public const int OncRpcAuthTypeNone = 0;

    /// <summary>
    /// (Immutable) The so-called "Unix" authentication scheme is not supported. This one only sends
    /// the users id as well as her/his group identifiers, so this is simply far too weak to use in
    /// typical situations where authentication is requested. <para>
    /// Renamed from <c>ONCRPC_AUTH_UNIX = 1</c>. </para> </summary>
    public const int OncRpcAuthTypeUnix = 1;

    /// <summary>   (Immutable) The so-called "short hand Unix style" is not supported.  <para>
    /// Renamed from <c>ONCRPC_AUTH_SHORT = 2</c>. </para> </summary>
    public const int OncRpcAuthTypeShortHandUnix = 2;

    /// <summary>
    /// (Immutable)
    /// The Data Encryption Standard (DES) authentication scheme (using encrypted time stamps) is not
    /// supported -- and besides, it's not a silver bullet either. <para>
    /// Renamed from <c>ONCRPC_AUTH_DES = 3</c>. </para>
    /// </summary>
    public const int OncRpcAuthTypeDes = 3;
}
