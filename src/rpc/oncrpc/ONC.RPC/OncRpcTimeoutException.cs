namespace cc.isr.ONC.RPC;

/// <summary>
/// The class <see cref="OncRpcTimeoutException"/> indicates a timed out call exception.
/// </summary>
/// <remarks> <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
[Serializable]
public class OncRpcTimeoutException : OncRpcException
{

    /// <summary>
    /// Initializes an <see cref="OncRpcTimeoutException"/> with a detail of
    /// <see cref="OncRpcException.OncRpcProcedureCallTimedOut"/>.
    /// </summary>
    public OncRpcTimeoutException() : base( OncRpcException.OncRpcProcedureCallTimedOut )
    {
    }
}
