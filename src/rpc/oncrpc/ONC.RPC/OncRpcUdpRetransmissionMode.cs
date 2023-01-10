using System.ComponentModel;

using cc.isr.ONC.RPC.Client;

namespace cc.isr.ONC.RPC;

/// <summary>
/// A collection of constants used to identify the retransmission timeout schemes 
/// when using <see cref="OncRpcUdpClient">UDP/IP-based ONC/RPC clients</see>.  
/// </summary>
/// <remarks> <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public enum OncRpcUdpRetransmissionMode
{
    /// <summary>
    /// In exponential back-off retransmission mode, UDP/IP-based ONC/RPC clients first wait a given
    /// retransmission timeout period before sending the ONC/RPC call again. The retransmission
    /// timeout then is doubled on each try. <para>
    /// Renamed from <c>EXPONENTIAL = 0</c> </para> </summary>
    [Description( "The retransmission timeout is doubled between consecutive tries." )] OncRpcExponentialTimeout = 0,

    /// <summary>
    /// In fixed retransmission mode, UDP/IP-based ONC/RPC clients wait a given retransmission
    /// timeout period before send the ONC/RPC call again. The retransmission timeout is fixed between consecutive tries. <para>
    /// Renamed from <c>FIXED = 1</c> </para> </summary>
    [Description( "The retransmission timeout is fixed between consecutive tries." )] OncRpcFixedTimeout = 1,
}
