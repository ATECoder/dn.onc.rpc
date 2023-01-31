using System.Net.Sockets;

namespace cc.isr.ONC.RPC.Server;

/// <summary>
/// Instances of class <see cref="OncRpcTcpTransport"/> encapsulate TCP/IP-based XDR streams
/// of ONC/RPC servers.
/// </summary>
/// <remarks>
/// This server transport class is responsible for accepting new ONC/RPC connections over TCP/IP. <para>
/// 
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public class OncRpcTcpTransport : OncRpcTransportBase
{

    #region " construction and cleanup "

    /// <summary>
    /// Create a new instance of a <see cref="OncRpcTcpTransport"/> which encapsulates TCP/IP-
    /// based XDR streams of an ONC/RPC server.
    /// </summary>
    /// <remarks>
    /// This particular server transport only waits for incoming connection requests and then creates
    /// <see cref="OncRpcTcpConnTransport"/>
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
    public OncRpcTcpTransport( IOncRpcDispatchable dispatcher, int port, int program, int version, int bufferSize )
                                                                : this( dispatcher, port,
                                                                        new OncRpcProgramInfo[] { new OncRpcProgramInfo( program, version ) },
                                                                        bufferSize )
    {
        this._openTransports = new TransportList();
    }

    /// <summary>
    /// Create a new instance of a <see cref="OncRpcTcpTransport"/> which encapsulates 
    /// TCP/IP-based XDR streams of an ONC/RPC server.
    /// </summary>
    /// <remarks>
    /// This particular server transport only waits for incoming connection requests and then creates
    /// <see cref="OncRpcTcpConnTransport"/>
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
    public OncRpcTcpTransport( IOncRpcDispatchable dispatcher, int port, OncRpcProgramInfo[] info, int bufferSize )
                                                                : this( dispatcher, IPAddress.Any, port, info, bufferSize )
    {
        this._openTransports = new TransportList();
    }

    /// <summary>
    /// Create a new instance of a <see cref="OncRpcTcpTransport"/> which encapsulates TCP/IP-
    /// based XDR streams of an ONC/RPC server.
    /// </summary>
    /// <remarks>
    /// This particular server transport only waits for incoming connection requests and then creates
    /// <see cref="OncRpcTcpConnTransport"/>
    /// server transports to handle individual connections.
    /// </remarks>
    /// <exception cref="InvalidOperationException">  because this method must not be called for a listening server transport. </exception>
    /// <param name="dispatcher">   Reference to interface of an object capable of dispatching
    ///                             (handling) ONC/RPC calls. </param>
    /// <param name="bindAddr">     The local Internet Address the server will bind to or <see cref="IPAddress.Any"/>. </param>
    /// <param name="port">         Number of port where the server will wait for incoming calls. </param>
    /// <param name="info">         Array of program and version number tuples of the ONC/RPC
    ///                             programs and versions handled by this transport. </param>
    /// <param name="bufferSize">   Size of buffer used when receiving and sending chunks of XDR
    ///                             fragments over TCP/IP. The fragments built up to form ONC/RPC
    ///                             call and reply messages. </param>
    public OncRpcTcpTransport( IOncRpcDispatchable dispatcher, IPAddress bindAddr, int port, OncRpcProgramInfo[] info, int bufferSize )
                                                                : base( dispatcher, port, OncRpcProtocol.OncRpcTcp, info )
    {
        this._openTransports = new TransportList();

        // Make sure the buffer is large enough and resize system buffers
        // accordingly, if possible.

        if ( bufferSize < OncRpcTransportBase.MinBufferSizeDefault ) bufferSize = OncRpcTransportBase.MinBufferSizeDefault;
        this.BufferSize = bufferSize;
        this._socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
        bindAddr ??= IPAddress.Any;
        IPEndPoint localEP = new( bindAddr, port );
        this._socket.Bind( localEP );
        if ( port == 0 )
            this.Port = (( IPEndPoint ) this._socket.LocalEndPoint).Port;
        this._socket.Listen( 0 );
        this.CharacterEncoding = XdrTcpEncodingStream.EncodingDefault;
    }

    /// <summary>
    /// TCP socket used for stream-based communication with ONC/RPC
    /// clients.
    /// </summary>
    private Socket? _socket;

    /// <summary>
    /// Gets or sets the size of send/receive buffers to use when encoding/decoding XDR data.
    /// </summary>
    /// <value> The size of the buffer. </value>
    internal int BufferSize { get; private set; }

    /// <summary>Collection containing currently open transports.</summary>
    private readonly TransportList _openTransports;

    /// <summary>
    /// Releases unmanaged, large objects and (optionally) managed resources used by this class.
    /// Closes the server transport and frees any resources associated with it.
    /// </summary>
    /// <remarks>
    /// Note that the server transport is <b>not deregistered</b>. You'll have to do it manually if
    /// you need to do so. The reason for this behavior is, that the portmapper removes all entries
    /// regardless of the protocol (TCP/IP or UDP/IP) for a given ONC/RPC program number and version.
    /// <para>
    /// 
    /// Calling this method on a <see cref="OncRpcTcpTransport"/> results in the listening TCP
    /// network socket immediately being closed. In addition, all server transports handling the
    /// individual TCP/IP connections will also be closed. The handler tasks will therefore either
    /// terminate directly or when they try to sent back replies.</para>
    /// </remarks>
    /// <exception cref="AggregateException">   Thrown when an Aggregate error condition occurs. </exception>
    /// <param name="disposing">    True to release large objects and managed and unmanaged resources;
    ///                             false to release only unmanaged resources and large objects. </param>
    protected override void Dispose( bool disposing )
    {
        List<Exception> exceptions = new();
        if ( disposing )
        {
            // dispose managed state (managed objects)

            // if a NetworkStream wasn't created, the Socket might
            // still be there and needs to be closed. In the case in which
            // we are bound to a local IPEndPoint this will remove the
            // binding and free up the IPEndPoint for later uses.

            Socket? socket = this._socket;
            if ( socket is not null )
            {
                try
                {
                    if ( socket.Connected )
                        socket.Shutdown( SocketShutdown.Both );
                }
                catch ( Exception ex )
                { exceptions.Add( ex ); }
                finally
                {
                    socket.Close();
                    this._socket = null;
                }
            }

            // Now close all per-connection transports currently open...

            lock ( this._openTransports )
                while ( this._openTransports.Size() > 0 )
                {
                    OncRpcTcpConnTransport? transport = this._openTransports.RemoveFirst();
                    if ( transport is not null )
                    {
                        try
                        {
                            transport?.Close();
                        }
                        catch ( Exception ex )
                        { exceptions.Add( ex ); }
                        finally
                        {
                        }
                    }
                }
        }

        // free unmanaged resources and override finalizer

        // set large fields to null

        // call base dispose( bool ).

        try
        {
            base.Dispose( disposing );
        }
        catch ( Exception ex )
        { exceptions.Add( ex ); }
        finally
        {
        }

        if ( exceptions.Any() )
        {
            AggregateException aggregateException = new( exceptions );
            throw aggregateException;
        }
    }

    #endregion

    #region " members "

    /// <summary>
    /// Gets or sets or set the timeout during the phase where data is received within calls, or data
    /// is sent within replies.
    /// </summary>
    /// <remarks>
    /// If the flow of data when sending calls or receiving replies blocks longer than the given
    /// timeout, an exception is thrown. The timeout must be greater than 0.
    /// </remarks>
    /// <value> The timeout when sending calls or receiving replies in milliseconds. </value>
    public int TransmitTimeout { get; set; } = OncRpcTransportBase.TransmitTimeoutDefault;

    #endregion

    #region " actions "

    /// <summary>
    /// Removes a TCP/IP server transport from the list of currently open transports.
    /// </summary>
    /// <param name="transport">    Server transport to remove from the list of currently open
    ///                             transports for this listening transport. </param>
    public virtual void RemoveTransport( OncRpcTcpConnTransport transport )
    {
        lock ( this._openTransports )
            _ = this._openTransports.Remove( transport );
    }

    /// <summary>   Do not call. </summary>
    /// <exception cref="InvalidOperationException">  because this method must not be called for a listening server transport. </exception>
    /// <param name="call"> The call. </param>
    internal override void RetrieveCall( IXdrCodec call )
    {
        throw new InvalidOperationException( $"{nameof( OncRpcTcpTransport.RetrieveCall )} is abstract and cannot be called." );
    }

    /// <summary>   Do not call. </summary>
    /// <exception cref="InvalidOperationException">  because this method must not be called for a listening server transport. </exception>
    internal override void EndDecoding()
    {
        throw new InvalidOperationException( $"{nameof( OncRpcTcpTransport.EndDecoding )} is abstract and cannot be called." );
    }

    /// <summary>   Do not call. </summary>
    /// <exception cref="InvalidOperationException">  because this method must not be called for a listening server transport. </exception>
    /// <param name="callInfo"> Information about ONC/RPC call for which we are about to send back
    ///                         the reply. </param>
    /// <param name="state">    ONC/RPC reply header indicating success or failure. </param>
    internal override void BeginEncoding( OncRpcCallHandler callInfo, OncRpcServerReplyMessage state )
    {
        throw new InvalidOperationException( $"{nameof( OncRpcTcpTransport.BeginEncoding )} is abstract and cannot be called." );
    }

    /// <summary>   Do not call. </summary>
    /// <exception cref="InvalidOperationException">  because this method must not be called for a listening server transport. </exception>
    internal override void EndEncoding()
    {
        throw new InvalidOperationException( $"{nameof( OncRpcTcpTransport.EndEncoding )} is abstract and cannot be called." );
    }

    /// <summary>   Do not call. </summary>
    /// <exception cref="InvalidOperationException">  because this method must not be called for a listening server transport. </exception>
    /// <param name="callInfo"> information about the original call, which are necessary to send back
    ///                         the reply to the appropriate caller. </param>
    /// <param name="state">    ONC/RPC reply message header indicating success or failure and
    ///                         containing associated state information. </param>
    /// <param name="reply">    If not (<see langword="null"/>), then this parameter references the reply to
    ///                         be serialized after the reply message header. </param>
    internal override void Reply( OncRpcCallHandler callInfo, OncRpcServerReplyMessage state, IXdrCodec reply )
    {
        throw new InvalidOperationException( $"{nameof( OncRpcTcpTransport.Reply )} is abstract and cannot be called." );
    }

    /// <summary>   Do not call. </summary>
    /// <exception cref="InvalidOperationException">  because this method must not be called for a listening server transport. </exception>
    /// <param name="callInfo"> <see cref="OncRpcCallHandler"/> about the original call, which are
    ///                         necessary to Sends back the reply to the appropriate caller. </param>
    /// <param name="state">    ONC/RPC reply message header indicating success or failure and
    ///                         containing associated state information. </param>
    internal override void Reply( OncRpcCallHandler callInfo, OncRpcServerReplyMessage state )
    {
        throw new InvalidOperationException( $"{nameof( OncRpcTcpTransport.Reply )} is abstract and cannot be called." );
    }

    #endregion

    #region " listen implementation "

    /// <summary>   Get the server to start listening on the transports. </summary>
    /// <remarks>
    /// For every incoming TCP/IP connection a handler task is created to handle ONC/RPC calls on
    /// this particular connection. <para>
    /// 
    /// Now wait for (new) connection requests to come in. </para><para>
    /// 
    /// Let the newly created transport object handle this connection. Note that it will create its
    /// own task for handling. </para><para>
    /// 
    /// We are just ignoring most of the IOExceptions as they might be thrown, for instance, if a
    /// client attempts a connection and resets it before it is pulled off by accept(). If the socket
    /// has been gone away after an IOException this means that the transport has been closed, so we
    /// end this task gracefully. </para><para>
    /// 
    /// @atecode 2023-01-23: add cancellation. </para>
    /// </remarks>
    /// <param name="cancelSource"> The cancel source. </param>
    protected override void DoListen( CancellationTokenSource cancelSource )
    {
        for (; ; )
            try
            {
                // break if cancellation is required
                if ( cancelSource.Token.IsCancellationRequested ) { break; }

                Socket myServerSocket = this._socket!;
                if ( myServerSocket == null )
                    break;

                Socket newSocket = myServerSocket.Accept();
                OncRpcTcpConnTransport transport = new( this, newSocket );


                lock ( this._openTransports )
                    this._openTransports.Add( transport );
                _ = transport.ListenAsync( cancelSource );
            }
            catch ( OncRpcException )
            {
            }
            catch ( SocketException )
            {
                // If the socket has been closed and set to null, don't bother
                // notifying anybody because we're shutting down
                if ( this._socket == null )
                    break;
            }
    }

    #endregion

    #region " transport linked list "

    private class TransportList
    {

        /// <summary>   Create a new instance of a list of open transports. </summary>
        public TransportList()
        {
            this._head = new Node( null );

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
        /// <param name="item"> The item to add. </param>
        public virtual void Add( OncRpcTcpConnTransport item )
        {
            Node node = new( item ) {
                Next = this._head.Next
            };
            this._head.Next = node;
            node.Prev = this._head;
            node.Next!.Prev = node;
            ++this._size;
        }

        /// <summary>   Remove given transport from list of open transports. </summary>
        /// <param name="item">    The item to remove. </param>
        /// <returns>   True if it succeeds, false if it fails. </returns>
        public virtual bool Remove( OncRpcTcpConnTransport item )
        {
            Node node = this._head.Next!;
            while ( node != this._head )
            {
                if ( node!.Item == item )
                {
                    node.Prev!.Next = node.Next;
                    node.Next!.Prev = node.Prev;
                    --this._size;
                    return true;
                }
                node = node.Next!;
            }
            return false;
        }

        /// <summary>   Removes and returns the first open transport from list. </summary>
        /// <exception cref="ArgumentOutOfRangeException">  Thrown when one or more arguments are outside
        ///                                                 the required range. </exception>
        /// <returns>   An object. </returns>
        public virtual OncRpcTcpConnTransport? RemoveFirst()
        {

            // Do not remove the header node.

            if ( this._size == 0 )
                throw new ArgumentOutOfRangeException();
            Node node = this._head.Next!;
            this._head.Next = node.Next;
            node.Next!.Prev = this._head;
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
            /// <param name="item">        The item/object placed at this position in the list. </param>
            public Node( OncRpcTcpConnTransport? item )
            {
                this.Item = item;
            }

            /// <summary>
            /// Next item node (in other words: next open transport)
            /// in the list. This will never be (<see langword="null"/>) for the first item, but instead reference
            /// the last item. Thus, the list is circular.
            /// </summary>
            internal Node? Next { get; set; }

            /// <summary>
            /// Previous item node (in other words: previous open transport)
            /// in the list. This will never be (<see langword="null"/>) for the last item, but instead reference
            /// the first item. Thus, the list is circular.
            /// </summary>
            internal Node? Prev { get; set; }

            /// <summary>
            /// The item/object placed at this position in the list. This currently always references an open
            /// transport.
            /// </summary>
            internal OncRpcTcpConnTransport? Item { get; set; }

        }

    }

    #endregion

}
