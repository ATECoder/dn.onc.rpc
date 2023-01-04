namespace cc.isr.ONC.RPC;

/// <summary>
/// The abstract <see cref="OncRpcClientStubBase"/> class is the base class upon which to build ONC/RPC
/// program-specific clients.
/// </summary>
/// <remarks>
/// This class is typically only used by <c>rpcgen</c>-generated clients, which provide a particular
/// set of remote procedures as defined in a x-file. <para>
/// When you do not need the client proxy object any longer, you should return the resources
/// it occupies to the system. Use the <see cref="Close()"/> method for this. </para>
/// <code>
/// client.Close();
/// client = null; // Hint to the garbage collector.
/// </code> 
/// Remote Tea authors: Harald Albrecht, Jay Walters.
/// </remarks>
public abstract class OncRpcClientStubBase
{
    /// <summary>
    /// Constructs a new <see cref="OncRpcClientStubBase"/> for communication with a remote ONC/RPC server.
    /// </summary>
    /// <param name="host">     Host address where the desired ONC/RPC server resides. </param>
    /// <param name="program">  Program number of the desired ONC/RPC server. </param>
    /// <param name="version">  Version number of the desired ONC/RPC server. </param>
    /// <param name="port">     The port. </param>
    /// <param name="protocol"> <see cref="OncRpcProtocols">Protocol</see>
    ///                         to be used for ONC/RPC calls. This
    ///                         information is necessary, so port lookups
    ///                         through the portmapper can be done. </param>
    public OncRpcClientStubBase( IPAddress host, int program, int version, int port, int protocol )
    {
        this.Client = OncRpcClientBase.NewOncRpcClient( host, program, version, port, protocol );
    }

    /// <summary>
    /// Constructs a new <see cref="OncRpcClientStubBase"/> which uses the given client proxy object for
    /// communication with a remote ONC/RPC server.
    /// </summary>
    /// <param name="client">   ONC/RPC client proxy object implementing a particular IP protocol. </param>
    ///
    /// <exception cref="OncRpcException">          Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="System.IO.IOException">    Thrown when an I/O error condition occurs. </exception>
    public OncRpcClientStubBase( OncRpcClientBase client )
    {
        this.Client = client;
    }

    /// <summary>
    /// Close the connection to an ONC/RPC server and free all network-related resources.
    /// </summary>
    ///
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    public virtual void Close()
    {
        if ( this.Client != null )
            try
            {
                this.Client.Close();
            }
            finally
            {
                this.Client = null;
            }
    }

    /// <summary>
    /// Gets or sets or set (private) the ONC/RPC client proxy for communicating with a remote
    /// ONC/RPC server. using a particular IP protocol.
    /// </summary>
    /// <value> ONC/RPC client proxy. </value>
    public OncRpcClientBase Client { get; private set; }
}
