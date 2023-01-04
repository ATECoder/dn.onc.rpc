namespace cc.isr.ONC.RPC.Server;

/// <summary>
/// The abstract server <see cref="OncRpcServerStubBase"/> class is the base class upon which to build ONC/RPC-program
/// specific servers.
/// </summary>
/// <remarks>
/// This class is typically only used by <c>rpcgen</c> generated servers, which
/// provide a particular set of remote procedures as defined in an x-file.  <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public abstract class OncRpcServerStubBase
{

    private OncRpcServerTransportBase[] _transports;
    /// <summary>
    /// Sets the array containing ONC/RPC server transport objects which describe what transports an
    /// ONC/RPC server offers for handling ONC/RPC calls.
    /// </summary>
    /// <param name="transports">   Array of server transport objects to register, which will later
    ///                             handle incoming remote procedure call requests. </param>
    public virtual void SetTransports( OncRpcServerTransportBase[] transports )
    {
        this._transports = transports;
    }
    /// <summary>
    /// gets the array containing ONC/RPC server transport objects which describe what transports an ONC/RPC
    /// server offers for handling ONC/RPC calls.
    /// </summary>
    public virtual OncRpcServerTransportBase[] GetTransports()
    {
        return this._transports;
    }

    /// <summary>
    /// Array containing program and version numbers tuples this server is willing to handle.
    /// </summary>
    private OncRpcServerTransportRegistrationInfo[] _transportRegistrationInfo;

    /// <summary>
    /// Sets the array containing program and version numbers tuples this server is willing to handle.
    /// </summary>
    /// <param name="info"> Array containing program and version numbers tuples this server is
    ///                     willing to handle. </param>
    public void SetTransportRegistrationInfo( OncRpcServerTransportRegistrationInfo[] info )
    {
        this._transportRegistrationInfo  = info;
    }

    /// <summary>
    /// Returns the array containing program and version numbers tuples this server is willing to
    /// handle.
    /// </summary>
    /// <returns>   The array containing program and version numbers tuples this server is willing to
    /// handle. </returns>
    public OncRpcServerTransportRegistrationInfo[] GetTransportRegistrationInfo()
    {
        return this._transportRegistrationInfo;
    }

    /// <summary>
    /// Gets or sets (private) the notification flag for signaling the server to stop processing
    /// incoming remote procedure calls and to shut down.
    /// </summary>
    /// <value> The shutdown signal. </value>
    internal object ShutdownSignal { get; private set; } = new();

    /// <summary>
    /// All inclusive convenience method: register server transports with port mapper, then Runs the
    /// call dispatcher until the server is signaled to shut down, and finally deregister the
    /// transports.
    /// </summary>
    ///
    /// <exception cref="OncRpcException">          if the port mapper cannot be contacted
    ///                                             successfully. </exception>
    /// <exception cref="System.IO.IOException">    if a severe network I/O error occurs in the
    ///                                             server from which it cannot recover (like severe
    ///                                             exceptions thrown when waiting for now
    ///                                             connections on a server socket). </exception>
    public virtual void Run()
    {

        // Ignore all problems during unregistration.

        try
        {
            try
            {
                this.Unregister( this._transports );
            }
            catch ( OncRpcException )
            {
            }
            this.Register( this._transports );
            this.Run( this._transports );
            try
            {
                this.Unregister( this._transports );
            }
            catch ( OncRpcException )
            {
            }
        }
        finally
        {
            this.Close( this._transports );
        }
    }

    /// <summary>   Register a set of server transports with the local port mapper. </summary>
    /// <param name="transports">   Array of server transport objects to register, which will later
    ///                             handle incoming remote procedure call requests. </param>
    ///
    /// <exception cref="OncRpcException">  if the port mapper could not be contacted
    ///                                         successfully. </exception>
    public virtual void Register( OncRpcServerTransportBase[] transports )
    {
        int size = transports.Length;
        for ( int idx = 0; idx < size; ++idx )
            transports[idx].Register();
    }

    /// <summary>
    /// Process incoming remote procedure call requests from all specified transports.
    /// </summary>
    /// <remarks>
    /// To end processing and to shut the server down signal the <see cref="ShutdownSignal"/> object. 
    /// Note that the thread on which <see cref="Run()"/> is called will
    /// ignore any interruptions and will silently swallow them.
    /// </remarks>
    /// <param name="transports">   Array of server transport objects for which processing of remote
    ///                             procedure call requests should be done. </param>
    public virtual void Run( OncRpcServerTransportBase[] transports )
    {
        int size = transports.Length;
        for ( int idx = 0; idx < size; ++idx )
            transports[idx].Listen();

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

    /// <summary>
    /// Notify the RPC server to stop processing of remote procedure call requests as soon as
    /// possible.
    /// </summary>
    /// <remarks>
    /// Note that each transport has its own thread, so processing will not stop before the
    /// transports have been closed by calling the 
    /// <see cref="cc.isr.ONC.RPC.Server.OncRpcServerStubBase.Close(OncRpcServerTransportBase[])"/> method of the server.
    /// </remarks>
    public virtual void StopRpcProcessing()
    {
        if ( this.ShutdownSignal != null )
            lock ( this.ShutdownSignal )
                Monitor.Pulse( this.ShutdownSignal );
    }

    /// <summary>   Unregister a set of server transports from the local portmapper. </summary>
    /// <param name="transports">   Array of server transport objects to unregister. </param>
    ///
    /// <exception cref="OncRpcException">  with a reason of
    ///                                     <see cref="OncRpcException.OncRpcFailed"/>
    ///                                     if the portmapper could not be contacted
    ///                                     successfully. Note that it is not considered an error
    ///                                     to remove a non-existing entry from the portmapper. </exception>
    public virtual void Unregister( OncRpcServerTransportBase[] transports )
    {
        int size = transports.Length;
        for ( int idx = 0; idx < size; ++idx )
            transports[idx].Unregister();
    }

    /// <summary>   Close all transports listed in a set of server transports. </summary>
    /// <remarks>
    /// Only by calling this method processing of remote procedure calls by individual transports can
    /// be stopped. This is because every server transport is handled by its own thread.
    /// </remarks>
    /// <param name="transports">   Array of server transport objects to close. </param>
    public virtual void Close( OncRpcServerTransportBase[] transports )
    {
        int size = transports.Length;
        for ( int idx = 0; idx < size; ++idx )
            transports[idx].Close();
    }

    /// <summary>
    /// Encoding to use when deserializing strings or <see langword="null"/> if
    /// the system's default encoding should be used.
    /// </summary>
    private string _characterEncoding;

    /// <summary>   Set the character encoding for deserializing strings. </summary>
    /// <param name="characterEncoding">    the encoding to use for deserializing strings. If 
    ///                                     <see langword="null"/>, the system's default 
    ///                                     encoding is to be used. </param>
    public virtual void SetCharacterEncoding( string characterEncoding )
    {
        this._characterEncoding = characterEncoding;
        int size = this._transports.Length;
        for ( int idx = 0; idx < size; ++idx )
            this._transports[idx].SetCharacterEncoding( characterEncoding );
    }

    /// <summary>   Get the character encoding for deserializing strings. </summary>
    /// <returns>
    /// the encoding currently used for deserializing strings. If <see langword="null"/>, then the
    /// system's default encoding is used.
    /// </returns>
    public virtual string GetCharacterEncoding()
    {
        return this._characterEncoding;
    }

}
