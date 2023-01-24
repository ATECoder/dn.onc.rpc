using cc.isr.ONC.RPC.Logging;
namespace cc.isr.ONC.RPC.Server;

/// <summary>
/// The abstract server <see cref="OncRpcServerStubBase"/> class is the base class upon which to build ONC/RPC-program
/// specific servers.
/// </summary>
/// <remarks>
/// This class is typically only used by <c>rpcgen</c> generated servers, which
/// provide a particular set of remote procedures as defined in an x-file.  <para>
/// 
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public abstract class OncRpcServerStubBase : IDisposable
{

    #region " construction and cleanup "

    public OncRpcServerStubBase()
    {
        this._characterEncoding = Encoding.Default;
        this._transports = Array.Empty<OncRpcTransportBase>();
        this._registeredPrograms = Array.Empty<OncRpcProgramInfo>();

    }

    /// <summary>   Close all transports listed in a set of server transports. </summary>
    /// <remarks>
    /// Only by calling this method processing of remote procedure calls by individual transports can
    /// be stopped. This is because every server transport is handled by its own thread.
    /// </remarks>
    public virtual void Close()
    {
        if ( this._transports is not null )
            foreach ( var transport in this._transports )
                transport.Close();
    }

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
        catch ( Exception ex ) { Logger.Writer.LogMemberError("Exception disposing", ex ); }
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
        this.StopRpcProcessing();

        // await for the port map service to stop running
        DateTime endTime = DateTime.Now.AddMilliseconds( 1000 );
        while ( this.Running && endTime < DateTime.Now )
        {
            Thread.Sleep( 100 );
        }
        if ( !this.Running )
            this.Close();
        else
            throw new InvalidOperationException( "Server still running after stopping RPC Processing." );

        // set large fields to null
    }

    /// <summary>   Finalizer. </summary>
    ~OncRpcServerStubBase()
    {
        if ( this.IsDisposed ) { return; }
        this.Dispose( false );
    }

    #endregion

    #endregion

    #region " members "

    private Encoding _characterEncoding;
    /// <summary>
    /// Gets or sets the encoding to use when serializing strings. If (<see langword="null"/>), the system's
    /// default encoding is to be used.
    /// </summary>
    /// <value> The character encoding. </value>
    public virtual Encoding CharacterEncoding
    {
        get => this._characterEncoding;
        set {
            this._characterEncoding = value;
            foreach ( var transport in this._transports )
                transport.CharacterEncoding = value;
        }
    }

    /// <summary>   Gets or sets a value indicating whether the port mapper server is running. </summary>
    /// <value> True if running, false if not. </value>
    public bool Running { get; protected set; }

    #endregion

    #region " transports "

    private OncRpcTransportBase[] _transports;
    /// <summary>
    /// Sets the array containing ONC/RPC server transport objects which describe what transports an
    /// ONC/RPC server offers for handling ONC/RPC calls.
    /// </summary>
    /// <param name="transports">   Array of server transport objects to register, which will later
    ///                             handle incoming remote procedure call requests. </param>
    public virtual void SetTransports( OncRpcTransportBase[] transports )
    {
        this._transports = transports ?? Array.Empty<OncRpcTransportBase>();
    }
    /// <summary>
    /// gets the array containing ONC/RPC server transport objects which describe what transports an ONC/RPC
    /// server offers for handling ONC/RPC calls.
    /// </summary>
    public virtual OncRpcTransportBase[] GetTransports()
    {
        return this._transports;
    }

    /// <summary>
    /// Array containing program and version numbers tuples this server is willing to handle.
    /// </summary>
    private OncRpcProgramInfo[] _registeredPrograms;

    /// <summary>
    /// Sets the array containing program and version numbers tuples this server is willing to handle.
    /// </summary>
    /// <param name="registeredPrograms">   Array containing program and version numbers tuples this
    ///                                     server is willing to handle. </param>
    public void SetRegisteredPrograms( OncRpcProgramInfo[] registeredPrograms )
    {
        this._registeredPrograms = registeredPrograms ?? Array.Empty<OncRpcProgramInfo>();
    }

    /// <summary>
    /// Returns the array containing program and version numbers tuples this server is willing to
    /// handle.
    /// </summary>
    /// <returns>   The array containing program and version numbers tuples this server is willing to
    /// handle. </returns>
    public OncRpcProgramInfo[] GetRegisteredPrograms()
    {
        return this._registeredPrograms;
    }

    /// <summary>   Register a set of server transports with the local port mapper. </summary>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="transports">   Array of server transport objects to register, which will later
    ///                             handle incoming remote procedure call requests. </param>
    public virtual void Register( OncRpcTransportBase[] transports )
    {
        int size = transports.Length;
        for ( int idx = 0; idx < size; ++idx )
            transports[idx].Register();
    }

    /// <summary>   Unregister a set of server transports from the local portmapper. </summary>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="transports">   Array of server transport objects to unregister. </param>
    public virtual void Unregister( OncRpcTransportBase[] transports )
    {
        foreach ( var transport in this._transports )
            transport.Unregister();
    }

    #endregion

    #region " run and stop "

    /// <summary>
    /// Gets or sets (<see langword="private"/>) the notification flag for signaling the server to stop processing
    /// incoming remote procedure calls and to shut down.
    /// </summary>
    /// <value> The shutdown signal. </value>
    internal object ShutdownSignal { get; private set; } = new();

    /// <summary>
    /// All inclusive convenience method: register server transports with port mapper, then Runs the
    /// call dispatcher until the server is signaled to shut down, and finally deregister the
    /// transports.
    /// </summary>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    public virtual void Run()
    {

        // Ignore all problems during unregistration.
        try
        {
            this.Unregister( this._transports );
        }
        catch ( OncRpcException )
        {
        }
        this.Register( this._transports );
        this.Run( this._transports, true );
        try
        {
            this.Unregister( this._transports );
        }
        catch ( OncRpcException )
        {
        }
    }

    /// <summary>
    /// Process incoming remote procedure call requests from all specified transports.
    /// </summary>
    /// <remarks>
    /// To end processing and to shut the server down signal the <see cref="ShutdownSignal"/> object.
    /// Note that the thread on which <see cref="Run()"/> is called will ignore any interruptions and
    /// will silently swallow them.
    /// </remarks>
    /// <param name="transports">           Array of server transport objects for which processing of
    ///                                     remote procedure call requests should be done. </param>
    /// <param name="closeUponShutdown">    True to close upon shutdown. </param>
    public virtual void Run( OncRpcTransportBase[] transports, bool closeUponShutdown )
    {

        this.Running = true;

        // Create the cancellation source.
        CancellationTokenSource cts = new();

        try
        {

            foreach ( var transport in transports )
                transport.Listen( cts );

            // Loop and wait for the shutdown flag to become signaled. If the
            // server's main thread gets interrupted it will not shut itself
            // down. It can only be stopped by signaling the shutdownSignal
            // object.
            for (; ; )
                lock ( this.ShutdownSignal )
                    try
                    {
                        _ = Monitor.Wait( this.ShutdownSignal );
                        break;
                    }
                    catch ( Exception )
                    {
                    }
        }
        catch ( Exception )
        {
            throw;
        }
        finally
        {

            foreach ( var transport in transports )
                transport.Unlisten( cts );

            if ( closeUponShutdown )
            {
                try
                {
                    this.Close();
                }
                catch ( Exception )
                {
                }
            }
            this.Running = false;
        }

    }

    /// <summary>
    /// Notify the RPC server to stop processing of remote procedure call requests as soon as
    /// possible.
    /// </summary>
    /// <remarks>
    /// Note that each transport has its own thread, so processing will not stop before the
    /// transports have been closed by calling the <see cref="Close()"/> method of the server.
    /// </remarks>
    public virtual void StopRpcProcessing()
    {
        if ( this.ShutdownSignal is not null )
            lock ( this.ShutdownSignal )
                Monitor.Pulse( this.ShutdownSignal );
    }

    #endregion

}
