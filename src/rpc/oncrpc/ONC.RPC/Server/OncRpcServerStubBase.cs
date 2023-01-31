using System.Diagnostics;

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
public abstract partial class OncRpcServerStubBase : ICloseable
{

    #region " construction and cleanup "

    public OncRpcServerStubBase()
    {
        this._characterEncoding = Encoding.Default;
        this._transports = Array.Empty<OncRpcTransportBase>();
        this._registeredPrograms = Array.Empty<OncRpcProgramInfo>();

    }

    /// <summary>   Terminates listening if active and closes the  all transports listed in a set of server transports. </summary>
    /// <remarks>
    /// Only by calling this method processing of remote procedure calls by individual transports can
    /// be stopped. This is because every server transport is handled by its own task.
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
    /// within a try...finaly block. <para>
    ///
    /// Because this class is implementing <see cref="IDisposable"/> and is not sealed, then it
    /// should include the call to <see cref="GC.SuppressFinalize(object)"/> even if it does not
    /// include a user-defined finalizer. This is necessary to ensure proper semantics for derived
    /// types that add a user-defined finalizer but only override the protected <see cref="Dispose(bool)"/>
    /// method. </para> <para>
    /// 
    /// To this end, call <see cref="GC.SuppressFinalize(object)"/>, where <see langword="Object"/> = <see langword="this"/> in the <see langword="Finally"/> segment of
    /// the <see langword="try"/>...<see langword="catch"/> clause. </para><para>
    ///
    /// If releasing unmanaged code or freeing large objects then override <see cref="Object.Finalize()"/>. </para>
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
            // this is included because this class is not sealed.

            GC.SuppressFinalize( this );

            // mark things as disposed.

            this.IsDisposed = true;
        }
    }

    /// <summary>   Gets or sets a value indicating whether this object is disposed. </summary>
    /// <value> True if this object is disposed, false if not. </value>
    public bool IsDisposed { get; private set; }

    /// <summary>   Gets or sets the shutdown timeout. </summary>
    /// <value> The shutdown timeout. </value>
    public static int ShutdownTimeout { get; set; } = 1000;

    /// <summary>
    /// Terminates listening if active and closes the  all transports listed in a set of server
    /// transports. Performs application-defined tasks associated with freeing, releasing, or
    /// resetting unmanaged resources.
    /// </summary>
    /// <remarks>
    /// Allows a timeout of <see cref="OncRpcServerStubBase.ShutdownTimeout"/> milliseconds for
    /// the server to stop listening before raising an exception to that effect.
    /// </remarks>
    /// <exception cref="InvalidOperationException">    Thrown when the requested operation is
    ///                                                 invalid. </exception>
    /// <exception cref="AggregateException">           Thrown when an Aggregate error condition
    ///                                                 occurs. </exception>
    /// <param name="disposing">    True to release both managed and unmanaged resources; false to
    ///                             release only unmanaged resources. </param>
    protected virtual void Dispose( bool disposing )
    {
        List<Exception> exceptions = new();
        if ( disposing )
        {
            // dispose managed state (managed objects)

            try
            {
                this.Shutdown( OncRpcServerStubBase.ShutdownTimeout, 5 );
            }
            catch ( Exception ex )
            { exceptions.Add( ex ); }
        }

        if ( exceptions.Any() )
        {
            AggregateException aggregateException = new( exceptions );
            throw aggregateException;
        }

        // free unmanaged resources and override finalizer

        // set large fields to null
    }

    #endregion

    #endregion

    #region " thread exception handler "

    /// <summary>
    /// Event queue for all listeners interested in ThreadExceptionOccurred events.
    /// </summary>
    public event ThreadExceptionEventHandler? ThreadExceptionOccurred;

    /// <summary>   Executes the <see cref="ThreadExceptionOccurred"/> event. </summary>
    /// <param name="e">    Event information to send to registered event handlers. </param>
    protected virtual void OnThreadException( ThreadExceptionEventArgs e )
    {
        var handler = this.ThreadExceptionOccurred;
        handler?.Invoke( this, e );
    }

    /// <summary>   Executes the <see cref="ThreadExceptionOccurred"/> event. </summary>
    /// <param name="sender">   Source of the event. </param>
    /// <param name="e">        Event information to send to registered event handlers. </param>
    protected virtual void OnThreadException( object sender, ThreadExceptionEventArgs e )
    {
        if ( sender is not null ) { this.OnThreadException( e ); };
    }

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
            if ( this.SetProperty( this.CharacterEncoding, value, () => this._characterEncoding = value ) )
                foreach ( var transport in this._transports )
                    transport.CharacterEncoding = value;
        }
    }

    private bool _running;
    /// <summary>   Gets or sets a value indicating whether the server is running. </summary>
    /// <value> True if running, false if not. </value>
    public virtual bool Running
    {
        get => this._running;
        protected set => _ = this.SetProperty( this.Running, value, () => this._running = value );
    }

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

        // register the transport exception handling.
        foreach ( OncRpcTransportBase transport in this._transports )
            transport.ThreadExceptionOccurred += this.OnThreadException;
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
        foreach ( OncRpcTransportBase transport in transports )
            transport.Register();
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

    /// <summary>   Checks if server started. </summary>
    /// <param name="timeout">      The timeout in milliseconds. </param>
    /// <param name="loopDelay">    The loop delay time in milliseconds. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    public bool ServerStarted( int timeout = 200, int loopDelay = 5 )
    {
        DateTime endTime = DateTime.Now.AddMilliseconds( timeout );
        while ( endTime > DateTime.Now || !this.Running )
        {
            Task.Delay( loopDelay ).Wait();
        }
        return this.Running;
    }

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
    /// Note that the task on which <see cref="Run()"/> is called will ignore any interruptions and
    /// will silently swallow them. <para>
    /// 
    /// <see href="https://www.albahari.com/threading/part4.aspx"/>
    /// </para>
    /// </remarks>
    /// <param name="transports">           Array of server transport objects for which processing of
    ///                                     remote procedure call requests should be done. </param>
    /// <param name="closeTransportsUponShutdown">    True to close transports upon stopping the server. </param>
    public virtual void Run( OncRpcTransportBase[] transports, bool closeTransportsUponShutdown )
    {

        this.Running = true;

        // Create the cancellation source.
        CancellationTokenSource cts = new();

        try
        {

            foreach ( var transport in transports )
                _ = transport.ListenAsync( cts );

            // Loop and wait for the shutdown flag to become signaled. If the
            // server's main task gets interrupted it will not shut itself
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

            if ( closeTransportsUponShutdown )
            {
                try
                {
                    this.CloseTransports();
                }
                catch ( Exception )
                {
                }
            }
            this.Running = false;
        }

    }

    /// <summary>   Starts the server using the all inclusive <see cref="Run()"/> asynchronously. </summary>
    /// <returns>   A Task. </returns>
    public virtual async Task RunAsync()
    {
        await Task.Factory.StartNew( () => { this.Run(); } )
                    .ContinueWith( failedTask => this.OnThreadException( new ThreadExceptionEventArgs( failedTask.Exception ) ),
                                                                                TaskContinuationOptions.OnlyOnFaulted );
    }

    /// <summary>   Starts the server using the existing transports synchronously. </summary>
    /// <param name="closeTransportsUponShutdown">  (Optional) True to close transports upon stopping
    ///                                             the server. </param>
    public virtual void Start( bool closeTransportsUponShutdown = true )
    {
        this.Run( this._transports, closeTransportsUponShutdown );
    }

    /// <summary>   Starts the server using the existing transports asynchronously. </summary>
    /// <param name="closeTransportsUponShutdown">  (Optional) True to close transports upon stopping
    ///                                             the server. </param>
    /// <returns>   A Task. </returns>
    public virtual async Task StartAsync( bool closeTransportsUponShutdown = true )
    {
        await Task.Factory.StartNew( () => { this.Start( closeTransportsUponShutdown ); } )
                    .ContinueWith( failedTask => this.OnThreadException( new ThreadExceptionEventArgs( failedTask.Exception ) ),
                                                                                TaskContinuationOptions.OnlyOnFaulted );
    }

    /// <summary>   Closes the transports. </summary>
    /// <remarks>   2023-01-30. </remarks>
    private void CloseTransports()
    {
        var transports = this._transports;
        if ( transports is not null )
            foreach ( var transport in this._transports )
            {
                transport.ThreadExceptionOccurred -= this.OnThreadException;
                transport.Close();
            }
    }

    /// <summary>
    /// Notify the RPC server to stop processing of remote procedure call requests as soon as
    /// possible.
    /// </summary>
    /// <remarks>
    /// Note that each transport has its own task, so processing will not stop before the
    /// transports have been closed by calling the <see cref="Close()"/> method of the server.
    /// </remarks>
    public virtual void StopRpcProcessing()
    {
        if ( this.ShutdownSignal is not null )
            lock ( this.ShutdownSignal )
                Monitor.PulseAll( this.ShutdownSignal );
    }

    /// <summary>   Shuts down this object and frees any resources it is using. </summary>
    /// <remarks> Unit test results shows that it took 4ms to shut down the server. </remarks>
    /// <exception cref="InvalidOperationException">    Thrown when the requested operation is
    ///                                                 invalid. </exception>
    /// <exception cref="AggregateException">           Thrown when an Aggregate error condition
    ///                                                 occurs. </exception>
    /// <param name="timeout">      (Optional) The timeout in milliseconds. </param>
    /// <param name="loopDelay">    (Optional) The loop delay in milliseconds. </param>
    public void Shutdown( int timeout = 100, int loopDelay = 5 )
    {
        List<Exception> exceptions = new();

        try
        {
            // if this works, this should also lead to closing.
            if ( this.Running )
                this.StopRpcProcessing();

            // await for the port map service to stop running
            DateTime endTime = DateTime.Now.AddMilliseconds( timeout );
            while ( this.Running && endTime > DateTime.Now )
            {
                Task.Delay( loopDelay ).Wait();
            }
            if ( this.Running )
                throw new InvalidOperationException( "Server still running after sending the stop signal." );
        }
        catch ( Exception ex )
        { exceptions.Add( ex ); }
        finally
        {
        }

        try
        {
            this.CloseTransports();
        }
        catch ( Exception ex )
        {
            { exceptions.Add( ex ); }
        }
        finally
        {
            this._transports = Array.Empty<OncRpcTransportBase>();
        }

        if ( exceptions.Any() )
        {
            AggregateException aggregateException = new( exceptions );
            throw aggregateException;
        }

    }

    /// <summary>   Shutdown asynchronous. </summary>
    /// <param name="timeout">      (Optional) The timeout in milliseconds. </param>
    /// <param name="loopDelay">    (Optional) The loop delay in milliseconds. </param>
    /// <returns>   A Task. </returns>
    public virtual async Task ShutdownAsync( int timeout = 1000, int loopDelay = 25 )
    {
        await Task.Factory.StartNew( () => { this.Shutdown( timeout, loopDelay ); } )
                .ContinueWith( failedTask => this.OnThreadException( new ThreadExceptionEventArgs( failedTask.Exception ) ), TaskContinuationOptions.OnlyOnFaulted );
    }



    #endregion

}
