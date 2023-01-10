using cc.isr.ONC.RPC.Server;

#nullable disable

namespace cc.isr.ONC.RPC.MSTest.Udp;

/// <summary>
/// The abstract UDP server <see cref="OncRpcTcpServerBase"/> class is the base class upon which to build ONC/RPC 
/// program-specific UDP servers.
/// </summary>
public abstract class OncRpcUdpServerBase : OncRpcServerStubBase, IOncRpcDispatchable
{

    public OncRpcUdpServerBase() : this( 0 )
    {
    }

    public OncRpcUdpServerBase( int port ) : this( null, port )
    {
    }

    public OncRpcUdpServerBase( IPAddress bindAddr, int port )
    {
        OncRpcServerTransportRegistrationInfo[] info = new OncRpcServerTransportRegistrationInfo[] {
            new OncRpcServerTransportRegistrationInfo(RpcProgramConstants.ProgramNumber, 1),
            new OncRpcServerTransportRegistrationInfo(RpcProgramConstants.ProgramNumber, 2),
        };
        this.SetTransportRegistrationInfo( info );
        OncRpcServerTransportBase[] transports = new OncRpcServerTransportBase[] {
            new OncRpcUdpServerTransport(this, bindAddr, port, info, 32768)
        };
        this.SetTransports( transports );
    }


    /// <summary>   Dispatch (handle) an ONC/RPC request from a client. </summary>
    /// <remarks>
    /// This interface has some fairly deep semantics, so please read the description above for how
    /// to use it properly. For background information about fairly deep semantics, please also refer
    /// to <i>Gigzales</i>, <i>J</i>.: Semantics considered harmful. Addison-Reilly, 1992, ISBN 0-542-
    /// 10815-X. <para>
    /// See the introduction to this class for examples of how to use this interface properly.</para>
    /// </remarks>
    /// <param name="call">         <see cref="OncRpcCallInformation"/> about the call to handle, like the
    ///                             caller's Internet address, the ONC/RPC
    ///                             call header, etc. </param>
    /// <param name="program">      Program number requested by client. </param>
    /// <param name="version">      Version number requested. </param>
    /// <param name="procedure">    Procedure number requested. </param>
    public virtual void DispatchOncRpcCall( OncRpcCallInformation call, int program, int version, int procedure )
    {
    }
}
