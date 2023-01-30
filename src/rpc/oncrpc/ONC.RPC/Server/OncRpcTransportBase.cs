using System.Net.Sockets;

using cc.isr.ONC.RPC.Client;
using cc.isr.ONC.RPC.Logging;
using cc.isr.ONC.RPC.Portmap;

namespace cc.isr.ONC.RPC.Server;

/// <summary>
/// Instances of class server <see cref="OncRpcTransportBase"/> encapsulate XDR streams of
/// ONC/RPC servers.
/// </summary>
/// <remarks>
/// Using server transports, ONC/RPC calls are received and the corresponding replies are later
/// sent back after handling. <para>
/// 
/// Note that the server-specific dispatcher handling requests (done through <see cref="IOncRpcDispatchable"/>
/// will only directly deal with <see cref="OncRpcCallHandler"/> objects. These call
/// information objects reference OncRpcServerTransport object, but the server programmer
/// typically will never touch them, as the call information object already contains all
/// necessary information about a call, so replies can be sent back (and this is definitely a
/// sentence containing too many words). </para> <para>
/// 
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public abstract class OncRpcTransportBase : ICloseable
{

    #region " construction and cleanup "

    /// <summary>
    /// Create a new instance of a server <see cref="OncRpcTransportBase"/> which encapsulates
    /// XDR streams of an ONC/RPC server.
    /// </summary>
    /// <remarks>
    /// Using a server transport, ONC/RPC calls are received and the corresponding replies are sent
    /// back. <para>
    /// 
    /// We do not create any XDR streams here, as it is the responsibility of derived classes to
    /// create appropriate XDR stream objects for the respective kind of transport mechanism used
    /// (like TCP/IP and UDP/IP).</para>
    /// </remarks>
    /// <param name="dispatcher">           Reference to interface of an object capable of
    ///                                     dispatching (handling) ONC/RPC calls. </param>
    /// <param name="port">                 Number of port where the server will wait for incoming
    ///                                     calls. </param>
    /// <param name="protocol">             The protocol, .e.g., <see cref="OncRpcProtocol.OncRpcTcp"/>
    ///                                     or <see cref="OncRpcProtocol.OncRpcUdp"/> </param>
    /// <param name="registeredPrograms">   Array of program and version number tuples of the ONC/RPC
    ///                                     programs and versions handled by this transport. </param>
    protected OncRpcTransportBase( IOncRpcDispatchable dispatcher, int port, OncRpcProtocol protocol,
                                   OncRpcProgramInfo[] registeredPrograms )
    {
        this.Dispatcher = dispatcher;
        this.Port = port;
        this.Protocol = protocol;
        this.RegisteredPrograms = registeredPrograms;
        this._characterEncoding = OncRpcTransportBase.EncodingDefault;
    }

    /// <summary>   Close the server transport and free any resources associated with it. </summary>
    /// <remarks>
    /// Note that the server transport is <b>not deregistered</b>. You'll
    /// have to do it manually if you need to do so. The reason for this behavior is, that the port
    /// mapper removes all entries regardless of the protocol (TCP/IP or UDP/IP) for a given ONC/RPC
    /// program number and version. <para>
    /// 
    /// Derived classes can choose between different behavior for shutting down the associated
    /// transport handler threads: </para> <list type="bullet"> <item>
    /// Close the transport immediately and let the threads stumble on the closed network connection.</item>
    /// <item>
    /// Wait for handler threads to complete their current ONC/RPC request (with timeout), then close
    /// connections and kill the threads. </item></list>
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

    /// <summary>
    /// Releases unmanaged, large objects and (optionally) managed resources used by this class.
    /// Closes the server transport and frees any resources associated with it.
    /// </summary>
    /// <remarks>
    /// Note that the server transport is <b>not deregistered</b>. You'll have to do it manually if
    /// you need to do so. The reason for this behavior is that the portmapper removes all entries
    /// regardless of the protocol (TCP/IP or UDP/IP) for a given ONC/RPC program number and version.
    /// <para>
    /// 
    /// Calling this method on a <see cref="OncRpcTcpTransport"/> results in the listening TCP
    /// network socket immediately being closed. In addition, all server transports handling the
    /// individual TCP/IP connections will also be closed. The handler threads will therefore either
    /// terminate directly or when they try to sent back replies.</para>
    /// </remarks>
    /// <exception cref="AggregateException">   Thrown when an Aggregate error condition occurs. </exception>
    /// <param name="disposing">    True to release large objects and managed and unmanaged resources;
    ///                             false to release only unmanaged resources and large objects. </param>
    protected virtual void Dispose( bool disposing )
    {
        List<Exception> exceptions = new();
        if ( disposing )
        {
            // dispose managed state (managed objects)

            XdrEncodingStreamBase? encoder = this.Encoder;
            if ( encoder is not null )
            {
                try
                {
                    encoder.Close();
                }
                catch ( Exception ex )
                { exceptions.Add( ex ); }
                finally
                {
                    this.Encoder = null;
                }
            }

            XdrDecodingStreamBase? decoder = this.Decoder;
            if ( decoder is not null )
            {
                try
                {
                    decoder.Close();
                }
                catch ( Exception ex )
                { exceptions.Add( ex ); }
                finally
                {
                    this.Decoder = null;
                }
            }
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

    #region " defaults "

    /// <summary>   Gets or sets the default buffer size. </summary>
    /// <value> The buffer size default. </value>
    public static int BufferSizeDefault { get; set; } = 32768;

    /// <summary>   Gets or sets the default minimum buffer size. </summary>
    public static int MinBufferSizeDefault { get; set; } = 1024;

    /// <summary>
    /// Gets or sets the default timeout for sending RPC calls or receiving RPC replies.
    /// </summary>
    /// <value> The transmit timeout default. </value>
    public static int TransmitTimeoutDefault { get; set; } = 1000;

    /// <summary>   Gets or sets the i/o timeout default. </summary>
    /// <value> The i/o timeout default. </value>
    public static int IOTimeoutDefault { get; set; } = 3000;

    /// <summary>   Gets or sets the default encoding. </summary>
    /// <remarks>
    /// The default encoding for VXI-11 is <see cref="Encoding.ASCII"/>, which is a subset of <see cref="Encoding.UTF8"/>
    /// </remarks>
    /// <value> The default encoding. </value>
    public static Encoding EncodingDefault { get; set; } = Encoding.UTF8;

    #endregion

    #region " members "

    /// <summary>
    /// Gets or sets the port number of socket this server transport listens on for incoming ONC/RPC
    /// calls.
    /// </summary>
    /// <value> The Port number where we're listening for incoming ONC/RPC requests. </value>
    internal int Port { get; set; }

    /// <summary>   Gets or sets the protocol, e.g., <see cref="OncRpcProtocol.OncRpcTcp"/> or <see cref="OncRpcProtocol.OncRpcTcp"/>. </summary>
    /// <value> The protocol. </value>
    internal OncRpcProtocol Protocol { get; private set; }

    private Encoding _characterEncoding;
    /// <summary>
    /// Gets or sets the character encoding for serializing strings. 
    /// </summary>
    /// <value>
    /// The encoding for serializing strings. If (<see langword="null"/>), then the
    /// system's default encoding is used.
    /// </value>
    public Encoding CharacterEncoding
    {
        get => this._characterEncoding;
        set {
            this._characterEncoding = value;
            if ( this.Encoder is not null ) this.Encoder.CharacterEncoding = value;
            if ( this.Decoder is not null ) this.Decoder.CharacterEncoding = value;
        }
    }

    /// <summary>
    /// Gets or sets the transport registration information consisting of the program and version
    /// number tuples handled by this server transport.
    /// </summary>
    /// <value> Information describing the transport registration. </value>
    internal OncRpcProgramInfo[] RegisteredPrograms { get; set; }

    #endregion

    #region " actions "

    /// <summary>
    /// Register the <see cref="Protocol"/> (UDP/IP or TCP/IP) port where this server transport waits for incoming requests with the
    /// ONC/RPC portmapper.
    /// </summary>
    /// <remarks>
    /// The contract of this method is, that derived classes implement the appropriate communication
    /// with the portmapper, so the transport is registered only for the protocol supported by a
    /// particular kind of server transport.
    /// </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    public virtual void Register()
    {
        try
        {
            using OncRpcPortmapClient pmapClient = new( IPAddress.Loopback, OncRpcProtocol.OncRpcUdp, OncRpcUdpTransport.TransmitTimeoutDefault );
            pmapClient.OncRpcClient!.IOTimeout = OncRpcUdpClient.IOTimeoutDefault;
            foreach ( var transportRegistrationInfo in this.RegisteredPrograms )
            {
                // Try to register the port for our transport with the local ONC/RPC
                // portmapper. If this fails, bail out with an exception.

                if ( !pmapClient.SetPort( transportRegistrationInfo.Program, transportRegistrationInfo.Version, this.Protocol, this.Port ) )
                    throw new OncRpcException( OncRpcExceptionReason.OncRpcCannotRegisterTransport );
            }
        }
        catch ( IOException )
        {
            throw new OncRpcException( OncRpcExceptionReason.OncRpcFailed );
        }
    }

    /// <summary>
    /// Unregisters the port where this server transport waits for incoming requests from the ONC/RPC
    /// port mapper.
    /// </summary>
    /// <remarks>
    /// Note that due to the way Sun decided to implement its ONC/RPC portmapper process,
    /// deregistering one server transports causes all entries for the same program and version to be
    /// removed, regardless of the protocol (UDP/IP or TCP/IP) used. Sigh.
    /// </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    public virtual void Unregister()
    {
        try
        {
            using OncRpcPortmapClient pmapClient = new( IPAddress.Loopback, OncRpcProtocol.OncRpcUdp, OncRpcUdpTransport.TransmitTimeoutDefault );
            pmapClient.OncRpcClient!.IOTimeout = OncRpcUdpClient.IOTimeoutDefault;
            foreach ( OncRpcProgramInfo registeredProgram in this.RegisteredPrograms )
                _ = pmapClient.UnsetPort( registeredProgram.Program, registeredProgram.Version );
        }
        catch ( System.IO.IOException )
        {
            throw new OncRpcException( OncRpcExceptionReason.OncRpcFailed );
        }
    }

    /// <summary>
    /// Creates a new thread and uses this thread to listen to incoming ONC/RPC requests, then
    /// dispatches them and finally sends back the appropriate reply messages.
    /// </summary>
    /// <remarks>
    /// Note that you have to supply an implementation for this abstract method in derived classes.
    /// Your implementation needs to create a new thread to wait for incoming requests. The method
    /// has to return immediately for the calling thread.
    /// </remarks>
    /// <param name="cancelSource"> The cancel source. </param>
    public abstract void Listen( CancellationTokenSource cancelSource );


    /// <summary>   Stops listening . </summary>
    /// <remarks>   @atecoder 2023-01-23. </remarks>
    /// <param name="cancelSource"> The cancel source. </param>
    public abstract void Unlisten( CancellationTokenSource cancelSource );


    #endregion

    #region " Encoding / Decoding "

    /// <summary>   Retrieves the parameters sent within an ONC/RPC call message. </summary>
    /// <remarks>
    /// It also makes sure that the deserialization process is properly finished after the call
    /// parameters have been retrieved. Under the hood this method therefore calls <see cref="XdrDecodingStreamBase.EndDecoding()"/>
    /// to free any pending resources from the decoding stage.
    /// </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="call"> The call. </param>
    internal abstract void RetrieveCall( IXdrCodec call );

    /// <summary>
    /// Returns XDR stream which can be used for deserializing the parameters of this ONC/RPC call.
    /// </summary>
    /// <remarks>
    /// This method belongs to the lower-level access pattern when handling ONC/RPC calls.
    /// </remarks>
    /// <returns>   Reference to decoding XDR stream. </returns>
    public XdrDecodingStreamBase? Decoder { get; set; }

    /// <summary>   Finishes call parameter deserialization. </summary>
    /// <remarks>
    /// Afterwards the XDR stream returned by <see cref="Decoder"/>
    /// must not be used any more. This method belongs to the lower-level access pattern when
    /// handling ONC/RPC calls.
    /// </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    internal abstract void EndDecoding();

    /// <summary>
    /// Returns XDR stream which can be used for serializing the reply to this ONC/RPC call.
    /// </summary>
    /// <remarks>
    /// This method belongs to the lower-level access pattern when handling ONC/RPC calls.
    /// </remarks>
    /// <value>   Reference to encoding XDR stream. </value>
    public XdrEncodingStreamBase? Encoder { get; set; }

    /// <summary>   Begins the sending phase for ONC/RPC replies. </summary>
    /// <remarks>
    /// This method belongs to the lower-level access pattern when handling ONC/RPC calls.
    /// </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="callInfo"> Information about ONC/RPC call for which we are about to send back
    ///                         the reply. </param>
    /// <param name="state">    ONC/RPC reply header indicating success or failure. </param>
    internal abstract void BeginEncoding( OncRpcCallHandler callInfo, OncRpcServerReplyMessage state );

    /// <summary>   Finishes encoding the reply to this ONC/RPC call. </summary>
    /// <remarks>
    /// Afterwards you must not use the XDR stream returned by <see cref="Encoder"/> any longer.
    /// </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    internal abstract void EndEncoding();

    /// <summary>   Sends back an ONC/RPC reply to the original caller. </summary>
    /// <remarks>
    /// This is rather a low-level method, typically not used by applications. Dispatcher handling
    /// ONC/RPC calls have to use the <see cref="OncRpcCallHandler.Reply(IXdrCodec)"/>
    /// method instead on the call object supplied to the handler. <para>
    /// 
    /// An appropriate implementation has to be provided in derived classes as it is dependent on the
    /// type of transport (whether UDP/IP or TCP/IP)
    /// used. </para>
    /// </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="callHandler">  A <see cref="OncRpcCallHandler"/> to handle the call. </param>
    /// <param name="state">        ONC/RPC reply message header indicating success or failure and
    ///                             containing associated state information. </param>
    /// <param name="reply">        If not (<see langword="null"/>), then this parameter references the
    ///                             reply to be serialized after the reply message header. </param>
    internal abstract void Reply( OncRpcCallHandler callHandler, OncRpcServerReplyMessage state, IXdrCodec reply );

    /// <summary>   Consumes the ONC/RPC call and responds to the original caller. </summary>
    /// <remarks>
    /// This is rather a low-level method, typically not used by applications. Dispatcher handling
    /// ONC/RPC calls have to use the <see cref="OncRpcCallHandler.Reply(IXdrCodec)"/>
    /// method instead on the call object supplied to the handler. <para>
    /// 
    /// An appropriate implementation has to be provided in derived classes as it is dependent on the
    /// type of transport (whether UDP/IP or TCP/IP)
    /// used. </para>
    /// </remarks>
    /// <param name="callInfo"> <see cref="OncRpcCallHandler"/> about the original call, which are
    ///                         necessary to Sends back the reply to the appropriate caller. </param>
    /// <param name="state">    ONC/RPC reply message header indicating success or failure and
    ///                         containing associated state information. </param>
    internal abstract void Reply( OncRpcCallHandler callInfo, OncRpcServerReplyMessage state );

    /// <summary>
    /// Gets or sets the reference to the interface implemented by the object capable of
    /// handling/dispatching ONC/RPC requests.
    /// </summary>
    /// <value> The dispatcher. </value>
    internal IOncRpcDispatchable Dispatcher { get; set; }

    #endregion

}
