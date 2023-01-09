namespace cc.isr.ONC.RPC.Client;

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
public abstract class OncRpcClientStubBase : IDisposable
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
    public OncRpcClientStubBase( IPAddress host, int program, int version, int port, OncRpcProtocols protocol )
    {
        this.Client = OncRpcClientBase.NewOncRpcClient( host, program, version, port, protocol );
    }

    /// <summary>
    /// Constructs a new <see cref="OncRpcClientStubBase"/> which uses the given client proxy object for
    /// communication with a remote ONC/RPC server.
    /// </summary>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="client">   ONC/RPC client proxy object implementing a particular IP protocol. </param>
    public OncRpcClientStubBase( OncRpcClientBase client )
    {
        this.Client = client;
    }

    /// <summary>
    /// Close the connection to an ONC/RPC server and free all network-related resources.
    /// </summary>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    public virtual void Close()
    {
        this.Client?.Close();
        this.Client = null;
    }

    /// <summary>
    /// Gets or sets or set (private) the ONC/RPC client proxy for communicating with a remote
    /// ONC/RPC server. using a particular IP protocol.
    /// </summary>
    /// <value> ONC/RPC client proxy. </value>
    public OncRpcClientBase Client { get; private set; }

    #region " disposable implementation "

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
    /// resources.
    /// </summary>
    /// <remarks> 
    /// Takes account of and updates <see cref="IsDisposed"/>.
    /// Encloses <see cref="Dispose(bool)"/> within a try...finaly block.
    /// </remarks>
    public void Dispose()
    {
        if ( this.IsDisposed ) { return; }
        try
        {
            // Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            this.Dispose( true );

            // uncomment the following line if Finalize() is overridden above.
            GC.SuppressFinalize( this );
        }
        finally
        {
            this.IsDisposed = true;
        }
    }

    /// <summary>   Gets or sets a value indicating whether this object is disposed. </summary>
    /// <value> True if this object is disposed, false if not. </value>
    protected bool IsDisposed { get; private set; }

    /// <summary>
    /// Releases the unmanaged resources used by the XdrDecodingStreamBase and optionally releases
    /// the managed resources.
    /// </summary>
    /// <param name="disposing">    True to release both managed and unmanaged resources; false to
    ///                             release only unmanaged resources. </param>
    protected virtual void Dispose( bool disposing )
    {
        if ( disposing )
        {
            // dispose managed state (managed objects)
        }

        // free unmanaged resources and override finalizer
        // I am assuming that the socket used in the derived classes include unmanaged resources.
        this.Close();

        // set large fields to null
    }

    /// <summary>   Finalizer. </summary>
    ~OncRpcClientStubBase()
    {
        if ( this.IsDisposed ) { return; }
        this.Dispose( false );
    }

    #endregion

}
