namespace cc.isr.ONC.RPC;

/// <summary>
/// The listener class for <see cref="IOncRpcBroadcastListener">receiving</see>
/// <see cref="OncRpcBroadcastEvent">ONC/RPC broadcast reply events</see>.
/// </summary>
/// <remarks> <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public interface IOncRpcBroadcastListener
{
    /// <summary>   Invoked when a reply to an ONC/RPC broadcast call is received. </summary>
    /// <remarks>
    /// Please note that you should not spend too much time when handling
    /// broadcast events, otherwise you'll probably miss some of the incoming replies. Because most
    /// operating systems will not buffer large amount of incoming UDP/IP datagrams for a given
    /// socket, you will experience packet drops when you stay too long in the processing stage.
    /// </remarks>
    /// <param name="evt">  The <see cref="OncRpcBroadcastEvent"/> event. </param>
    void ReplyReceived( OncRpcBroadcastEvent evt );
}
