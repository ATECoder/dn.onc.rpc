
using System.Net.Sockets;

namespace cc.isr.ONC.RPC.Server;

/// <summary>
/// Instances of class <see cref="OncRpcTcpServerTransport"/> encapsulate TCP/IP-based XDR streams
/// of ONC/RPC servers.
/// </summary>
/// <remarks>
/// Remote Tea authors: Harald Albrecht, Jay Walters.
/// </remarks>
public class OncRpcTcpConnectionServerTransport : OncRpcServerTransportBase
{

    #region " CONSTRUCTION and CLEANUP "

    /// <summary>
    /// Create a new instance of a <see cref="OncRpcTcpConnectionServerTransport"/>
    /// which encapsulates TCP/IP-based XDR streams of an ONC/RPC server.
    /// </summary>
    /// <remarks>
    /// This particular server transport handles individual ONC/RPC connections over TCP/IP. This
    /// constructor is a convenience constructor for those transports handling only a single ONC/RPC
    /// program and version number.
    /// </remarks>
    /// <param name="dispatcher">           Reference to interface of an object capable of
    ///                                     dispatching (handling) ONC/RPC calls. </param>
    /// <param name="socket">               TCP/IP-based socket of new connection. </param>
    /// <param name="program">              Number of ONC/RPC program handled by this server
    ///                                     transport. </param>
    /// <param name="version">              Version number of ONC/RPC program handled. </param>
    /// <param name="bufferSize">           Size of buffer used when receiving and sending chunks of
    ///                                     XDR fragments over TCP/IP. The fragments built up to form
    ///                                     ONC/RPC call and reply messages. </param>
    /// <param name="parent">               Parent server transport which created us. </param>
    /// <param name="transmissionTimeout">  Inherited transmission timeout. </param>
    ///
    /// <exception cref="OncRpcException">          Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="System.IO.IOException">    Thrown when an I/O error condition occurs. </exception>
    public OncRpcTcpConnectionServerTransport( IOncRpcDispatchable dispatcher, Socket socket, int program, int version, int bufferSize,
        OncRpcTcpServerTransport parent, int transmissionTimeout ) : this( dispatcher, socket,
             new OncRpcServerTransportRegistrationInfo[] { new OncRpcServerTransportRegistrationInfo( program, version ) },
             bufferSize, parent, transmissionTimeout )
    {
    }

    /// <summary>
    /// Create a new instance of a <see cref="OncRpcTcpConnectionServerTransport"/>
    /// which encapsulates TCP/IP-based XDR streams of an ONC/RPC server.
    /// </summary>
    /// <remarks>
    /// This particular server transport handles individual ONC/RPC connections over TCP/IP.
    /// </remarks>
    /// <param name="dispatcher">           Reference to interface of an object capable of
    ///                                     dispatching (handling) ONC/RPC calls. </param>
    /// <param name="socket">               TCP/IP-based socket of new connection. </param>
    /// <param name="info">                 Array of program and version number tuples of the ONC/RPC
    ///                                     programs and versions handled by this transport. </param>
    /// <param name="bufferSize">           Size of buffer used when receiving and sending chunks of
    ///                                     XDR fragments over TCP/IP. The fragments built up to form
    ///                                     ONC/RPC call and reply messages. </param>
    /// <param name="parent">               Parent server transport which created us. </param>
    /// <param name="transmissionTimeout">  Inherited transmission timeout. </param>
    ///
    /// <exception cref="OncRpcException">          Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="System.IO.IOException">    Thrown when an I/O error condition occurs. </exception>
    public OncRpcTcpConnectionServerTransport( IOncRpcDispatchable dispatcher, Socket socket,
        OncRpcServerTransportRegistrationInfo[] info, int bufferSize, OncRpcTcpServerTransport parent,
        int transmissionTimeout ) : base( dispatcher, 0, OncRpcProtocols.OncRpcTcp, info )
    {
        this._parent = parent;
        this._transmissionTimeout = transmissionTimeout;

        // Make sure the buffer is large enough and resize system buffers
        // accordingly, if possible.

        if ( bufferSize < OncRpcServerTransportBase.DefaultMinBufferSize ) bufferSize = OncRpcServerTransportBase.DefaultMinBufferSize;
        this._socket = socket;
        this.Port = (( IPEndPoint ) socket.RemoteEndPoint).Port;
        if ( socket.SendBufferSize < bufferSize )
            socket.SendBufferSize = bufferSize;
        if ( socket.ReceiveBufferSize < bufferSize )
            socket.ReceiveBufferSize = bufferSize;

        // Create the necessary encoding and decoding streams, so we can
        // communicate at all.

        this.Encoder = new XdrTcpEncodingStream( socket, bufferSize );
        this.Decoder = new XdrTcpDecodingStream( socket, bufferSize );

        // Inherit the character encoding setting from the listening
        // transport (parent transport).
        this.CharacterEncoding = parent.CharacterEncoding;
    }

    /// <summary>   Close the server transport and free any resources associated with it. </summary>
    /// <remarks>
    /// Note that the server transport is <b>not deregistered</b>. You'll have to do it manually if
    /// you need to do so. The reason for this behavior is, that the portmapper removes all entries
    /// regardless of the protocol (TCP/IP or UDP/IP) for a given ONC/RPC program number and version. <para>
    /// Calling this method on a <see cref="OncRpcTcpServerTransport"/>
    /// results in the listening TCP network socket immediately being closed. The handler thread will
    /// therefore either terminate directly or when it tries to sent back replies. </para>
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

            // TODO: uncomment this and test:
            // this._socket.Shutdown( SocketShutdown.Both );
            Socket deadSocket = this._socket;
            this._socket = null;
            try
            {
                deadSocket.Close();
            }
            catch ( System.IO.IOException )
            {
            }
        }
        base.Close();
        if ( this._parent != null )
        {
            this._parent.RemoveTransport( this );
            this._parent = null;
        }
    }

    /// <summary>   Finalizer. </summary>
    ~OncRpcTcpConnectionServerTransport()
    {
        this._parent?.RemoveTransport( this );
    }

    /// <summary>
    /// TCP socket used for stream-based communication with ONC/RPC
    /// clients.
    /// </summary>
    private Socket _socket;

    /// <summary>
    /// Indicates that <see cref="OncRpcServerTransportBase.BeginEncoding"/> has been called for the
    /// receiving XDR stream, so that it should be closed later using
    /// <see cref="OncRpcServerTransportBase.EndDecoding"/>.
    /// </summary>
    private bool _pendingDecoding = false;

    /// <summary>
    /// Indicates that <see cref="OncRpcServerTransportBase.BeginEncoding"/> has been called for the
    /// sending XDR stream, so in face of exceptions we cannot send an
    /// error reply to the client but only drop the connection.
    /// </summary>
    private bool _pendingEncoding = false;

    /// <summary>
    /// Reference to the TCP/IP transport which created us to handle a
    /// new ONC/RPC connection.
    /// </summary>
    private OncRpcTcpServerTransport _parent;

    /// <summary>
    /// Timeout during the phase where data is received within calls, or data is sent within replies.
    /// </summary>
    private readonly int _transmissionTimeout;

    #endregion

    #region " Operation Methods "

    /// <summary>   Do not call. </summary>
    /// <exception cref="Exception">    because this method must not be called for an individual
    ///                                 TCP/IP-based server transport. </exception>
    public override void Register()
    {
        throw new InvalidOperationException( $"{nameof( OncRpcTcpServerTransport.Register )} must not be called for an individual TCP/IP-based server transport." );
    }

    /// <summary>   Retrieves the parameters sent within an ONC/RPC call message. </summary>
    /// <remarks>
    /// It also makes sure that the deserialization process is properly finished after the call
    /// parameters have been retrieved. Under the hood this method therefore calls
    /// <see cref="XdrDecodingStreamBase.EndDecoding()"/>
    /// to free any pending resources from the decoding stage.
    /// </remarks>
    /// <param name="call"> The call. </param>
    ///
    /// <exception cref="OncRpcException">          if an ONC/RPC exception occurs, like the data
    ///                                             could not be successfully deserialized. </exception>
    /// <exception cref="System.IO.IOException">    if an I/O exception occurs, like transmission
    ///                                             failures over the network, etc. </exception>
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
    ///
    /// <exception cref="OncRpcException">          if an ONC/RPC exception occurs, like the data
    ///                                             could not be successfully deserialized. </exception>
    /// <exception cref="System.IO.IOException">    if an I/O exception occurs, like transmission
    ///                                             failures over the network, etc. </exception>
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
    /// <param name="callInfo"> Information about ONC/RPC call for which we are about to send back
    ///                         the reply. </param>
    /// <param name="state">    ONC/RPC reply header indicating success or failure. </param>
    ///
    /// <exception cref="OncRpcException">          if an ONC/RPC exception occurs, like the data
    ///                                                 could not be successfully serialized. </exception>
    /// <exception cref="System.IO.IOException">    if an I/O exception occurs, like
    ///                                                 transmission. </exception>
    internal override void BeginEncoding( OncRpcCallInformation callInfo, OncRpcServerReplyMessage state )
    {

        // In case decoding has not been properly finished, do it now to
        // free up pending resources, etc.

        if ( this._pendingDecoding )
        {
            this._pendingDecoding = false;
            this.Decoder.EndDecoding();
        }

        // Now start encoding using the reply message header first...

        this._pendingEncoding = true;
        this.Encoder.BeginEncoding( callInfo.PeerIPAddress, callInfo.PeerPort );
        state.Encode( this.Encoder );
    }

    /// <summary>   Finishes encoding the reply to this ONC/RPC call. </summary>
    /// <remarks>
    /// Afterwards you must not use the XDR stream returned by <see cref="Encoder"/>
    /// any longer.
    /// </remarks>
    ///
    /// <exception cref="OncRpcException">          if an ONC/RPC exception occurs, like the data
    ///                                             could not be successfully serialized. </exception>
    /// <exception cref="System.IO.IOException">    if an I/O exception occurs, like transmission
    ///                                             failures over the network, etc. </exception>
    internal override void EndEncoding()
    {
        // Close the case
        this.Encoder.EndEncoding();
        this._pendingEncoding = false;
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
    /// <exception cref="OncRpcException">          if an ONC/RPC exception occurs, like the data
    ///                                             could not be successfully serialized. </exception>
    /// <exception cref="System.IO.IOException">    if an I/O exception occurs, like transmission
    ///                                             failures over the network, etc. </exception>
    internal override void Reply( OncRpcCallInformation callInfo, OncRpcServerReplyMessage state, IXdrCodec reply )
    {
        this.BeginEncoding( callInfo, state );
        reply?.Encode( this.Encoder );
        this.EndEncoding();
    }

    /// <summary>
    /// Creates a new thread and uses this thread to handle the new connection to receive ONC/RPC
    /// requests, then dispatching them and finally sending back reply messages.
    /// </summary>
    /// <remarks>
    /// Control in the calling thread immediately returns after the handler thread has been created. <para>
    /// Currently only one call after the other is dispatched, so no
    /// multi threading is done when receiving multiple calls. Instead, later calls have to wait for
    /// the current call to finish before they are handled.</para>
    /// </remarks>
    public override void Listen()
    {
        TransportHelper t = new( this );
        Thread listener = new( new ThreadStart( t.Run ) ) {
            Name = "TCP server transport connection thread"
        };
        // Should be a Daemon thread if possible
        // listener.setDaemon(true);
        listener.Start();
    }

    #endregion

    #region " Thread-Aware Listen Implementation "

    private sealed class TransportHelper
    {
        /// <summary>   Constructor. </summary>
        /// <param name="enclosing">   The enclosing. </param>
        public TransportHelper( OncRpcTcpConnectionServerTransport enclosing )
        {
            this._enclosing = enclosing;
        }

        /// <summary>   Runs this object. </summary>
        public void Run()
        {
            this._enclosing.DoListen();
        }

        private readonly OncRpcTcpConnectionServerTransport _enclosing;
    }

    private void DoListen()
    {
        OncRpcCallInformation callInfo = new( this );
        for (; ; )
        {

            // Start decoding the incoming call. This involves remembering
            // from whom we received the call so we can later Sends back the
            // appropriate reply message.

            try
            {
                this._socket.ReceiveTimeout = 0;
                this._pendingDecoding = true;
                this.Decoder.BeginDecoding();
                callInfo.PeerIPAddress = this.Decoder.GetSenderAddress();
                callInfo.PeerPort = this.Decoder.GetSenderPort();
                this._socket.ReceiveTimeout = this._transmissionTimeout;
            }
            catch ( System.IO.IOException )
            {

                // In case of I/O Exceptions (especially socket exceptions)
                // close the file and leave the stage. There's nothing we can
                // do anymore.

                this.Close();
                return;
            }
            catch ( OncRpcException )
            {

                // In case of ONC/RPC exceptions at this stage kill the
                // connection.

                this.Close();
                return;
            }
            try
            {

                // Pull off the ONC/RPC call header of the XDR stream.

                callInfo.CallMessage.Decode( this.Decoder );
            }
            catch ( System.IO.IOException )
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
                    catch ( System.IO.IOException )
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
                Console.WriteLine( $"Failed dispatching ONC/RPC Call: \n{ex} " );

                // In case of some other runtime exception, we report back to
                // the caller a system error. We cannot do this if we don't
                // got the exception when serializing the reply, in this case
                // all we can do is to drop the connection. If a reply was not
                // yet started, we can safely send a system error reply.

                if ( this._pendingEncoding )
                {
                    this.Close();
                    // Drop the connection...
                    return;
                }
                // ...and kill the transport.

                // Looks safe, so we try to Sends back an error reply.

                if ( this._pendingDecoding )
                {
                    this._pendingDecoding = false;
                    try
                    {
                        this.Decoder.EndDecoding();
                    }
                    catch ( System.IO.IOException )
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
                catch ( System.IO.IOException )
                {
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
