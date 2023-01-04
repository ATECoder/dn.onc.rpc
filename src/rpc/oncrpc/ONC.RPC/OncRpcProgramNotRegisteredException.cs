namespace cc.isr.ONC.RPC;

/// <summary>
/// The class server <see cref="OncRpcProgramNotRegisteredException"/> indicates that the requests
/// ONC/RPC program is not available at the specified host.
/// </summary>
/// <remarks> <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
[Serializable]
public class OncRpcProgramNotRegisteredException : OncRpcException
{

    /// <summary>
    /// Constructs an ONC/RPC program not registered exception with a detail code of 
    /// <see cref="OncRpcException.OncRpcProgramNotRegistered"/> and an appropriate clear-text detail message.
    /// </summary>
    public OncRpcProgramNotRegisteredException() : base( OncRpcException.OncRpcProgramNotRegistered )
    {
    }
}
