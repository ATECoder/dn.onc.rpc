using System.ComponentModel;

namespace cc.isr.ONC.RPC;

/// <summary>   A collection of protocol constants used by the ONC/RPC package. </summary>
/// <remarks>
/// Each constant defines one of the possible transport protocols, which can be used for
/// communication between ONC/RPC clients and servers. <para>
///  
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public enum OncRpcProtocol
{

    /// <summary>   An enum constant representing the not specified option. </summary>
    [Description( "Unspecified network communication protocol. " )]
    NotSpecified = 0,

    /// <summary>
    /// Use the UDP protocol of the IP (Internet protocol) suite as the network communication
    /// protocol for doing remote procedure calls. This is the same as the IPPROTO_UDP definition
    /// from the famous BSD socket API. <para>
    /// 
    /// Renamed from <c>ONCRPC_UDP (=17).</c> </para></summary>
    [Description( "Use the UDP protocol of the IP (Internet protocol) suite as the network communication protocol for remote procedure calls. " )]
    OncRpcUdp = 17,

    /// <summary>
    /// Use the TCP protocol of the IP (Internet protocol) suite as the network communication
    /// protocol for doing remote procedure calls. This is the same as the IPPROTO_TCP definition
    /// from the famous BSD socket API.<para>
    /// 
    /// Renamed from <c>ONCRPC_TCP (=6).</c> </para></summary>
    [Description( "Use the TCP protocol of the IP (Internet protocol) suite as the network communication protocol for remote procedure calls." )]
    OncRpcTcp = 6,

    /// <summary>
    /// Use the HTTP application protocol for tunneling ONC remote procedure calls. This is
    /// definitely not similar to any definition in the famous BSD socket API.<para>
    /// 
    /// Renamed from <c>ONCRPC_HTTP (=-42).</c> </para></summary>
    [Description( "Use the HTTP application protocol for tunneling ONC remote procedure calls." )]
    OncRpcHttp = -42,
}
