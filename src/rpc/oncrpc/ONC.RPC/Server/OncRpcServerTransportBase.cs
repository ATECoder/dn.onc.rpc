using System.IO;
using System.Net.Sockets;

using cc.isr.XDR;

namespace cc.isr.ONC.RPC.Server;

/// <summary>
/// Instances of class server <see cref="OncRpcServerTransportBase"/> encapsulate XDR streams of
/// ONC/RPC servers.
/// </summary>
/// <remarks>
/// Using server transports, ONC/RPC calls are received and the corresponding replies are later
/// sent back after handling. <para>
/// Note that the server-specific dispatcher handling requests (done through <see cref="IOncRpcDispatchable"/>
/// will only directly deal with <see cref="OncRpcCallInformation"/> objects. These call
/// information objects reference OncRpcServerTransport object, but the server programmer
/// typically will never touch them, as the call information object already contains all
/// necessary information about a call, so replies can be sent back (and this is definitely a
/// sentence containing too many words). </para> <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public abstract class OncRpcServerTransportBase
{

    /// <summary>   (Immutable) the default buffer size. </summary>
    public const int DefaultBufferSize = 32768;

    /// <summary>   (Immutable) the default minimum buffer size. </summary>
    public const int DefaultMinBufferSize = 1024;

    /// <summary>   (Immutable) the default transmission timeout. </summary>
    public const int DefaultTransmissionTimeout = 30000;

    /// <summary>
    /// Create a new instance of a server <see cref="OncRpcServerTransportBase"/> which encapsulates XDR streams
    /// of an ONC/RPC server.
    /// </summary>
    /// <remarks>
    /// Using a server transport, ONC/RPC calls are received and the corresponding replies are sent
    /// back. <para>
    /// We do not create any XDR streams here, as it is the responsibility
    /// of derived classes to create appropriate XDR stream objects for the respective kind of
    /// transport mechanism used (like TCP/IP and UDP/IP).</para>
    /// </remarks>
    /// <param name="dispatcher">   Reference to interface of an object capable of dispatching
    ///                             (handling) ONC/RPC calls. </param>
    /// <param name="port">         Number of port where the server will wait for incoming calls. </param>
    /// <param name="info">         Array of program and version number tuples of the ONC/RPC
    ///                             programs and versions handled by this transport. </param>
    protected OncRpcServerTransportBase( IOncRpcDispatchable dispatcher, int port, OncRpcServerTransportRegistrationInfo[] info )
    {
        this.Dispatcher = dispatcher;
        this.Port = port;
        this.TransportRegistrationInfo = info;
    }

    /// <summary>
    /// Register the port where this server transport waits for incoming requests with the ONC/RPC
    /// portmapper.
    /// </summary>
    /// <remarks>
    /// The contract of this method is, that derived classes implement the appropriate communication
    /// with the portmapper, so the transport is registered only for the protocol supported by a
    /// particular kind of server transport.
    /// </remarks>
    /// 
    /// <exception cref="OncRpcException">  if the portmapper could not be contacted successfully. </exception>
    public abstract void Register();

    /// <summary>
    /// Unregisters the port where this server transport waits for incoming requests from the ONC/RPC
    /// port mapper.
    /// </summary>
    /// <remarks>
    /// Note that due to the way Sun decided to implement its ONC/RPC portmapper process,
    /// deregistering one server transports causes all entries for the same program and version to be
    /// removed, regardless of the protocol (UDP/IP or TCP/IP) used. Sigh.
    /// </remarks>
    /// <exception cref="OncRpcException">  with a reason of <see cref="OncRpcException.OncRpcFailed"/>
    ///                                     if the portmapper could not be contacted successfully. 
    ///                                     Note that it is not considered an error to remove a non-existing 
    ///                                     entry from the portmapper. </exception>
    public virtual void Unregister()
    {
        try
        {
            OncRpcPortmapClient portmapper = new( IPAddress.Loopback );
            int size = this.TransportRegistrationInfo.Length;
            for ( int idx = 0; idx < size; ++idx )
                _ = portmapper.UnsetPort( this.TransportRegistrationInfo[idx].Program, this.TransportRegistrationInfo[idx].Version );
        }
        catch ( System.IO.IOException )
        {
            throw new OncRpcException( OncRpcException.OncRpcFailed );
        }
    }

    /// <summary>   Close the server transport and free any resources associated with it. </summary>
    /// <remarks>
    /// Note that the server transport is <b>not deregistered</b>. You'll
    /// have to do it manually if you need to do so. The reason for this behavior is, that the port
    /// mapper removes all entries regardless of the protocol (TCP/IP or UDP/IP) for a given ONC/RPC
    /// program number and version. <para>
    /// Derived classes can choose between different behavior for shutting down the associated
    /// transport handler threads: </para> <list type="bullet"> <item>
    /// Close the transport immediately and let the threads stumble on the closed network connection.</item>
    /// <item>
    /// Wait for handler threads to complete their current ONC/RPC request (with timeout), then close
    /// connections and kill the threads. </item></list>
    /// </remarks>
    public virtual void Close()
    {
        if ( this.Encoder != null )
        {
            XdrEncodingStreamBase deadXdrStream = this.Encoder;
            this.Encoder = null;
            try
            {
                deadXdrStream.Close();
            }
            catch ( System.IO.IOException )
            {
            }
            catch ( OncRpcException )
            {
            }
        }
        if ( this.Decoder != null )
        {
            XdrDecodingStreamBase deadXdrStream = this.Decoder;
            this.Decoder = null;
            try
            {
                deadXdrStream.Close();
            }
            catch ( System.IO.IOException )
            {
            }
            catch ( OncRpcException )
            {
            }
        }
    }

    /// <summary>
    /// Creates a new thread and uses this thread to listen to incoming ONC/RPC requests, then
    /// dispatches them and finally sends back the appropriate reply messages.
    /// </summary>
    /// <remarks>
    /// Note that you have to supply an implementation for this abstract
    /// method in derived classes. Your implementation needs to create a new thread to wait for
    /// incoming requests. The method has to return immediately for the calling thread.
    /// </remarks>
    public abstract void Listen();

    /// <summary>
    /// Gets or sets the port number of socket this server transport listens on for incoming ONC/RPC
    /// calls.
    /// </summary>
    /// <value> The Port number where we're listening for incoming ONC/RPC requests. </value>
    internal int Port { get; set; }

    private string _characterEncoding;
    /// <summary>
    /// Gets or sets the character encoding for serializing strings. If <see langword="null"/>, the
    /// system's default encoding is to be used.
    /// </summary>
    /// <value>
    /// The encoding for serializing strings. If <see langword="null"/>, then the
    /// system's default encoding is used.
    /// </value>
    public string CharacterEncoding
    {
        get => this._characterEncoding;
        set {
            this._characterEncoding = value;
            if ( this.Encoder is not null ) this.Encoder.CharacterEncoding = value;
            if ( this.Decoder is not null ) this.Decoder.CharacterEncoding = value;
        }
    }

    /// <summary>   Retrieves the parameters sent within an ONC/RPC call message. </summary>
    /// <remarks>
    /// It also makes sure that the deserialization process is properly finished after the call
    /// parameters have been retrieved. Under the hood this method therefore calls <see cref="XdrDecodingStreamBase.EndDecoding()"/>
    /// to free any pending resources from the decoding stage.
    /// </remarks>
    /// <param name="call"> The call. </param>
    /// 
    /// <exception cref="OncRpcException">          if an ONC/RPC exception occurs, like the data
    ///                                             could not be successfully deserialized. </exception>
    /// <exception cref="System.IO.IOException">    if an I/O exception occurs, like transmission
    ///                                             failures over the network, etc. </exception>
    internal abstract void RetrieveCall( IXdrCodec call );

    /// <summary>
    /// Returns XDR stream which can be used for deserializing the parameters of this ONC/RPC call.
    /// </summary>
    /// <remarks>
    /// This method belongs to the lower-level access pattern when handling ONC/RPC calls.
    /// </remarks>
    /// <returns>   Reference to decoding XDR stream. </returns>
    public XdrDecodingStreamBase Decoder { get; set; }

    /// <summary>   Finishes call parameter deserialization. </summary>
    /// <remarks>
    /// Afterwards the XDR stream returned by <see cref="Decoder"/>
    /// must not be used any more. This method belongs to the lower-level access pattern when
    /// handling ONC/RPC calls.
    /// </remarks>
    /// 
    /// <exception cref="OncRpcException">          if an ONC/RPC exception occurs, like the data
    ///                                             could not be successfully deserialized. </exception>
    /// <exception cref="System.IO.IOException">    if an I/O exception occurs, like transmission
    ///                                             failures over the network, etc. </exception>
    internal abstract void EndDecoding();

    /// <summary>
    /// Returns XDR stream which can be used for serializing the reply to this ONC/RPC call.
    /// </summary>
    /// <remarks>
    /// This method belongs to the lower-level access pattern when handling ONC/RPC calls.
    /// </remarks>
    /// <value>   Reference to encoding XDR stream. </value>
    public XdrEncodingStreamBase Encoder { get; set; }

    /// <summary>   Begins the sending phase for ONC/RPC replies. </summary>
    /// <remarks>
    /// This method belongs to the lower-level access pattern when handling ONC/RPC calls.
    /// </remarks>
    /// <exception cref="OncRpcException">          if an ONC/RPC exception occurs, like the data
    ///                                             could not be successfully serialized. </exception>
    /// <exception cref="System.IO.IOException">    if an I/O exception occurs, like transmission. </exception>
    /// <param name="callInfo"> Information about ONC/RPC call for which we are about to send back
    ///                         the reply. </param>
    /// <param name="state">    ONC/RPC reply header indicating success or failure. </param>
    internal abstract void BeginEncoding( OncRpcCallInformation callInfo, OncRpcServerReplyMessage state );

    /// <summary>   Finishes encoding the reply to this ONC/RPC call. </summary>
    /// <remarks>
    /// Afterwards you must not use the XDR stream returned by <see cref="Encoder"/> any longer.
    /// </remarks>
    /// <exception cref="OncRpcException">          if an ONC/RPC exception occurs, like the data
    ///                                             could not be successfully serialized. </exception>
    /// <exception cref="System.IO.IOException">    if an I/O exception occurs, like transmission
    ///                                             failures over the network, etc. </exception>
    internal abstract void EndEncoding();

    /// <summary> Sends back an ONC/RPC reply to the original caller. </summary>
    /// <remarks>
    /// This is rather a low-level method, typically not used by applications. Dispatcher handling
    /// ONC/RPC calls have to use the <see cref="OncRpcCallInformation.Reply(IXdrCodec)"/>
    /// method instead on the call object supplied to the handler. <para>
    /// An appropriate implementation has to be provided in derived classes
    /// as it is dependent on the type of transport (whether UDP/IP or TCP/IP)
    /// used. </para>
    /// </remarks>
    /// <exception cref="OncRpcException">          if an ONC/RPC exception occurs, like the data
    ///                                             could not be successfully serialized. </exception>
    /// <exception cref="System.IO.IOException">    if an I/O exception occurs, like transmission
    ///                                             failures over the network, etc. </exception>
    /// <param name="callInfo"> <see cref="OncRpcCallInformation"/> about the original call, 
    ///                         which are necessary to Sends back the reply to the appropriate caller. </param>
    /// <param name="state">    ONC/RPC reply message header indicating success or failure and
    ///                         containing associated state information. </param>
    /// <param name="reply">    If not <see langword="null"/>, then this parameter references the reply to
    ///                         be serialized after the reply message header. </param>
    internal abstract void Reply( OncRpcCallInformation callInfo, OncRpcServerReplyMessage state, IXdrCodec reply );

    /// <summary>
    /// Gets or sets the reference to the interface implemented by the object capable of
    /// handling/dispatching ONC/RPC requests.
    /// </summary>
    /// <value> The dispatcher. </value>
    internal IOncRpcDispatchable Dispatcher { get; set; }

    /// <summary>
    /// Gets or sets the transport registration information consisting of the program and version
    /// number tuples handled by this server transport.
    /// </summary>
    /// <value> Information describing the transport registration. </value>
    internal OncRpcServerTransportRegistrationInfo[] TransportRegistrationInfo { get; set; }
}
