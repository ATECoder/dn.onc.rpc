namespace cc.isr.ONC.RPC;

/// <summary>
/// A collection of constants used to identify the retransmission schemes when using
/// <see cref="OncRpcUdpClient">UDP/IP-based ONC/RPC clients</see>.  
/// </summary>
/// <remarks> <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public class OncRpcUdpRetransmissionMode
{
    /// <summary>
    /// (Immutable)
    /// In exponential back-off retransmission mode, UDP/IP-based ONC/RPC clients first wait a given
    /// retransmission timeout period before sending the ONC/RPC call again. The retransmission
    /// timeout then is doubled on each try. <para>
    /// Renamed from <c>EXPONENTIAL = 0</c> </para> </summary>
    public const int OncRpcExponentialTimeout = 0;

    /// <summary>
    /// (Immutable)
    /// In fixed retransmission mode, UDP/IP-based ONC/RPC clients wait a given retransmission
    /// timeout period before send the ONC/RPC call again. The retransmission timeout is not changed
    /// between consecutive tries but is fixed instead. <para>
    /// Renamed from <c>FIXED = 1</c> </para> </summary>
    public const int OncRpcFixedTimeout = 1;
}
