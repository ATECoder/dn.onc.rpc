
using System.Net.Sockets;
using System.IO;

namespace cc.isr.ONC.RPC.Server;

/// <summary>
/// Instances of class <see cref="OncRpcUdpTransport"/> encapsulate UDP/IP-based XDR streams of
/// ONC/RPC servers.
/// </summary>
/// <remarks>
/// This server transport class is responsible for receiving ONC/RPC calls over UDP/IP. <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public class OncRpcUdpTransport : OncRpcTransportBase
{

    #region " construction and cleanup "

    /// <summary>
    /// Create a new instance of a <see cref="OncRpcUdpTransport"/> which encapsulates UDP/IP-
    /// based XDR streams of an ONC/RPC server.
    /// </summary>
    /// <remarks>
    /// Using a server transport, ONC/RPC calls are received and the corresponding replies are sent
    /// back. This constructor is a convenience constructor for those transports handling only a
    /// single ONC/RPC program and version number.
    /// </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="dispatcher">   Reference to interface of an object capable of dispatching
    ///                             (handling) ONC/RPC calls. </param>
    /// <param name="port">         Number of port where the server will wait for incoming calls. </param>
    /// <param name="program">      Number of ONC/RPC program handled by this server transport. </param>
    /// <param name="version">      Version number of ONC/RPC program handled. </param>
    /// <param name="bufferSize">   Size of buffer for receiving and sending UDP/IP datagrams
    ///                             containing ONC/RPC call and reply messages. </param>
    public OncRpcUdpTransport( IOncRpcDispatchable dispatcher, int port, int program, int version, int bufferSize ) : this( dispatcher, port, new
        OncRpcProgramInfo[] { new OncRpcProgramInfo( program, version ) }, bufferSize )
    {
    }

    /// <summary>
    /// Create a new instance of a <see cref="OncRpcUdpTransport"/> which encapsulates UDP/IP-
    /// based XDR streams of an ONC/RPC server.
    /// </summary>
    /// <remarks>
    /// Using a server transport, ONC/RPC calls are received and the corresponding replies are sent
    /// back. This constructor is a convenience constructor for those transports handling only a
    /// single ONC/RPC program and version number.
    /// </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="dispatcher">   Reference to interface of an object capable of dispatching
    ///                             (handling) ONC/RPC calls. </param>
    /// <param name="port">         Number of port where the server will wait for incoming calls. </param>
    /// <param name="info">         Array of program and version number tuples of the ONC/RPC
    ///                             programs and versions handled by this transport. </param>
    /// <param name="bufferSize">   Size of buffer for receiving and sending UDP/IP datagrams
    ///                             containing ONC/RPC call and reply messages. </param>
    public OncRpcUdpTransport( IOncRpcDispatchable dispatcher, int port,
        OncRpcProgramInfo[] info, int bufferSize ) : this( dispatcher, null, port, info, bufferSize )
    {
    }

    /// <summary>
    /// Create a new instance of a <see cref="OncRpcUdpTransport"/> which encapsulates UDP/IP-
    /// based XDR streams of an ONC/RPC server.
    /// </summary>
    /// <remarks>
    /// Using a server transport, ONC/RPC calls are received and the corresponding replies are sent
    /// back. This constructor is a convenience constructor for those transports handling only a
    /// single ONC/RPC program and version number.
    /// </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="dispatcher">   Reference to interface of an object capable of dispatching
    ///                             (handling) ONC/RPC calls. </param>
    /// <param name="bindAddr">     The local Internet Address the server will bind to. </param>
    /// <param name="port">         Number of port where the server will wait for incoming calls. </param>
    /// <param name="info">         Array of program and version number tuples of the ONC/RPC
    ///                             programs and versions handled by this transport. </param>
    /// <param name="bufferSize">   Size of buffer for receiving and sending UDP/IP datagrams
    ///                             containing ONC/RPC call and reply messages. </param>
    public OncRpcUdpTransport( IOncRpcDispatchable dispatcher, IPAddress bindAddr, int port,
        OncRpcProgramInfo[] info, int bufferSize ) : base( dispatcher, port, OncRpcProtocols.OncRpcUdp, info )
    {

        // Make sure the buffer is large enough and resize system buffers
        // accordingly, if possible.

        if ( bufferSize < OncRpcTransportBase.DefaultMinBufferSize ) bufferSize = OncRpcTransportBase.DefaultMinBufferSize;
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

        this.Encoder = new XdrUdpEncodingStream( this._socket, bufferSize );
        this.Decoder = new XdrUdpDecodingStream( this._socket, bufferSize );
    }

    /// <summary>   Close the server transport and free any resources associated with it. </summary>
    /// <remarks>
    /// Note that the server transport is <b>not deregistered</b>. You'll
    /// have to do it manually if you need to do so. The reason for this behavior is, that the
    /// portmapper removes all entries regardless of the protocol (TCP/IP or UDP/IP) for a given
    /// ONC/RPC program number and version. <para>
    /// Calling this method on a <see cref="OncRpcUdpTransport"/>
    /// results in the UDP network socket immediately being closed. The handler thread will therefore
    /// either terminate directly or when it tries to sent back a reply which it was about to handle
    /// at the time the close method was called. </para>
    /// </remarks>
    public override void Close()
    {
        if ( this._socket is not null )
        {

            // Since there is a non-zero chance of getting race conditions,
            // we now first set the socket instance member to null, before
            // we close the corresponding socket. This avoids null-pointer
            // exceptions in the method which waits for new requests: it is
            // possible that this method is awakened because the socket has
            // been closed before we could set the socket instance member to
            // null. Many thanks to Michael Smith for tracking down this one.

            // @atecoder: added shutdown
            Socket socket = this._socket;
            if ( socket.Connected )
                socket.Shutdown( SocketShutdown.Both );
            this._socket = null;
            socket.Close();
        }
        base.Close();
    }

    /// <summary>   UDP socket used for datagram-based communication with ONC/RPC clients. </summary>
    private Socket _socket;

    /// <summary>
    ///  Thread which manages listening on the socket
    /// </summary>
    private Thread _listener;

    #endregion

    #region " actions "

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
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="call"> The call. </param>
    internal override void RetrieveCall( IXdrCodec call )
    {
        call.Decode( this.Decoder );
        if ( this._pendingDecoding )
        {
            this._pendingDecoding = false;
            this.Decoder.EndDecoding();
        }
    }

    /// <summary>   Finishes call parameter deserialization. </summary>
    /// <remarks>
    /// Afterwards the XDR stream returned by <see cref="Decoder"/>
    /// must not be used any more. This method belongs to the lower-level access pattern when
    /// handling ONC/RPC calls.
    /// </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    internal override void EndDecoding()
    {
        if ( this._pendingDecoding )
        {
            this._pendingDecoding = false;
            this.Decoder.EndDecoding();
        }
    }

    /// <summary>   Begins the sending phase for ONC/RPC replies. </summary>
    /// <remarks>
    /// This method belongs to the lower-level access pattern when handling ONC/RPC calls.
    /// </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="callInfo"> Information about ONC/RPC call for which we are about to send back
    ///                         the reply. </param>
    /// <param name="state">    ONC/RPC reply header indicating success or failure. </param>
    internal override void BeginEncoding( OncRpcCallHandler
         callInfo, OncRpcServerReplyMessage state )
    {

        // In case decoding has not been properly finished, do it now to
        // free up pending resources, etc.

        if ( this._pendingDecoding )
        {
            this._pendingDecoding = false;
            this.Decoder.EndDecoding();
        }

        // Now start encoding using the reply message header first...

        this.Encoder.BeginEncoding( callInfo.PeerIPAddress, callInfo.PeerPort );
        state.Encode( this.Encoder );
    }

    /// <summary>   Finishes encoding the reply to this ONC/RPC call. </summary>
    /// <remarks>
    /// Afterwards you must not use the XDR stream returned by <see cref="Encoder"/>
    /// any longer.
    /// </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    internal override void EndEncoding()
    {

        // Close the case. 
        this.Encoder.EndEncoding();
    }

    /// <summary> Sends back an ONC/RPC reply to the original caller. </summary>
    /// <remarks>
    /// This is rather a low-level method, typically not used by applications. Dispatcher handling
    /// ONC/RPC calls have to use the <see cref="OncRpcCallHandler.Reply(IXdrCodec)"/>
    /// method instead on the call object supplied to the handler.
    /// </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="callInfo"> information about the original call, which are necessary to send back
    ///                         the reply to the appropriate caller. </param>
    /// <param name="state">    ONC/RPC reply message header indicating success or failure and
    ///                         containing associated state information. </param>
    /// <param name="reply">    If not <see langword="null"/>, then this parameter references the reply to
    ///                         be serialized after the reply message header. </param>
    internal override void Reply( OncRpcCallHandler callInfo, OncRpcServerReplyMessage state, IXdrCodec reply )
    {
        this.BeginEncoding( callInfo, state );
        reply?.Encode( this.Encoder );
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
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    public override void Listen()
    {
        TransportHelper t = new( this );
        this._listener = new Thread( new ThreadStart( t.Run ) ) { Name = "UDP server transport listener thread" };
        // Should be a Daemon
        //listener.setDaemon(true);
        this._listener.Start();
    }

    #endregion

    #region " Thread Aware Listener implementation "

    private sealed class TransportHelper
    {
        /// <summary>   Constructor. </summary>
        /// <param name="enclosingTransport">   The enclosing transport. </param>
        public TransportHelper( OncRpcUdpTransport enclosingTransport )
        {
            this._enclosing = enclosingTransport;
        }

        /// <summary>   Runs this object. </summary>
        public void Run()
        {
            this._enclosing.DoListen();
        }

        private readonly OncRpcUdpTransport _enclosing;
    }

    /// <summary>
    /// The real workhorse handling incoming requests, dispatching them and sending back replies.
    /// </summary>
    public virtual void DoListen()
    {
        OncRpcCallHandler callInfo = new( this );
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
                this.Decoder.BeginDecoding();
                callInfo.PeerIPAddress = this.Decoder.GetSenderAddress();
                callInfo.PeerPort = this.Decoder.GetSenderPort();
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

                callInfo.CallMessage.Decode( this.Decoder );
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
                        this.Decoder.EndDecoding();
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

                Console.WriteLine( $"Failed dispatching ONC/RPC call: \n{ex} " );

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
                        this.Decoder.EndDecoding();
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
                    if ( ex is OncRpcAuthException exception )
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