using System.Diagnostics;

using cc.isr.ONC.RPC.Logging;
using cc.isr.ONC.RPC.Codecs;
using cc.isr.ONC.RPC.Server;


namespace cc.isr.ONC.RPC.Portmap;

/// <summary>
/// The class server <see cref="OncRpcEmbeddedPortmapService"/> provides an embeddable Portmap service, which is
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
public class OncRpcEmbeddedPortmapService
{

    #region " construction and cleanup "

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
    ///                             portmap service is currently available [3000]. </param>
    public OncRpcEmbeddedPortmapService( int checkTimeout = 3000 )
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

    /// <summary>   Stop the embedded portmap service if it is running. </summary>
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
        oncRpcServerStub?.StopRpcProcessing();
    }

    /// <summary>   Start the embedded port map service. </summary>
    /// <exception cref="InvalidOperationException">    Thrown when the service failed to run
    ///                                                 or is not available. </exception>
    /// <returns>   An OncRpcEmbeddedPortmapService. </returns>
    public static OncRpcEmbeddedPortmapService StartEmbeddedPortmapService()
    {
        Logger.Writer.LogInformation( $"{DateTime.Now.ToShortTimeString()} Checking for portmap service: " );
        bool externalPortmap = OncRpcEmbeddedPortmapService.IsPortmapRunning();
        if ( externalPortmap )
            Logger.Writer.LogInformation( "A portmap service is already running." );
        else
            Logger.Writer.LogInformation( "No portmap service available." );

        // Create embedded portmap service and check whether is has sprung
        // into action.

        Logger.Writer.LogInformation( "Creating embedded portmap instance: " );
        OncRpcEmbeddedPortmapService epm = new();

        if ( !epm.EmbeddedPortmapInUse() )
            Logger.Writer.LogInformation( "embedded service not used: " );
        else
            Logger.Writer.LogInformation( "embedded service started: " );

        if ( epm.EmbeddedPortmapInUse() == externalPortmap )
        {
            throw new InvalidOperationException( "Portmap service is not available or not in use." );
        }
        Stopwatch sw = Stopwatch.StartNew();
        externalPortmap = OncRpcEmbeddedPortmapService.IsPortmapRunning();
        if ( !externalPortmap )
            throw new InvalidOperationException( "Portmap service is not running." );
        Logger.Writer.LogInformation( $"portmap service is {(externalPortmap ? "running" : "idle")}; elapsed: {sw.ElapsedMilliseconds:0}ms" );
        return epm;
    }

    #endregion

    #region " actions "

    /// <summary>
    /// Indicates whether a portmap service (regardless whether it's supplied by the operating system
    /// or an embedded portmap service) is currently running.
    /// </summary>
    /// <remarks> Unit tests shows this to take 3 ms. </remarks>
    /// <param name="checkTimeout"> timeout in milliseconds to wait before assuming that no portmap
    ///                             service is currently available [3000]. </param>
    /// <returns>
    /// <see cref="T:true"/>, if a portmap service (either external or
    /// embedded) is running and can be contacted.
    /// </returns>
    public static bool IsPortmapRunning( int checkTimeout = 3000 )
    {
        bool available = false;
        try
        {
            using OncRpcPortmapClient portmap = new( IPAddress.Loopback, OncRpcProtocols.OncRpcUdp, checkTimeout );
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

    #endregion

    #region " members "

    /// <summary>
    /// Portmap object acting as embedded portmap service or <see langword="null"/>
    /// if no embedded portmap service is necessary because the operating system already supplies one
    /// or another port mapper is already running.
    /// </summary>
    private readonly EmbeddedPortmapService? _embeddedPortmapService;

    /// <summary>   Returns the embedded portmap service. </summary>
    /// <returns>
    /// Embedded portmap object or <see langword="null"/> if no embedded portmap service has been started.
    /// </returns>
    public virtual OncRpcPortMapService? PortmapService => this._embeddedPortmapService;


    /// <summary>   References thread object running the embedded portmap service. </summary>
    private readonly EmbeddedPortmapServiceThread? _embeddedPortmapThread;

    #endregion

    #region " embedded portmap service and thread classes "

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

    #endregion

}
