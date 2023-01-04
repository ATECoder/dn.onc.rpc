namespace cc.isr.ONC.RPC;

/// <summary>
/// A collection of constants used to identify the (overall) status of an ONC/RPC reply message.
/// </summary>
/// <remarks> <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public class OncRpcReplyStatus
{
    /// <summary>
    /// (Immutable)
    /// Reply status identifying that the corresponding message call was accepted.
    /// </summary>
    public const int OncRpcMessageAccepted = 0;

    /// <summary>
    /// (Immutable)
    /// Reply status identifying that the corresponding message call was denied.
    /// </summary>
    public const int OncRpcMessageDenied = 1;
}
