using System.Diagnostics;

using cc.isr.ONC.RPC.Logging;
using cc.isr.ONC.RPC.Server;

namespace cc.isr.ONC.RPC.Portmap;

/// <summary>
/// The class server <see cref="OncRpcEmbeddedPortmapServiceStub"/> provides an embeddable
/// Portmap service, which is automatically started in its own task if the (operating) system
/// does not already provide the Portmap service.
/// </summary>
/// <remarks>
/// If an embedded Portmap service is started it will stop only after the last ONC/RPC program
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
    /// Constructs an embeddable Portmap service of class server <see cref="OncRpcEmbeddedPortmapServiceStub"/>
    /// and starts the service if no other (external) Portmap service is available.
    /// </summary>
    /// <remarks>
    /// Call <see cref="StartEmbeddedPortmapServiceAsync"/> to start the Portmap service in its own
    /// task and then returns.
    /// </remarks>
    /// <param name="ioTimeout">        Timeout in milliseconds to wait before assuming that no
    ///                                 Portmap service is currently available [100]. </param>
    /// <param name="transmitTimeout">  The transmit timeout; defaults to 25 ms. </param>
    public OncRpcEmbeddedPortmapServiceStub( int ioTimeout, int transmitTimeout ) : this( OncRpcPortmapClient.TryPingPortmapService( ioTimeout, transmitTimeout ) )
    {
    }

    /// <summary>
    /// Constructs an embeddable Portmap service of class server <see cref="OncRpcEmbeddedPortmapServiceStub"/>
    /// if no other (external) Portmap service is available.
    /// </summary>
    /// <remarks>
    /// Call <see cref="StartEmbeddedPortmapServiceAsync"/> to start the Portmap service in its own
    /// task and then returns.
    /// </remarks>
    /// <param name="usingExternalPortmapService">  True if Portmap service already running, false if
    ///                                             not. </param>
    public OncRpcEmbeddedPortmapServiceStub( bool usingExternalPortmapService )
    {
        this.UsingExternalPortmapService = usingExternalPortmapService;
        if ( !usingExternalPortmapService )
            this._embeddedPortmapService = new OncRpcEmbeddedPortmapService();
    }

    /// <summary>   Starts embedded Portmap service asynchronously if not <see cref="UsingExternalPortmapService"/>. </summary>
    /// <returns>   A Task. </returns>
    public virtual async Task StartEmbeddedPortmapServiceAsync()
    {
        if ( !this.UsingExternalPortmapService )
            await this._embeddedPortmapService!.StartAsync();
    }

    /// <summary>   Stop the embedded Portmap service synchronously if it is running. </summary>
    /// <remarks>
    /// Normally you should not use this method except you need to force the embedded Portmap service
    /// to terminate. Under normal conditions the task responsible for the embedded Portmap service
    /// will terminate automatically after the last ONC/RPC program has been deregistered.
    /// This method just signals the portmap task to stop processing ONC/RPC portmap calls and to
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
    /// <remarks>
    /// Unit tests show the following timing: checked 110ms, start 220ms, ping: 4ms. Total time is
    /// around 330ms with the new default timeouts for pinging the service.
    /// </remarks>
    /// <exception cref="InvalidOperationException">    Thrown when the service failed to run or is
    ///                                                 not available. </exception>
    /// <param name="ioTimeout">        (Optional) timeout in milliseconds to wait before assuming
    ///                                 that no Portmap service is currently available [100]. </param>
    /// <param name="transmitTimeout">  (Optional) The transmit timeout; defaults to 25 ms. </param>
    /// <param name="validate">         (Optional) True to validate the port map after onset. </param>
    /// <returns>   An OncRpcEmbeddedPortmapService. </returns>
    public static OncRpcEmbeddedPortmapServiceStub StartEmbeddedPortmapService( int ioTimeout = 10, int transmitTimeout = 5, bool validate = false )
    {
        Logger.Writer.LogInformation( $"Checking for Portmap service" );
        Stopwatch sw = Stopwatch.StartNew();
        bool alreadyRunning = OncRpcPortmapClient.TryPingPortmapService( ioTimeout, transmitTimeout );
        double checkTime = sw.Elapsed.TotalMilliseconds;
        if ( alreadyRunning )
            Logger.Writer.LogInformation( "A Portmap service is already running." );
        else
            Logger.Writer.LogInformation( "No Portmap service available." );

        Logger.Writer.LogInformation( "Creating embedded Portmap instance" );
        sw = Stopwatch.StartNew();

        if ( alreadyRunning )
        {
            Logger.Writer.LogInformation( $"Found that an external Portmap service is running in {checkTime}ms" );
            return new OncRpcEmbeddedPortmapServiceStub( true );
        }
        else
        {
            OncRpcEmbeddedPortmapServiceStub epm = new( false );
            _ = epm.StartEmbeddedPortmapServiceAsync();

            // wait for the service to start.
            if ( epm.EmbeddedPortmapServiceStarted() )

                // validate if requested.
                if ( validate )
                {
                    int startTime = ( int ) sw.Elapsed.TotalMilliseconds;
                    Logger.Writer.LogInformation( "Embedded service started; trying to ping using the port map service" );

                    sw = Stopwatch.StartNew();
                    bool pinged = OncRpcPortmapClient.TryPingPortmapService();
                    if ( !pinged )
                        throw new InvalidOperationException( "Portmap service is not running." );
                    int pingTime = ( int ) sw.Elapsed.TotalMilliseconds;
                    Logger.Writer.LogInformation( $"Portmap service is {(pinged ? "running" : "idle")}; checked in {checkTime}ms, started in {startTime}ms, pinged in {pingTime}ms " );
                }
                else
                    Logger.Writer.LogInformation( $"Portmap service started; checked {checkTime:0.0} ms." );

            else
            {
                throw new InvalidOperationException( "External Portmap service is not available; embedded Portmap service did not start." );
            }
            return epm;
        }
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
    /// Portmap object acting as embedded Portmap service or (<see langword="null"/>)
    /// if no embedded Portmap service is necessary because the operating system already supplies one
    /// or another port mapper is already running.
    /// </summary>
    private OncRpcEmbeddedPortmapService? _embeddedPortmapService;

    /// <summary>   Returns the embedded Portmap service. </summary>
    /// <returns>
    /// Embedded portmap object or (<see langword="null"/>) if no embedded Portmap service has been started.
    /// </returns>
    public virtual OncRpcPortMapService? EmbeddedPortmapService => this._embeddedPortmapService;

    /// <summary>
    /// Gets or sets a value indicating whether an external Portmap service
    /// was already running when trying to establish this embedded Portmap service, in which case we
    /// do not have a <see cref="EmbeddedPortmapService"/>
    /// </summary>
    /// <value> True if Portmap service already running, false if not. </value>
    public bool UsingExternalPortmapService { get; set; } = false;

    #endregion

    #region " actions "

    /// <summary>
    /// Tries to ping the Portmap service regardless whether it's supplied by the operating system or
    /// an embedded Portmap service.
    /// </summary>
    /// <remarks>   Unit tests shows this to take 3 ms. </remarks>
    /// <param name="ioTimeout">        (Optional) The i/o timeout; defaults to 100 ms. </param>
    /// <param name="transmitTimeout">  (Optional) The transmit timeout; defaults to 25 ms. </param>
    /// <returns>
    /// <see langword="true"/>, if a Portmap service (either external or embedded) is running and can
    /// be contacted.
    /// </returns>
    public static bool TryPingPortmapService1( int ioTimeout = 10, int transmitTimeout = 5 )
    {
        using OncRpcPortmapClient pmapClient = new( IPAddress.Loopback, OncRpcProtocol.OncRpcUdp, transmitTimeout );
        pmapClient.OncRpcClient!.IOTimeout = ioTimeout;
        return pmapClient.TryPingPortmapService();
    }

    /// <summary>   Determines if we can embedded Portmap service started without waiting for timeout. </summary>
    /// <returns>   <see langword="true"/> if it succeeds; otherwise, <see langword="false"/>. </returns>
    private bool EmbeddedPortmapServiceStartedImmediate()
    {
        return (this._embeddedPortmapService?.Running ?? false);
    }

    /// <summary>   Checks if the embedded Portmap service has started. </summary>
    /// <remarks>   2023-01-31. </remarks>
    /// <param name="timeout">      (Optional) The timeout; defaults to 200 ms. </param>
    /// <param name="loopDelay">    (Optional) The loop delay in milliseconds. </param>
    /// <returns>   <see langword="true"/> if it succeeds; otherwise, <see langword="false"/>. </returns>
    public virtual bool EmbeddedPortmapServiceStarted( int timeout = 200, int loopDelay = 5 )
    {
        DateTime endTime = DateTime.Now.AddMilliseconds( timeout );
        while ( endTime > DateTime.Now || !this.EmbeddedPortmapServiceStartedImmediate() )
        { Task.Delay( loopDelay ).Wait(); }
        return this.EmbeddedPortmapServiceStartedImmediate();
    }

    #endregion

}
