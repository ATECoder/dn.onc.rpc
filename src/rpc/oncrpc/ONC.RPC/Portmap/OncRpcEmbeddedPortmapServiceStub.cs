using System.Diagnostics;

using cc.isr.ONC.RPC.Logging;
using cc.isr.ONC.RPC.Server;

namespace cc.isr.ONC.RPC.Portmap;

/// <summary>
/// The class server <see cref="OncRpcEmbeddedPortmapServiceStub"/> provides an embeddable Portmap service, which is
/// automatically started in its own thread if the (operating) system does not already provide
/// the portmap service.
/// </summary>
/// <remarks>
/// If an embedded portmap service is started it will stop only after the last ONC/RPC program
/// has been deregistered. <para>
/// 
/// This class need not be disposable as the service will automatically terminate after the last 
/// program deregisters. </para><para>
/// 
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public class OncRpcEmbeddedPortmapServiceStub : ICloseable
{

    #region " construction and cleanup "

    /// <summary>
    /// Constructs an embeddable portmap service of class server <see cref="OncRpcEmbeddedPortmapServiceStub"/>
    /// and starts the service if no other (external) portmap service is available.
    /// </summary>
    /// <remarks>
    /// The constructor starts the portmap service in its own thread and then returns.
    /// </remarks>
    /// <param name="ioTimeout">        (Optional) timeout in milliseconds to wait before assuming
    ///                                 that no portmap service is currently available [100]. Set to
    ///                                 zero to skip check of portmap already running in case this
    ///                                 was already done. </param>
    /// <param name="transmitTimeout">  (Optional) The transmit timeout; defaults to 25 ms. </param>
    public OncRpcEmbeddedPortmapServiceStub( int ioTimeout = 100, int transmitTimeout = 25 )
    {
        if ( ioTimeout <= 0 || !OncRpcPortmapClient.TryPingPortmapService( ioTimeout, transmitTimeout ) )
        {
            this._embeddedPortmapService = new OncRpcEmbeddedPortmapService();
            _ = this._embeddedPortmapService!.StartAsync();
        }
        else
        {
            this.UsingExternalPortmapService = true;
        }
    }

    /// <summary>
    /// Constructs an empty portmap service of class server <see cref="OncRpcEmbeddedPortmapServiceStub"/>
    /// and starts the service if no other (external) portmap service is available.
    /// </summary>
    /// <remarks>
    /// The constructor starts the portmap service in its own thread and then returns.
    /// </remarks>
    /// <param name="usingExternalPortmapService">  True if portmap service already running, false if
    ///                                             not. </param>
    public OncRpcEmbeddedPortmapServiceStub( bool usingExternalPortmapService )
    {
        this.UsingExternalPortmapService = usingExternalPortmapService;
    }

    /// <summary>   Stop the embedded portmap service synchronously if it is running. </summary>
    /// <remarks>
    /// Normally you should not use this method except you need to force the embedded portmap service
    /// to terminate. Under normal conditions the thread responsible for the embedded portmap service
    /// will terminate automatically after the last ONC/RPC program has been deregistered.
    /// This method just signals the portmap thread to stop processing ONC/RPC portmap calls and to
    /// terminate itself after it has cleaned up after itself.
    /// </remarks>
    public virtual void Shutdown()
    {
        OncRpcServerStubBase? oncRpcServerStub = this._embeddedPortmapService;
        oncRpcServerStub?.Shutdown();
    }

    /// <summary>   Shutdown asynchronously. </summary>
    public virtual async Task ShutdownAsync()
    {
        OncRpcServerStubBase? oncRpcServerStub = this._embeddedPortmapService;
        if ( oncRpcServerStub != null ) 
            await oncRpcServerStub.ShutdownAsync();
    }

    /// <summary>   Start the embedded port map service. </summary>
    /// <remarks> Unit tests show the following timing:
    /// checked 110ms, start 220ms, ping: 4ms. Total time is around 330ms
    /// with the new default timeouts for pinging the service. 
    /// </remarks>
    /// <exception cref="InvalidOperationException">    Thrown when the service failed to run
    ///                                                 or is not available. </exception>
    /// <returns>   An OncRpcEmbeddedPortmapService. </returns>
    public static OncRpcEmbeddedPortmapServiceStub StartEmbeddedPortmapService()
    {
        Logger.Writer.LogInformation( $"Checking for portmap service" );
        Stopwatch sw = Stopwatch.StartNew();
        bool alreadyRunning = OncRpcPortmapClient.TryPingPortmapService();
        int checkTime = ( int ) sw.ElapsedMilliseconds;
        if ( alreadyRunning )
            Logger.Writer.LogInformation( "A portmap service is already running." );
        else
            Logger.Writer.LogInformation( "No portmap service available." );

        Logger.Writer.LogInformation( "Creating embedded portmap instance" );
        sw = Stopwatch.StartNew();

        if ( alreadyRunning ) return new OncRpcEmbeddedPortmapServiceStub( true );

        OncRpcEmbeddedPortmapServiceStub epm = new( 0 );

        return !epm.EmbeddedPortmapServiceStarted()
            ? throw new InvalidOperationException( "Portmap service is not available or not in use." )
            : epm;

#if false
        if ( epm.EmbeddedPortmapServiceStarted() )
        {
            int startTime = ( int ) sw.ElapsedMilliseconds;
            Logger.Writer.LogInformation( "embedded service started; try pinging the port map service" );

            sw = Stopwatch.StartNew();
            bool pinged = OncRpcPortmapClient.TryPingPortmapService();
            if ( !pinged )
                throw new InvalidOperationException( "Portmap service is not running." );
            int pingTime = ( int ) sw.ElapsedMilliseconds;
            Logger.Writer.LogInformation( $"portmap service is {(pinged ? "running" : "idle")}; checked {checkTime}ms, start {startTime}ms, ping: {pingTime}ms " );
            return epm;
        }

        else
            throw new InvalidOperationException( "Portmap service is not available or not in use." );
#endif
    }

    /// <summary>
    /// Closes the connection to an ONC/RPC server and frees all network-related resources.
    /// </summary>
    /// <remarks>
    /// This implementation of close and dispose follows the implementation of the <see cref="System.Net.Sockets.TcpClient"/>
    /// at
    /// <see href="https://github.com/microsoft/referencesource/blob/master/System/net/System/Net/Sockets/TCPClient.cs"/>
    /// with the following modifications:
    /// <list type="bullet"> <item>
    /// <see cref="Close()"/> is not <see langword="virtual"/> </item><item>
    /// <see cref="Close()"/> calls <see cref="IDisposable.Dispose()"/> </item><item>
    /// Consequently, <see cref="Close()"/> need not be overridden. </item><item>
    /// <see cref="Close()"/> does not hide any exception that might be thrown by <see cref="IDisposable.Dispose()"/>
    /// </item></list>
    /// <list type="bullet"> <item>
    /// The <see cref="IDisposable.Dispose()"/> method skips if <see cref="ICloseable.IsDisposed"/>
    /// is <see langword="true"/>; </item><item>
    /// The <see cref="XdrEncodingStreamBase.Dispose(bool)"/> accumulates and throws an aggregate
    /// exception </item><item>
    /// The <see cref="IDisposable.Dispose()"/> method throws the aggregate exception from <see cref="XdrEncodingStreamBase.Dispose(bool)"/>
    /// . </item></list>
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
        catch ( Exception ex ) { Logger.Writer.LogMemberError( "Exception disposing", ex ); }
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

            ICloseable? portmapService = this._embeddedPortmapService;
            try
            {
                portmapService?.Close();
            }
            catch
            {
                throw;
            }
            finally
            {
                this._embeddedPortmapService = null;
            }

        }

        // free unmanaged resources and override finalizer

        // set large fields to null
    }

#endregion

#endregion

#region " members "

    /// <summary>
    /// Portmap object acting as embedded portmap service or (<see langword="null"/>)
    /// if no embedded portmap service is necessary because the operating system already supplies one
    /// or another port mapper is already running.
    /// </summary>
    private OncRpcEmbeddedPortmapService? _embeddedPortmapService;

    /// <summary>   Returns the embedded portmap service. </summary>
    /// <returns>
    /// Embedded portmap object or (<see langword="null"/>) if no embedded portmap service has been started.
    /// </returns>
    public virtual OncRpcPortMapService? EmbeddedPortmapService => this._embeddedPortmapService;

    /// <summary>
    /// Gets or sets a value indicating whether an external portmap service
    /// was already running when trying to establish this embedded portmap service, in which case we
    /// do not have a <see cref="EmbeddedPortmapService"/>
    /// </summary>
    /// <value> True if portmap service already running, false if not. </value>
    public bool UsingExternalPortmapService { get; set; } = false;

#endregion

#region " actions "

    /// <summary>
    /// Tries to ping the portmap service regardless whether it's supplied by the operating system or
    /// an embedded portmap service.
    /// </summary>
    /// <remarks>   Unit tests shows this to take 3 ms. </remarks>
    /// <param name="ioTimeout">        (Optional) The i/o timeout; defaults to 100 ms. </param>
    /// <param name="transmitTimeout">  (Optional) The transmit timeout; defaults to 25 ms. </param>
    /// <returns>
    /// <see langword="true"/>, if a portmap service (either external or embedded) is running and can
    /// be contacted.
    /// </returns>
    public static bool TryPingPortmapService1( int ioTimeout = 100, int transmitTimeout = 25 )
    {
        using OncRpcPortmapClient pmapClient = new( IPAddress.Loopback, OncRpcProtocol.OncRpcUdp, transmitTimeout );
        pmapClient.OncRpcClient!.IOTimeout = ioTimeout;
        return pmapClient.TryPingPortmapService();
    }

    /// <summary>   Determines if we can embedded portmap service started without waiting for timeout. </summary>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    private bool EmbeddedPortmapServiceStartedImmediate()
    {
        return this._embeddedPortmapService is not null && this._embeddedPortmapService.Running;
    }

    /// <summary>   Delays. </summary>
    /// <remarks>   2023-01-24. </remarks>
    /// <param name="delay">    The delay. </param>
    private async void Delay( int delay )
    {
        { await Task.Delay( delay ); }

    }
    /// <summary>   Checks if the embedded portmap service has started. </summary>
    /// <param name="timeout">  (Optional) The timeout; defaults to 200 ms. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    public virtual bool EmbeddedPortmapServiceStarted( int timeout = 200 )
    {
        DateTime endTime = DateTime.Now.AddMilliseconds( timeout );
        while ( endTime > DateTime.Now || !this.EmbeddedPortmapServiceStartedImmediate() )
        { this.Delay( 10 ); }
        return this.EmbeddedPortmapServiceStartedImmediate();
        // return this._embeddedPortmapService?.ServiceThread is not null;
    }

#endregion

#region " embedded portmap service and thread classes "

#if false
    /// <summary>   Returns the thread object running the embedded portmap service. </summary>
    /// <returns>
    /// Thread object or (<see langword="null"/>) if no embedded portmap service has been started.
    /// </returns>
    public virtual Thread? GetEmbeddedPortmapServiceThread()
    {
        return this._embeddedPortmapService?.ServiceThread;
    }

    /// <summary>   References thread object running the embedded portmap service. </summary>
    private readonly EmbeddedPortmapServiceThread? _embeddedPortmapThread;


    private class EmbeddedPortmapService : OncRpcPortMapService
    {
        /// <summary>   Creates a new instance of an embeddable portmap service. </summary>
        /// <param name="enclosing">    The enclosing. </param>
        public EmbeddedPortmapService( OncRpcEmbeddedPortmapService enclosing )
        {
            this._enclosing = enclosing;
        }

        /// <summary>   Thread running the embedded portmap service. </summary>
        public Thread? ServiceThread { get; set; }

        /// <summary>
        /// Deregister all port settings for a particular (program, version) for all transports (TCP, UDP,
        /// etc.).
        /// </summary>
        /// <remarks>
        /// This method basically falls back to the implementation provided by the <c>rpcgen</c>
        /// superclass, but checks whether there are other ONC/RPC programs registered. If not, it
        /// signals itself to shut down the portmap service.
        /// </remarks>
        /// <param name="serverIdentification"> the server identification, which includes the program and
        ///                                     version to deregister. The protocol and port fields are
        ///                                     not used. </param>
        /// <returns>   <see langword="true"/> if deregistration succeeded. </returns>
        internal override BooleanXdrCodec UnsetPort( OncRpcServerIdentifierCodec serverIdentification )
        {
            BooleanXdrCodec ok = base.UnsetPort( serverIdentification );
            if ( ok.Value )
            {

                // Check for registered programs other than OncRpcPortmapConstants.OncRpcPortmapProgramNumber.

                bool onlyPmap = true;
                int size = this.ServerIdentifierCodecs.Count;
                for ( int idx = 0; idx < size; ++idx )
                    if ( (( OncRpcServerIdentifierCodec ) this.ServerIdentifierCodecs[idx]!).Program != OncRpcPortmapConstants.OncRpcPortmapProgramNumber )
                    {
                        onlyPmap = false;
                        break;
                    }

                // If only portmap-related entries are left, then shut down this
                // portmap service.

                if ( onlyPmap && this.ServiceThread is not null )
                    this.StopRpcProcessing();
            }
            return ok;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "CodeQuality", "IDE0052:Remove unread private members", Justification = "<Pending>" )]
        private readonly OncRpcEmbeddedPortmapService _enclosing;
    }

    private class EmbeddedPortmapServiceThread
    {

        /// <summary>   (Immutable) the post shutdown timeout required, at this time, to provide enough time after 
        /// the removal of the entries from the portmap to respond okay to the client. </summary>
        public const int PostShutdownTimeout = 1000;

        /// <summary>
        /// Constructs a new embedded portmap service thread and associate it with the portmap object to
        /// be used as the service.
        /// </summary>
        /// <remarks> The service is not started yet. </remarks>
        /// <param name="enclosing">    The enclosing embedded portmap service. </param>
        /// <param name="portmap">      The embedded portmap service object this thread belongs to. </param>
        public EmbeddedPortmapServiceThread( OncRpcEmbeddedPortmapService enclosing, EmbeddedPortmapService portmap )
        {
            this._enclosing = enclosing;
            this._portmap = portmap;
        }

        /// <summary>
        /// Runs the embedded portmap service thread, starting dispatching of all portmap transports until
        /// we get the signal to shut down.
        /// </summary>
        public void Run()
        {
            this._portmap.Run( this._portmap.GetTransports(), true );
#if false
            // This is not optimal but we need enough time after we remove the entry
            // from the portmap to respond okay to the client and I haven't figured out
            // any better way yet.
            Thread.Sleep( EmbeddedPortmapServiceThread.PostShutdownTimeout );
            this._portmap.Close();
#endif
            this._portmap.ServiceThread = null;
        }

        /// <summary>   Attempts to run. </summary>
        /// <returns>   A Tuple( True if it succeeds, false if it fails; Exception if it fails). </returns>
        public (bool Success, Exception? Exception) TryRun()
        {
            try
            {
                this.Run();
            }
            catch ( Exception e )
            {
                return (false, e);
            }
            return (true, null);
        }

        /// <summary>   (Immutable) The embedded portmap service object this thread belongs to. </summary>
        /// <remarks>
        /// The service object implements the ONC/RPC dispatcher and the individual remote procedures for
        /// a port mapper).
        /// </remarks>
        private readonly EmbeddedPortmapService _portmap;

        /// <summary>   (Immutable) the enclosing. </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "CodeQuality", "IDE0052:Remove unread private members", Justification = "<Pending>" )]
        private readonly OncRpcEmbeddedPortmapService _enclosing;
    }
#endif

#endregion

}
