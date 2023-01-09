
using System.Net.Sockets;

namespace cc.isr.ONC.RPC.Server;

/// <summary>
/// Instances of class <see cref="OncRpcTcpServerTransport"/> encapsulate TCP/IP-based XDR streams
/// of ONC/RPC servers.
/// </summary>
/// <remarks>
/// This server transport class is responsible for accepting new ONC/RPC connections over TCP/IP. <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public class OncRpcTcpServerTransport : OncRpcServerTransportBase
{

    #region " construction and cleanup "

    /// <summary>
    /// Create a new instance of a <see cref="OncRpcTcpServerTransport"/> which encapsulates TCP/IP-
    /// based XDR streams of an ONC/RPC server.
    /// </summary>
    /// <remarks>
    /// This particular server transport only waits for incoming connection requests and then creates
    /// <see cref="OncRpcTcpConnectionServerTransport"/>
    /// server transports to handle individual connections. This constructor is a convenience
    /// constructor for those transports handling only a single ONC/RPC program and version number.
    /// </remarks>
    /// <exception cref="InvalidOperationException">  because this method must not be called for a listening server transport. </exception>
    /// <param name="dispatcher">   Reference to interface of an object capable of dispatching
    ///                             (handling) ONC/RPC calls. </param>
    /// <param name="port">         Number of port where the server will wait for incoming calls. </param>
    /// <param name="program">      Number of ONC/RPC program handled by this server transport. </param>
    /// <param name="version">      Version number of ONC/RPC program handled. </param>
    /// <param name="bufferSize">   Size of buffer used when receiving and sending chunks of XDR
    ///                             fragments over TCP/IP. The fragments built up to form ONC/RPC
    ///                             call and reply messages. </param>
    public OncRpcTcpServerTransport( IOncRpcDispatchable dispatcher, int port, int program, int version, int bufferSize ) : this( dispatcher, port,
        new OncRpcServerTransportRegistrationInfo[] { new OncRpcServerTransportRegistrationInfo( program, version ) }, bufferSize )
    {
        this._openTransports = new TransportList( this );
    }

    /// <summary>
    /// Create a new instance of a <see cref="OncRpcTcpServerTransport"/> which encapsulates 
    /// TCP/IP-based XDR streams of an ONC/RPC server.
    /// </summary>
    /// <remarks>
    /// This particular server transport only waits for incoming connection requests and then creates
    /// <see cref="OncRpcTcpConnectionServerTransport"/>
    /// server transports to handle individual connections.
    /// </remarks>
    /// <exception cref="InvalidOperationException">  because this method must not be called for a listening server transport. </exception>
    /// <param name="dispatcher">   Reference to interface of an object capable of dispatching
    ///                             (handling) ONC/RPC calls. </param>
    /// <param name="port">         Number of port where the server will wait for incoming calls. </param>
    /// <param name="info">         Array of program and version number tuples of the ONC/RPC
    ///                             programs and versions handled by this transport. </param>
    /// <param name="bufferSize">   Size of buffer used when receiving and sending chunks of XDR
    ///                             fragments over TCP/IP. The fragments built up to form ONC/RPC
    ///                             call and reply messages. </param>
    public OncRpcTcpServerTransport( IOncRpcDispatchable dispatcher, int port,
        OncRpcServerTransportRegistrationInfo[] info, int bufferSize ) : this( dispatcher, null, port, info, bufferSize )
    {
        this._openTransports = new TransportList( this );
    }

    /// <summary>
    /// Create a new instance of a <see cref="OncRpcTcpServerTransport"/> which encapsulates TCP/IP-
    /// based XDR streams of an ONC/RPC server.
    /// </summary>
    /// <remarks>
    /// This particular server transport only waits for incoming connection requests and then creates
    /// <see cref="OncRpcTcpConnectionServerTransport"/>
    /// server transports to handle individual connections.
    /// </remarks>
    /// <exception cref="InvalidOperationException">  because this method must not be called for a listening server transport. </exception>
    /// <param name="dispatcher">   Reference to interface of an object capable of dispatching
    ///                             (handling) ONC/RPC calls. </param>
    /// <param name="bindAddr">     The local Internet Address the server will bind to. </param>
    /// <param name="port">         Number of port where the server will wait for incoming calls. </param>
    /// <param name="info">         Array of program and version number tuples of the ONC/RPC
    ///                             programs and versions handled by this transport. </param>
    /// <param name="bufferSize">   Size of buffer used when receiving and sending chunks of XDR
    ///                             fragments over TCP/IP. The fragments built up to form ONC/RPC
    ///                             call and reply messages. </param>
    public OncRpcTcpServerTransport( IOncRpcDispatchable dispatcher, IPAddress bindAddr, int port,
        OncRpcServerTransportRegistrationInfo[] info, int bufferSize ) : base( dispatcher, port, OncRpcProtocols.OncRpcTcp, info )
    {
        this._openTransports = new TransportList( this );

        // Make sure the buffer is large enough and resize system buffers
        // accordingly, if possible.

        if ( bufferSize < OncRpcServerTransportBase.DefaultMinBufferSize ) bufferSize = OncRpcServerTransportBase.DefaultMinBufferSize;
        this._bufferSize = bufferSize;
        this._socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
        bindAddr ??= IPAddress.Any;
        IPEndPoint localEP = new( bindAddr, port );
        this._socket.Bind( localEP );
        if ( port == 0 )
            this.Port = (( IPEndPoint ) this._socket.LocalEndPoint).Port;
        this._socket.Listen( 0 );
        this.CharacterEncoding = XdrTcpEncodingStream.DefaultEncoding;
    }

    /// <summary>   Close the server transport and free any resources associated with it. </summary>
    /// <remarks>
    /// Note that the server transport is <b>not deregistered</b>. You'll
    /// have to do it manually if you need to do so. The reason for this behavior is, that the
    /// portmapper removes all entries regardless of the protocol (TCP/IP or UDP/IP) for a given
    /// ONC/RPC program number and version. <para>
    /// Calling this method on a <see cref="OncRpcTcpServerTransport"/>
    /// results in the listening TCP network socket immediately being closed. In addition, all server
    /// transports handling the individual TCP/IP connections will also be closed. The handler
    /// threads will therefore either terminate directly or when they try to sent back replies.</para>
    /// </remarks>
    public override void Close()
    {
        if ( this._socket is not null )
        {

            // Since there is a non-zero chance of getting race conditions,
            // we now first set the socket instance member to null, before
            // we close the corresponding socket. This avoids null-pointer
            // exceptions in the method which waits for connections: it is
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

        // close the base class codecs (should be none)
        base.Close();

        // Now close all per-connection transports currently open...

        lock ( this._openTransports )
            while ( this._openTransports.Size() > 0 )
            {
                OncRpcTcpConnectionServerTransport transport = ( OncRpcTcpConnectionServerTransport ) this._openTransports.RemoveFirst();
                transport.Close();
            }
    }

    /// <summary>
    /// TCP socket used for stream-based communication with ONC/RPC
    /// clients.
    /// </summary>
    private Socket _socket;

    /// <summary>Size of send/receive buffers to use when encoding/decoding XDR data.</summary>
    private readonly int _bufferSize;

    /// <summary>Collection containing currently open transports.</summary>
    private readonly TransportList _openTransports;

    #endregion

    #region " Configuration Methods "

    /// <summary>
    /// Gets or sets or set the timeout during the phase where data is received within calls, or data
    /// is sent within replies.
    /// </summary>
    /// <remarks>
    /// If the flow of data when sending calls or receiving replies blocks longer than the given
    /// timeout, an exception is thrown. The timeout must be greater than 0.
    /// </remarks>
    /// <value> The transmission timeout in milliseconds. </value>
    public int TransmissionTimeout { get; set; } = OncRpcServerTransportBase.DefaultTransmissionTimeout;

    #endregion

    #region " Operation Methods "

    /// <summary>
    /// Removes a TCP/IP server transport from the list of currently open transports.
    /// </summary>
    /// <param name="transport">    Server transport to remove from the list of currently open
    ///                             transports for this listening transport. </param>
    public virtual void RemoveTransport( OncRpcTcpConnectionServerTransport transport )
    {
        lock ( this._openTransports )
            _ = this._openTransports.Remove( transport );
    }

    /// <summary>   Do not call. </summary>
    /// <exception cref="InvalidOperationException">  because this method must not be called for a listening server transport. </exception>
    /// <param name="call"> The call. </param>
    internal override void RetrieveCall( IXdrCodec call )
    {
        throw new Exception( $"{nameof( OncRpcTcpServerTransport.RetrieveCall )} is abstract and cannot be called." );
    }

    /// <summary>   Do not call. </summary>
    /// <exception cref="InvalidOperationException">  because this method must not be called for a listening server transport. </exception>
    internal override void EndDecoding()
    {
        throw new InvalidOperationException( $"{nameof( OncRpcTcpServerTransport.EndDecoding )} is abstract and cannot be called." );
    }

    /// <summary>   Do not call. </summary>
    /// <exception cref="InvalidOperationException">  because this method must not be called for a listening server transport. </exception>
    /// <param name="callInfo"> Information about ONC/RPC call for which we are about to send back
    ///                         the reply. </param>
    /// <param name="state">    ONC/RPC reply header indicating success or failure. </param>
    internal override void BeginEncoding( OncRpcCallInformation callInfo, OncRpcServerReplyMessage state )
    {
        throw new InvalidOperationException( $"{nameof( OncRpcTcpServerTransport.BeginEncoding )} is abstract and cannot be called." );
    }

    /// <summary>   Do not call. </summary>
    /// <exception cref="InvalidOperationException">  because this method must not be called for a listening server transport. </exception>
    internal override void EndEncoding()
    {
        throw new InvalidOperationException( $"{nameof( OncRpcTcpServerTransport.EndEncoding )} is abstract and cannot be called." );
    }

    /// <summary>   Do not call. </summary>
    /// <exception cref="InvalidOperationException">  because this method must not be called for a listening server transport. </exception>
    /// <param name="callInfo"> information about the original call, which are necessary to send back
    ///                         the reply to the appropriate caller. </param>
    /// <param name="state">    ONC/RPC reply message header indicating success or failure and
    ///                         containing associated state information. </param>
    /// <param name="reply">    If not <see langword="null"/>, then this parameter references the reply to
    ///                         be serialized after the reply message header. </param>
    internal override void Reply( OncRpcCallInformation callInfo, OncRpcServerReplyMessage state, IXdrCodec reply )
    {
        throw new Exception( $"{nameof( OncRpcTcpServerTransport.Reply )} is abstract and cannot be called." );
    }

    /// <summary>
    /// Creates a new thread and uses this thread to listen to incoming ONC/RPC requests, then
    /// dispatches them and finally sends back the appropriate reply messages.
    /// </summary>
    /// <remarks>
    /// Control in the calling thread immediately returns after the handler thread has been created. <para>
    /// For every incoming TCP/IP connection a handler thread is created
    /// to handle ONC/RPC calls on this particular connection.</para>
    /// </remarks>
    public override void Listen()
    {

        // Create a new (daemon) thread which will handle incoming connection
        // requests.

        TransportHelper t = new( this );
        Thread listenThread = new( new ThreadStart( t.Run ) ) {
            Name = "TCP server transport listener thread"
        };

        // Now wait for (new) connection requests to come in.


        // Let the newly created transport object handle this
        // connection. Note that it will create its own
        // thread for handling.


        // We are just ignoring most of the IOExceptions as
        // they might be thrown, for instance, if a client
        // attempts a connection and resets it before it is
        // pulled off by accept(). If the socket has been
        // gone away after an IOException this means that the
        // transport has been closed, so we end this thread
        // gracefully.

        listenThread.Start();
    }


    #endregion

    #region " Thread-Aware Listen Implementation "

    private sealed class TransportHelper
    {
        /// <summary>   Constructor. </summary>
        /// <param name="enclosingTransport">   The enclosing transport. </param>
        public TransportHelper( OncRpcTcpServerTransport enclosingTransport )
        {
            this._enclosing = enclosingTransport;
        }

        /// <summary>   Runs this object. </summary>
        public void Run()
        {
            for (; ; )
                try
                {
                    Socket myServerSocket = this._enclosing._socket;
                    if ( myServerSocket == null )
                        break;
                    Socket newSocket = myServerSocket.Accept();
                    OncRpcTcpConnectionServerTransport transport = new( this._enclosing.Dispatcher, newSocket,
                        this._enclosing.TransportRegistrationInfo, this._enclosing._bufferSize, this._enclosing, this._enclosing.TransmissionTimeout );
                    lock ( this._enclosing._openTransports )
                        this._enclosing._openTransports.Add( transport );
                    transport.Listen();
                }
                catch ( OncRpcException )
                {
                }
                catch ( SocketException )
                {
                    // If the socket has been closed and set to null, don't bother
                    // notifying anybody because we're shutting down
                    if ( this._enclosing._socket == null )
                        break;
                }
        }

        private readonly OncRpcTcpServerTransport _enclosing;
    }

    #endregion

    #region " Transport List "

    private class TransportList
    {

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "CodeQuality", "IDE0052:Remove unread private members", Justification = "<Pending>" )]
        private readonly OncRpcTcpServerTransport _enclosing;

        /// <summary>   Create a new instance of a list of open transports. </summary>
        /// <param name="enclosing">   The enclosing transport. </param>
        public TransportList( OncRpcTcpServerTransport enclosing )
        {
            this._enclosing = enclosing;
            this._head = new Node( this, null );

            // Link header node with itself, so it is its own successor
            // and predecessor. Using a header node excuses us from checking
            // for the special cases of first and last node (or both at
            // the same time).

            this._head.Next = this._head;
            this._head.Prev = this._head;
        }

        /// <summary>   Add new transport to list of open transports. </summary>
        /// <remarks>
        /// The new transport is always added immediately after the head of the linked list.
        /// </remarks>
        /// <param name="o">    The o to remove. </param>
        public virtual void Add( object o )
        {
            Node node = new( this, o ) {
                Next = this._head.Next
            };
            this._head.Next = node;
            node.Prev = this._head;
            node.Next.Prev = node;
            ++this._size;
        }

        /// <summary>   Remove given transport from list of open transports. </summary>
        /// <param name="o">    The o to remove. </param>
        /// <returns>   True if it succeeds, false if it fails. </returns>
        public virtual bool Remove( object o )
        {
            Node node = this._head.Next;
            while ( node != this._head )
            {
                if ( node.Item == o )
                {
                    node.Prev.Next = node.Next;
                    node.Next.Prev = node.Prev;
                    --this._size;
                    return true;
                }
                node = node.Next;
            }
            return false;
        }

        /// <summary>   Removes and returns the first open transport from list. </summary>
        /// <exception cref="ArgumentOutOfRangeException">  Thrown when one or more arguments are outside
        ///                                                 the required range. </exception>
        /// <returns>   An object. </returns>
        public virtual object RemoveFirst()
        {

            // Do not remove the header node.

            if ( this._size == 0 )
                throw new ArgumentOutOfRangeException();
            Node node = this._head.Next;
            this._head.Next = node.Next;
            node.Next.Prev = this._head;
            --this._size;
            return node.Item;
        }

        /// <summary>   Returns the number of (open) transports in this list. </summary>
        /// <returns>   the number of (open) transports. </returns>
        public virtual int Size()
        {
            return this._size;
        }

        /// <summary>
        /// Head node for list of open transports which does not represent
        /// an open transport but instead excuses us of dealing with all
        /// the special cases of real nodes at the begin or end of the list.
        /// </summary>
        private readonly Node _head;

        /// <summary>
        /// Number of (real) open transports currently registered in this
        /// list.
        /// </summary>
        private int _size = 0;

        private class Node
        {
            /// <summary>
            /// Create a new instance of a node object and let it reference an open transport.
            /// </summary>
            /// <remarks>
            /// The creator of this object is then responsible for adding this node to the circular list itself.
            /// </remarks>
            /// <param name="enclosing">   The enclosing transport linked list. </param>
            /// <param name="item">        The item/object placed at this position in the list. </param>
            public Node( TransportList enclosing, object item )
            {
                this._enclosing = enclosing;
                this.Item = item;
            }

            /// <summary>
            /// Next item node (in other words: next open transport)
            /// in the list. This will never be <see langword="null"/> for the first item, but instead reference
            /// the last item. Thus, the list is circular.
            /// </summary>
            internal Node Next { get; set; }

            /// <summary>
            /// Previous item node (in other words: previous open transport)
            /// in the list. This will never be <see langword="null"/> for the last item, but instead reference
            /// the first item. Thus, the list is circular.
            /// </summary>
            internal Node Prev { get; set; }

            /// <summary>
            /// The item/object placed at this position in the list. This currently always references an open
            /// transport.
            /// </summary>
            internal object Item { get; set; }

            /// <summary>   (Immutable) the enclosing <see cref="TransportList"/>. </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage( "CodeQuality", "IDE0052:Remove unread private members", Justification = "<Pending>" )]
            private readonly TransportList _enclosing;
        }

    }

    #endregion

}
