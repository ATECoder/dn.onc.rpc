namespace cc.isr.ONC.RPC;

/// <summary>
/// An abstract adapter class for <see cref="IOncRpcBroadcastListener">receiving</see> 
/// <see cref="OncRpcBroadcastEvent"/>. The methods in this class are empty. 
/// This class exists as convenience for creating listener objects.  
/// </summary>
/// <remarks>   
/// Remote Tea authors: Harald Albrecht, Jay Walters.
/// </remarks>
public abstract class OncRpcBroadcastAdapterBase : IOncRpcBroadcastListener
{
    /// <summary>   Invoked when a reply to an ONC/RPC broadcast call is received. </summary>
    /// <param name="evt">  The <see cref="OncRpcBroadcastEvent"/>. </param>
    public virtual void ReplyReceived( OncRpcBroadcastEvent evt )
    {
    }
}
