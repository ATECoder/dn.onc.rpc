using System.ComponentModel;

namespace cc.isr.ONC.RPC;

/// <summary>
/// A collection of constants used to identify the (overall) status of an ONC/RPC reply message.
/// </summary>
/// <remarks> <para>
///  
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public enum OncRpcReplyStatus
{
    /// <summary>
    /// Reply status indicating that the corresponding message call was accepted.
    /// </summary>
    [Description( "Reply status indicating that the corresponding message call was accepted." )]
    OncRpcMessageAccepted = 0,

    /// <summary>
    /// Reply status indicating that the corresponding message call was denied.
    /// </summary>
    [Description( "Reply status indicating that the corresponding message call was denied." )]
    OncRpcMessageDenied = 1,
}
