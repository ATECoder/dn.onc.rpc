namespace cc.isr.ONC.RPC;

/// <summary>   A collection of protocol constants used by the ONC/RPC package. </summary>
/// <remarks>
/// Each constant defines one of the possible transport protocols, which can be used for
/// communication between ONC/RPC clients and servers. <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public class OncRpcProtocols
{
    /// <summary>
    /// (Immutable)
    /// Use the UDP protocol of the IP (Internet protocol) suite as the network communication
    /// protocol for doing remote procedure calls. This is the same as the IPPROTO_UDP definition
    /// from the famous BSD socket API. <para>
    /// Renamed from ONCRPC_UDP (=17). </para></summary>
    public const int OncRpcUdp = 17;

    /// <summary>
    /// (Immutable)
    /// Use the TCP protocol of the IP (Internet protocol) suite as the network communication
    /// protocol for doing remote procedure calls. This is the same as the IPPROTO_TCP definition
    /// from the famous BSD socket API.<para>
    /// Renamed from ONCRPC_TCP (=6). </para></summary>
    public const int OncRpcTcp = 6;

    /// <summary>
    /// (Immutable)
    /// Use the HTTP application protocol for tunneling ONC remote procedure calls. This is
    /// definitely not similar to any definition in the famous BSD socket API.<para>
    /// Renamed from ONCRPC_HTTP (=-42). </para></summary>
    public const int OncRpcHttp = -42;
}
