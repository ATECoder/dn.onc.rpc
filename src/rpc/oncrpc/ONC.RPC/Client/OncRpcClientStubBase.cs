using cc.isr.ONC.RPC.Logging;
namespace cc.isr.ONC.RPC.Client;

/// <summary>
/// The abstract <see cref="OncRpcClientStubBase"/> class is the base class upon which to build ONC/RPC
/// program-specific clients.
/// </summary>
/// <remarks>
/// This class is typically only used by <c>rpcgen</c>-generated clients, which provide a particular
/// set of remote procedures as defined in a x-file. <para>
///  
/// When you do not need the client proxy object any longer, you should return the resources
/// it occupies to the system. Use the <see cref="Close()"/> method for this. </para>
/// <code>
/// client.Close();
/// client = null; // Hint to the garbage collector.
/// </code> 
/// Remote Tea authors: Harald Albrecht, Jay Walters.
/// </remarks>
public abstract partial class OncRpcClientStubBase : ICloseable
{

    #region " construction and cleanup "

    /// <summary>
    /// Constructs a new <see cref="OncRpcClientStubBase"/> for communication with a remote ONC/RPC
    /// server.
    /// </summary>
    /// <param name="host">     Host address where the desired ONC/RPC server resides. </param>
    /// <param name="program">  Program number of the desired ONC/RPC server. </param>
    /// <param name="version">  Version number of the desired ONC/RPC server. </param>
    /// <param name="port">     The port. </param>
    /// <param name="protocol"> The protocol to be used for ONC/RPC calls. This information is
    ///                         necessary, so port lookups through the portmapper can be done. </param>
    /// <param name="timeout">  The transmit timeout for <see cref="OncRpcProtocol.OncRpcUdp"/>
    ///                         or the connection timeout for <see cref="OncRpcProtocol.OncRpcTcp"/>. </param>
    public OncRpcClientStubBase( IPAddress host, int program, int version, int port, OncRpcProtocol protocol, int timeout )
    {
        this.Client = OncRpcClientBase.NewOncRpcClient( host, program, version, port, protocol, timeout );
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
    /// <remarks>
    /// The general contract of <see cref="Close()"/> is that it closes and disposes of the 
    /// ONC/RPC client. A closed client cannot perform RPC calls and cannot be reopened. <para>
    /// 
    /// The <see cref="Close()"/> method of calls <see cref="Dispose(bool)"/> and is not 
    /// <see langword="virtual"/>.</para>
    /// </remarks>
    public void Close()
    {
        (( IDisposable ) this).Dispose();
    }

    #region " disposable implementation "

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
    /// resources.
    /// </summary>
    /// <remarks>
    /// Takes account of and updates <see cref="IsDisposed"/>. Encloses <see cref="Dispose(bool)"/>
    /// within a try...finaly block.
    /// </remarks>
    public void Dispose()
    {
        if ( this.IsDisposed ) { return; }
        try
        {
            // Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.

            this.Dispose( true );

        }
        catch { throw; }
        finally
        {
            // uncomment the following line if Finalize() is overridden above.

            GC.SuppressFinalize( this );

            // mark things as disposed.

            this.IsDisposed = true;
        }
    }

    /// <summary>   Gets or sets a value indicating whether this object is disposed. </summary>
    /// <value> True if this object is disposed, false if not. </value>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Releases unmanaged, large objects and (optionally) managed resources used by this class.
    /// </summary>
    /// <param name="disposing">    True to release large objects and managed and unmanaged resources;
    ///                             false to release only unmanaged resources and large objects. </param>
    protected virtual void Dispose( bool disposing )
    {
        if ( disposing )
        {
            // dispose managed state (managed objects)
            ICloseable? client = this.Client;
            if ( client is not null )
            {
                try
                {
                    this.Client?.Close();
                }
                catch ( Exception )
                {
                    throw;
                }
                finally
                {
                    this.Client = null;
                }
            }
        }

        // free unmanaged resources and override finalizer

        // set large fields to null
    }

    /// <summary>   Finalizer. </summary>
    ~OncRpcClientStubBase()
    {
        if ( this.IsDisposed ) { return; }
        this.Dispose( false );
    }

    #endregion

    #region " construction and cleanup "

    #endregion


    #endregion

    #region " members "

    /// <summary>
    /// Gets or sets or set (<see langword="private"/>) the ONC/RPC client proxy for communicating with a remote
    /// ONC/RPC server. using a particular IP protocol.
    /// </summary>
    /// <value> ONC/RPC client proxy. </value>
    public OncRpcClientBase? Client { get; private set; }

    #endregion
}
