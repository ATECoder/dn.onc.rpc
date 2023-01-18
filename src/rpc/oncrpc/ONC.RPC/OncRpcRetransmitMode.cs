using System.ComponentModel;

using cc.isr.ONC.RPC.Client;

namespace cc.isr.ONC.RPC;

/// <summary>
/// A collection of constants used to identify the retransmit schemes 
/// when using <see cref="OncRpcUdpClient">UDP/IP-based ONC/RPC clients</see>.  
/// </summary>
/// <remarks> <para>
/// 
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public enum OncRpcRetransmitMode
{
    /// <summary>
    /// In exponential back-off retransmit mode, UDP/IP-based ONC/RPC clients first wait a given
    /// retransmit timeout period before sending the ONC/RPC call again. The retransmit
    /// timeout then is doubled on each try. <para>
    /// 
    /// Renamed from <c>EXPONENTIAL = 0</c> </para> </summary>
    [Description( "The retransmit timeout is doubled between consecutive tries." )]
    OncRpcExponentialTimeout = 0,

    /// <summary>
    /// In fixed retransmit mode, UDP/IP-based ONC/RPC clients wait a given retransmit
    /// timeout period before send the ONC/RPC call again. The retransmit timeout is unchanged between consecutive tries. <para>
    /// 
    /// Renamed from <c>FIXED = 1</c> </para> </summary>
    [Description( "The retransmit timeout is fixed between consecutive tries." )]
    OncRpcFixedTimeout = 1,
}
