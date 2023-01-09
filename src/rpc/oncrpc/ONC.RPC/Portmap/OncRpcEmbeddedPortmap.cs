using cc.isr.ONC.RPC.Codecs;
using cc.isr.ONC.RPC.Server;

#nullable enable

namespace cc.isr.ONC.RPC.Portmap;

/// <summary>
/// The class server <see cref="OncRpcEmbeddedPortmapService"/> provides an embeddable Portmap service, which is
/// automatically started in its own thread if the (operating) system does not already provide
/// the portmap service.
/// </summary>
/// <remarks>
/// If an embedded portmap service is started it will stop only after the last ONC/RPC program
/// has been deregistered. <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public class OncRpcEmbeddedPortmapService : IDisposable
{

    /// <summary>   (Immutable) the default timeout. </summary>
    public const int DefaultTimeout = 3000;

    #region " construction and cleanup "

    /// <summary>
    /// Constructs an embeddable portmap service of class
    /// server <see cref="OncRpcEmbeddedPortmapService"/> and starts the service if no
    /// other (external) portmap service is available.
    /// </summary>
    /// <remarks>
    /// This constructor is the same as server <see cref="OncRpcEmbeddedPortmapService"/> calling with a timeout of
    /// 3 seconds. The constructor starts the portmap service in its own thread and then returns.
    /// </remarks>
    /// <see cref="EmbeddedPortmapInUse()"/>
    public OncRpcEmbeddedPortmapService() : this( OncRpcEmbeddedPortmapService.DefaultTimeout )
    {
    }

    /// <summary>
    /// Constructs an embeddable portmap service of class
    /// server <see cref="OncRpcEmbeddedPortmapService"/> and starts the service if no
    /// other (external) portmap service is available.
    /// </summary>
    /// <remarks>
    /// The constructor starts the portmap service in its own thread and then returns.
    /// <see cref="EmbeddedPortmapInUse()"/>
    /// </remarks>
    /// <param name="checkTimeout"> timeout in milliseconds to wait before assuming that no
    ///                             portmap service is currently available. </param>
    public OncRpcEmbeddedPortmapService( int checkTimeout )
    {
        if ( !IsPortmapRunning( checkTimeout ) )
        {
            this._embeddedPortmapService = new EmbeddedPortmapService( this );
            this._embeddedPortmapThread = new EmbeddedPortmapServiceThread( this, this._embeddedPortmapService );
            this._embeddedPortmapService.ServiceThread = new Thread( new ThreadStart( this._embeddedPortmapThread.Run ) ) {
                Name = "Embedded Portmap Service Thread"
            };
            this._embeddedPortmapService.ServiceThread.Start();
        }
    }

    /// <summary> Gets or sets the sentinel to detect redundant calls. </summary>
    /// <value> The sentinel to detect redundant calls. </value>
    protected bool IsDisposed { get; private set; }

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

    /// <summary>
    /// Releases the unmanaged resources used by the isr.Std.Models.ThreadSafeToken{T} and
    /// optionally releases the managed resources.
    /// </summary>
    /// <remarks> David, 2020-09-22. </remarks>
    /// <param name="disposing"> True to release both managed and unmanaged resources; false to
    /// release only unmanaged resources. </param>
    protected virtual void Dispose( bool disposing )
    {
        if ( disposing )
        {
            // dispose managed state (managed objects)
        }

        // free unmanaged resources and override finalizer
        // i am assuming the slim lock depends on unmanaged resources.
        try
        {
            this.Shutdown();
        }
        catch ( Exception ex )
        {
            Console.WriteLine( $"Failed shutting down the port mapper: \n{ex} " );
        }

        OncRpcServerStubBase? portmap = this._embeddedPortmapService;
        this._embeddedPortmapService = null;
        try
        {
            portmap?.Close();
            portmap?.Dispose();
        }
        catch ( Exception ex )
        {
            Console.WriteLine( $"Failed closing the port mapper: \n{ex} " );
        }

        // set large fields to null
    }

    /// <summary>
    /// This destructor will Runs only if the Dispose method does not get called. It gives the base
    /// class the opportunity to finalize. Do not provide destructors in types derived from this
    /// class.
    /// </summary>
    /// <remarks> David, 2020-09-22. </remarks>
    ~OncRpcEmbeddedPortmapService()
    {
        if ( this.IsDisposed ) { return; }
        // Do not re-create Dispose clean-up code here.
        // Calling Dispose(false) is optimal for readability and maintainability.
        this.Dispose( false );
    }


    #endregion

    /// <summary>
    /// Indicates whether a portmap service (regardless whether it's supplied by the operating system
    /// or an embedded portmap service) is currently running.
    /// </summary>
    /// <remarks>
    /// This method will check for 3 seconds for an answer from a portmap before assuming that no one
    /// exists.
    /// </remarks>
    /// <returns>
    /// <see cref="T:true"/>, if a portmap service (either external or
    /// embedded) is running and can be contacted.
    /// </returns>
    public static bool IsPortmapRunning()
    {
        return IsPortmapRunning( OncRpcEmbeddedPortmapService.DefaultTimeout );
    }

    /// <summary>
    /// Indicates whether a portmap service (regardless whether it's supplied by the operating system
    /// or an embedded portmap service) is currently running.
    /// </summary>
    /// <param name="checkTimeout"> timeout in milliseconds to wait before assuming that no portmap
    ///                             service is currently available. </param>
    /// <returns>
    /// <see cref="T:true"/>, if a portmap service (either external or
    /// embedded) is running and can be contacted.
    /// </returns>
    public static bool IsPortmapRunning( int checkTimeout )
    {
        bool available = false;
        try
        {
            OncRpcPortmapClient portmap = new( IPAddress.Loopback );
            portmap.PortmapClient.Timeout = checkTimeout;
            portmap.Ping();
            available = true;
        }
        catch ( OncRpcException )
        {
            // We get noise from here if the port mapper is down
            // Logger.Writer.ConsoleWriteException( string.Empty, ex );
        }
        catch ( System.IO.IOException )
        {
            // We get noise from here if the port mapper is down
            // Logger.Writer.ConsoleWriteException( string.Empty, ex );
        }
        return available;
    }

    /// <summary>   Indicates whether the embedded portmap service is in use. </summary>
    /// <returns>
    /// <see cref="T:true"/>, if embedded portmap service is currently
    /// used.
    /// </returns>
    public virtual bool EmbeddedPortmapInUse()
    {
        return this._embeddedPortmapService?.ServiceThread is not null;
    }

    /// <summary>   Returns the thread object running the embedded portmap service. </summary>
    /// <returns>
    /// Thread object or <see langword="null"/> if no embedded portmap service has been started.
    /// </returns>
    public virtual Thread? GetEmbeddedPortmapServiceThread()
    {
        return this._embeddedPortmapService?.ServiceThread;
    }


    /// <summary>
    /// Portmap object acting as embedded portmap service or <see langword="null"/>
    /// if no embedded portmap service is necessary because the operating system already supplies one
    /// or another port mapper is already running.
    /// </summary>
    private EmbeddedPortmapService? _embeddedPortmapService;

    /// <summary>   Returns object implementing the embedded portmap service. </summary>
    /// <returns>
    /// Embedded portmap object or <see langword="null"/> if no embedded portmap service has been started.
    /// </returns>
    public virtual OncRpcPortMapService? GetEmbeddedPortmapService()
    {
        return this._embeddedPortmapService;
    }

    /// <summary>   Stop the embedded portmap service if it is running. </summary>
    /// <remarks>
    /// Normally you should not use this method except you need to force the embedded portmap service
    /// to terminate. Under normal conditions the thread responsible for the embedded portmap service
    /// will terminate automatically after the last ONC/RPC program has been deregistered. &lt;p&gt;
    /// This method just signals the portmap thread to stop processing ONC/RPC portmap calls and to
    /// terminate itself after it has cleaned up after itself.
    /// </remarks>
    public virtual void Shutdown()
    {
        OncRpcServerStubBase? portmap = this._embeddedPortmapService;
        portmap?.StopRpcProcessing();
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
        /// <returns>   <see cref="T:true"/> if deregistration succeeded. </returns>
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
            this._portmap.Run( this._portmap.GetTransports() );
            // This is not optimal but we need enough time after we remove the entry
            // from the portmap to respond okay to the client and I haven't figured out
            // any better way yet.
            Thread.Sleep( EmbeddedPortmapServiceThread.PostShutdownTimeout );
            this._portmap.Close();
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
}
