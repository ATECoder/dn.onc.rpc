
using System.Net.Sockets;
using System.IO;

namespace cc.isr.ONC.RPC.Server;

/// <summary>
/// Instances of class <see cref="OncRpcUdpServerTransport"/> encapsulate UDP/IP-based XDR streams of
/// ONC/RPC servers.
/// </summary>
/// <remarks>
/// This server transport class is responsible for receiving ONC/RPC calls over UDP/IP. <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public class OncRpcUdpServerTransport : OncRpcServerTransportBase
{

    #region " CONSTRUCTION and CLEANUP "

    /// <summary>
    /// Create a new instance of a <see cref="OncRpcUdpServerTransport"/> which encapsulates UDP/IP-
    /// based XDR streams of an ONC/RPC server.
    /// </summary>
    /// <remarks>
    /// Using a server transport, ONC/RPC calls are received and the corresponding replies are sent
    /// back. This constructor is a convenience constructor for those transports handling only a
    /// single ONC/RPC program and version number.
    /// </remarks>
    /// <param name="dispatcher">   Reference to interface of an object capable of dispatching
    ///                             (handling) ONC/RPC calls. </param>
    /// <param name="port">         Number of port where the server will wait for incoming calls. </param>
    /// <param name="program">      Number of ONC/RPC program handled by this server transport. </param>
    /// <param name="version">      Version number of ONC/RPC program handled. </param>
    /// <param name="bufferSize">   Size of buffer for receiving and sending UDP/IP datagrams
    ///                             containing ONC/RPC call and reply messages. </param>
    ///
    /// <exception cref="OncRpcException">          Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="IOException">    Thrown when an I/O error condition occurs. </exception>
    public OncRpcUdpServerTransport( IOncRpcDispatchable dispatcher, int port, int program, int version, int bufferSize ) : this( dispatcher, port, new
        OncRpcServerTransportRegistrationInfo[] { new OncRpcServerTransportRegistrationInfo( program, version ) }, bufferSize )
    {
    }

    /// <summary>
    /// Create a new instance of a <see cref="OncRpcUdpServerTransport"/> which encapsulates UDP/IP-
    /// based XDR streams of an ONC/RPC server.
    /// </summary>
    /// <remarks>
    /// Using a server transport, ONC/RPC calls are received and the corresponding replies are sent
    /// back. This constructor is a convenience constructor for those transports handling only a
    /// single ONC/RPC program and version number.
    /// </remarks>
    /// <param name="dispatcher">   Reference to interface of an object capable of dispatching
    ///                             (handling) ONC/RPC calls. </param>
    /// <param name="port">         Number of port where the server will wait for incoming calls. </param>
    /// <param name="info">         Array of program and version number tuples of the ONC/RPC
    ///                             programs and versions handled by this transport. </param>
    /// <param name="bufferSize">   Size of buffer for receiving and sending UDP/IP datagrams
    ///                             containing ONC/RPC call and reply messages. </param>
    ///
    /// <exception cref="OncRpcException">          Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="IOException">    Thrown when an I/O error condition occurs. </exception>
    public OncRpcUdpServerTransport( IOncRpcDispatchable dispatcher, int port,
        OncRpcServerTransportRegistrationInfo[] info, int bufferSize ) : this( dispatcher, null, port, info, bufferSize )
    {
    }

    /// <summary>
    /// Create a new instance of a <see cref="OncRpcUdpServerTransport"/> which encapsulates UDP/IP-
    /// based XDR streams of an ONC/RPC server.
    /// </summary>
    /// <remarks>
    /// Using a server transport, ONC/RPC calls are received and the corresponding replies are sent
    /// back. This constructor is a convenience constructor for those transports handling only a
    /// single ONC/RPC program and version number.
    /// </remarks>
    /// <param name="dispatcher">   Reference to interface of an object capable of dispatching
    ///                             (handling) ONC/RPC calls. </param>
    /// <param name="bindAddr">     The local Internet Address the server will bind to. </param>
    /// <param name="port">         Number of port where the server will wait for incoming calls. </param>
    /// <param name="info">         Array of program and version number tuples of the ONC/RPC
    ///                             programs and versions handled by this transport. </param>
    /// <param name="bufferSize">   Size of buffer for receiving and sending UDP/IP datagrams
    ///                             containing ONC/RPC call and reply messages. </param>
    ///
    /// <exception cref="OncRpcException">          Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="IOException">    Thrown when an I/O error condition occurs. </exception>
    public OncRpcUdpServerTransport( IOncRpcDispatchable dispatcher, IPAddress bindAddr, int port,
        OncRpcServerTransportRegistrationInfo[] info, int bufferSize ) : base( dispatcher, port, info )
    {

        // Make sure the buffer is large enough and resize system buffers
        // accordingly, if possible.

        if ( bufferSize < OncRpcServerTransportBase.DefaultMinBufferSize ) bufferSize = OncRpcServerTransportBase.DefaultMinBufferSize;
        this._socket = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );
        bindAddr ??= IPAddress.Any;
        IPEndPoint localEP = new( bindAddr, port );
        this._socket.Bind( localEP );
        if ( port == 0 )
            this.Port = (( IPEndPoint ) this._socket.LocalEndPoint).Port;
        if ( this._socket.SendBufferSize < bufferSize )
            this._socket.SendBufferSize = bufferSize;
        if ( this._socket.ReceiveBufferSize < bufferSize )
            this._socket.ReceiveBufferSize = bufferSize;

        // Create the necessary encoding and decoding streams, so we can
        // communicate at all.

        this._encoder = new XdrUdpEncodingStream( this._socket, bufferSize );
        this._decoder = new XdrUdpDecodingStream( this._socket, bufferSize );
    }

    /// <summary>   Close the server transport and free any resources associated with it. </summary>
    /// <remarks>
    /// Note that the server transport is <b>not deregistered</b>. You'll
    /// have to do it manually if you need to do so. The reason for this behavior is, that the
    /// portmapper removes all entries regardless of the protocol (TCP/IP or UDP/IP) for a given
    /// ONC/RPC program number and version. <para>
    /// Calling this method on a <see cref="OncRpcUdpServerTransport"/>
    /// results in the UDP network socket immediately being closed. The handler thread will therefore
    /// either terminate directly or when it tries to sent back a reply which it was about to handle
    /// at the time the close method was called. </para>
    /// </remarks>
    public override void Close()
    {
        if ( this._socket != null )
        {

            // Since there is a non-zero chance of getting race conditions,
            // we now first set the socket instance member to null, before
            // we close the corresponding socket. This avoids null-pointer
            // exceptions in the method which waits for new requests: it is
            // possible that this method is awakened because the socket has
            // been closed before we could set the socket instance member to
            // null. Many thanks to Michael Smith for tracking down this one.

            this._socket.Shutdown( SocketShutdown.Both );
            Socket deadSocket = this._socket;
            this._socket = null;
            deadSocket.Close();
        }
        if ( this._encoder != null )
        {
            XdrEncodingStreamBase deadXdrStream = this._encoder;
            this._encoder = null;
            try
            {
                deadXdrStream.Close();
            }
            catch ( IOException )
            {
            }
            catch ( OncRpcException )
            {
            }
        }
        if ( this._decoder != null )
        {
            XdrDecodingStreamBase deadXdrStream = this._decoder;
            this._decoder = null;
            try
            {
                deadXdrStream.Close();
            }
            catch ( IOException )
            {
            }
            catch ( OncRpcException )
            {
            }
        }
    }

    /// <summary>   UDP socket used for datagram-based communication with ONC/RPC clients. </summary>
    private Socket _socket;

    /// <summary>
    ///  Thread which manages listening on the socket
    /// </summary>
    private Thread _listener;

    /// <summary>
    /// XDR encoding stream used for sending replies via UDP/IP back to an ONC/RPC client.
    /// </summary>
    private XdrUdpEncodingStream _encoder;

    /// <summary>
    /// XDR decoding stream used when receiving requests via UDP/IP from ONC/RPC clients.
    /// </summary>
    private XdrUdpDecodingStream _decoder;

    #endregion

    #region " CONFIGURATION "

    /// <summary>
    /// Set the character encoding for serializing strings. Phew. Done with the error reply. So
    /// let's wait for new incoming ONC/RPC calls...
    /// </summary>
    /// <param name="characterEncoding">    the encoding to use for serializing strings. If 
    ///                                     <see langword="null"/>, the system's default encoding is to be used. </param>
    public override void SetCharacterEncoding( string characterEncoding )
    {
        this._encoder.CharacterEncoding = characterEncoding;
        this._decoder.CharacterEncoding = characterEncoding;
    }

    /// <summary>   Get the character encoding for serializing strings. </summary>
    /// <returns>
    /// the encoding currently used for serializing strings. If <see langword="null"/>, then the
    /// system's default encoding is used.
    /// </returns>
    public override string GetCharacterEncoding()
    {
        return this._encoder.CharacterEncoding;
    }

    #endregion

    #region " OPERATIONS "


    /// <summary>
    /// Register the UDP/IP port where this server transport waits for incoming requests with the
    /// ONC/RPC portmapper.
    /// </summary>
    /// <exception cref="OncRpcException">  if the portmapper could not be contacted successfully. </exception>
    public override void Register()
    {
        try
        {
            OncRpcPortmapClient portmapper = new( IPAddress.Loopback );
            int size = this.TransportRegistrationInfo.Length;
            for ( int idx = 0; idx < size; ++idx )

                // Try to register the port for our transport with the local ONC/RPC
                // portmapper. If this fails, bail out with an exception.

                if ( !portmapper.SetPort( this.TransportRegistrationInfo[idx].Program, this.TransportRegistrationInfo[idx].Version, OncRpcProtocols.OncRpcUdp, this.Port ) )
                    throw new OncRpcException( OncRpcException.OncRpcCannotRegisterTransport );
        }
        catch ( IOException )
        {
            throw new OncRpcException( OncRpcException.OncRpcFailed );
        }
    }

    /// <summary>
    /// Indicates that <see cref="XdrDecodingStreamBase.BeginDecoding()"/> has been called for the
    /// receiving XDR stream, so that it should be closed later using
    /// <see cref="XdrDecodingStreamBase.EndDecoding"/>.
    /// </summary>
    private bool _pendingDecoding = false;

    /// <summary>   Retrieves the parameters sent within an ONC/RPC call message. </summary>
    /// <remarks>
    /// It also makes sure that the deserialization process is properly finished after the call
    /// parameters have been retrieved. Under the hood this method therefore calls
    /// <see cref="XdrDecodingStreamBase.EndDecoding()"/>
    /// to free any pending resources from the decoding stage.
    /// </remarks>
    /// <param name="call"> The call. </param>
    ///
    /// <exception cref="OncRpcException">  if an ONC/RPC exception occurs, like the data could
    ///                                     not be successfully deserialized. </exception>
    /// <exception cref="IOException">      if an I/O exception occurs, like transmission
    ///                                     failures over the network, etc. </exception>
    internal override void RetrieveCall( IXdrCodec call )
    {
        call.Decode( this._decoder );
        if ( this._pendingDecoding )
        {
            this._pendingDecoding = false;
            this._decoder.EndDecoding();
        }
    }

    /// <summary>
    /// Returns XDR stream which can be used for deserializing the parameters of this ONC/RPC call.
    /// </summary>
    /// <remarks>
    /// This method belongs to the lower-level access pattern when handling ONC/RPC calls.
    /// </remarks>
    /// <returns>   Reference to decoding XDR stream. </returns>
    internal override XdrDecodingStreamBase GetXdrDecodingStream()
    {
        return this._decoder;
    }

    /// <summary>   Finishes call parameter deserialization. </summary>
    /// <remarks>
    /// Afterwards the XDR stream returned by <see cref="GetXdrDecodingStream()"/>
    /// must not be used any more. This method belongs to the lower-level access pattern when
    /// handling ONC/RPC calls.
    /// </remarks>
    ///
    /// <exception cref="OncRpcException">  if an ONC/RPC exception occurs, like the data could
    ///                                     not be successfully deserialized. </exception>
    /// <exception cref="IOException">      if an I/O exception occurs, like transmission
    ///                                     failures over the network, etc. </exception>
    internal override void EndDecoding()
    {
        if ( this._pendingDecoding )
        {
            this._pendingDecoding = false;
            this._decoder.EndDecoding();
        }
    }

    /// <summary>
    /// Returns XDR stream which can be used for serializing the reply to this ONC/RPC call.
    /// </summary>
    /// <remarks>
    /// This method belongs to the lower-level access pattern when handling ONC/RPC calls.
    /// </remarks>
    /// <returns>   Reference to encoding XDR stream. </returns>
    internal override XdrEncodingStreamBase GetXdrEncodingStream()
    {
        return this._encoder;
    }

    /// <summary>   Begins the sending phase for ONC/RPC replies. </summary>
    /// <remarks>
    /// This method belongs to the lower-level access pattern when handling ONC/RPC calls.
    /// </remarks>
    /// <param name="callInfo"> Information about ONC/RPC call for which we are about to send back
    ///                         the reply. </param>
    /// <param name="state">    ONC/RPC reply header indicating success or failure. </param>
    ///
    /// <exception cref="OncRpcException">  if an ONC/RPC exception occurs, like the data could
    ///                                     not be successfully serialized. </exception>
    /// <exception cref="IOException">      if an I/O exception occurs, like transmission. </exception>
    internal override void BeginEncoding( OncRpcCallInformation
         callInfo, OncRpcServerReplyMessage state )
    {

        // In case decoding has not been properly finished, do it now to
        // free up pending resources, etc.

        if ( this._pendingDecoding )
        {
            this._pendingDecoding = false;
            this._decoder.EndDecoding();
        }

        // Now start encoding using the reply message header first...

        this._encoder.BeginEncoding( callInfo.PeerIPAddress, callInfo.PeerPort );
        state.Encode( this._encoder );
    }

    /// <summary>   Finishes encoding the reply to this ONC/RPC call. </summary>
    /// <remarks>
    /// Afterwards you must not use the XDR stream returned by <see cref="GetXdrEncodingStream()"/>
    /// any longer.
    /// </remarks>
    ///
    /// <exception cref="OncRpcException">  if an ONC/RPC exception occurs, like the data could
    ///                                     not be successfully serialized. </exception>
    /// <exception cref="IOException">      if an I/O exception occurs, like transmission
    ///                                     failures over the network, etc. </exception>
    internal override void EndEncoding()
    {

        // Close the case. 
        this._encoder.EndEncoding();
    }

    /// <summary> Sends back an ONC/RPC reply to the original caller. </summary>
    /// <remarks>
    /// This is rather a low-level method, typically not used by applications. Dispatcher handling
    /// ONC/RPC calls have to use the <see cref="OncRpcCallInformation.Reply(IXdrCodec)"/>
    /// method instead on the call object supplied to the handler.
    /// </remarks>
    /// <param name="callInfo"> information about the original call, which are necessary to send back
    ///                         the reply to the appropriate caller. </param>
    /// <param name="state">    ONC/RPC reply message header indicating success or failure and
    ///                         containing associated state information. </param>
    /// <param name="reply">    If not <see langword="null"/>, then this parameter references the reply to
    ///                         be serialized after the reply message header. </param>
    ///
    /// <exception cref="OncRpcException">  if an ONC/RPC exception occurs, like the data could
    ///                                     not be successfully serialized. </exception>
    /// <exception cref="IOException">      if an I/O exception occurs, like transmission
    ///                                     failures over the network, etc. </exception>
    internal override void Reply( OncRpcCallInformation callInfo, OncRpcServerReplyMessage state, IXdrCodec reply )
    {
        this.BeginEncoding( callInfo, state );
        reply?.Encode( this._encoder );
        this.EndEncoding();
    }

    /// <summary>
    /// Creates a new thread and uses this thread to listen to incoming ONC/RPC requests, then
    /// dispatches them and finally sends back the appropriate reply messages.
    /// </summary>
    /// <remarks>
    /// Control in the calling thread immediately returns after the handler thread has been created. <para>
    /// Currently only one call after the other is dispatched, so no multi threading is done when
    /// receiving multiple calls. Instead, later calls have to wait for
    /// the current call to finish before they are handled. </para>
    /// </remarks>
    ///
    /// <exception cref="OncRpcException">  if an ONC/RPC exception occurs, like the data could
    ///                                     not be successfully serialized. </exception>
    /// <exception cref="IOException">      if an I/O exception occurs, like transmission
    ///                                     failures over the network, etc. </exception>
    public override void Listen()
    {
        TransportHelper t = new( this );
        this._listener = new Thread( new ThreadStart( t.Run ) ) { Name = "UDP server transport listener thread" };
        // Should be a Daemon
        //listener.setDaemon(true);
        this._listener.Start();
    }

    #endregion

    #region " HELPERS "


    private sealed class TransportHelper
    {
        /// <summary>   Constructor. </summary>
        /// <param name="enclosingtransport">   The enclosing. </param>
        public TransportHelper( OncRpcUdpServerTransport enclosingtransport )
        {
            this._enclosing = enclosingtransport;
        }

        /// <summary>   Runs this object. </summary>
        public void Run()
        {
            this._enclosing.DoListen();
        }

        private readonly OncRpcUdpServerTransport _enclosing;
    }

    /// <summary>
    /// The real workhorse handling incoming requests, dispatching them and sending back replies.
    /// </summary>
    public virtual void DoListen()
    {
        OncRpcCallInformation callInfo = new( this );
        for (; ; )
        {

            // Start decoding the incoming call. This involves remembering
            // from whom we received the call so we can later Sends back the
            // appropriate reply message.
            // Note that for UDP-based communication we don't need to deal
            // with timeouts.

            try
            {
                this._pendingDecoding = true;
                this._decoder.BeginDecoding();
                callInfo.PeerIPAddress = this._decoder.GetSenderAddress();
                callInfo.PeerPort = this._decoder.GetSenderPort();
            }
            catch ( SocketException )
            {

                // In case of I/O Exceptions (especially socket exceptions)
                // close the file and leave the stage. There's nothing we can
                // do anymore.

                this.Close();
                return;
            }
            catch ( OncRpcException )
            {

                // In case of ONC/RPC exceptions at this stage we're silently
                // ignoring that there was some data coming in...

                continue;
            }
            try
            {

                // Pull off the ONC/RPC call header of the XDR stream.

                callInfo.CallMessage.Decode( this._decoder );
            }
            catch ( IOException )
            {

                // In case of I/O Exceptions (especially socket exceptions)
                // close the file and leave the stage. There's nothing we can
                // do anymore.

                this.Close();
                return;
            }
            catch ( OncRpcException )
            {

                // In case of ONC/RPC exceptions at this stage we're silently
                // ignoring that there was some data coming in, as we're not
                // sure we got enough information to send a matching reply
                // message back to the caller.

                if ( this._pendingDecoding )
                {
                    this._pendingDecoding = false;
                    try
                    {
                        this._decoder.EndDecoding();
                    }
                    catch ( IOException )
                    {
                        this.Close();
                        return;
                    }
                    catch ( OncRpcException )
                    {
                    }
                }
                continue;
            }
            try
            {

                // Let the dispatcher retrieve the call parameters, work on
                // it and Sends back the reply.
                // To make it once again clear: the dispatch called has to
                // pull off the parameters of the stream!

                this.Dispatcher.DispatchOncRpcCall( callInfo, callInfo.CallMessage.Program, callInfo.CallMessage.Version, callInfo.CallMessage.Procedure );
            }
            catch ( Exception ex )
            {

                Console.Out.WriteLine( $"Failed dispatching ONC/RPC call: \n{ex} " );

                // In case of some other runtime exception, we report back to
                // the caller a system error.

                // In case of UDP-bases transports we can do so, because we
                // know that we can reset the buffer and serialize another
                // reply message even in case we caught some OncRpcException.

                // Note that we "kill" the transport by closing it when we
                // got stuck with an I/O exception when trying to send back
                // an error reply.

                if ( this._pendingDecoding )
                {
                    this._pendingDecoding = false;
                    try
                    {
                        this._decoder.EndDecoding();
                    }
                    catch ( IOException )
                    {
                        this.Close();
                        return;
                    }
                    catch ( OncRpcException )
                    {
                    }
                }

                // Check for authentication exceptions, which are reported back
                // as is. Otherwise, just report a system error
                // -- very generic, indeed.

                try
                {
                    if ( ex is OncRpcAuthenticationException exception )
                        callInfo.ReplyAuthError( exception.AuthStatus );
                    else
                        callInfo.ReplySystemError();
                }
                catch ( SocketException )
                {
                    this.Close();
                    return;
                }
                catch ( NullReferenceException )
                {
                    // Until I have a better way to clean up the threads on the sockets,
                    // we need to catch this here and SocketException all over the place
                    this.Close();
                    return;
                }
                catch ( OncRpcException )
                {
                }
            }
        }
    }

    #endregion

}
