using System.Diagnostics;
using System.Net.Sockets;

namespace cc.isr.ONC.RPC.Server;

/// <summary>
/// Instances of class <see cref="OncRpcTcpTransport"/> encapsulate TCP/IP-based XDR streams
/// of ONC/RPC servers.
/// </summary>
/// <remarks>
/// Remote Tea authors: Harald Albrecht, Jay Walters.
/// </remarks>
public class OncRpcTcpConnTransport : OncRpcTransportBase
{

    #region " construction and cleanup "

    /// <summary>
    /// Create a new instance of a <see cref="OncRpcTcpConnTransport"/>
    /// which encapsulates TCP/IP-based XDR streams of an ONC/RPC server.
    /// </summary>
    /// <remarks>
    /// This particular server transport handles individual ONC/RPC connections over TCP/IP.
    /// </remarks>
    /// <param name="parent">   Parent server transport which created us. </param>
    /// <param name="socket">   TCP/IP-based socket of new connection. </param>
    public OncRpcTcpConnTransport( OncRpcTcpTransport parent, Socket socket ) : this( parent.Dispatcher, socket,
                                                            parent.RegisteredPrograms, parent.BufferSize,
                                                            parent, parent.TransmitTimeout )
    { }

    /// <summary>
    /// Create a new instance of a <see cref="OncRpcTcpConnTransport"/>
    /// which encapsulates TCP/IP-based XDR streams of an ONC/RPC server.
    /// </summary>
    /// <remarks>
    /// This particular server transport handles individual ONC/RPC connections over TCP/IP. This
    /// constructor is a convenience constructor for those transports handling only a single ONC/RPC
    /// program and version number.
    /// </remarks>
    /// <param name="dispatcher">       Reference to interface of an object capable of dispatching
    ///                                 (handling) ONC/RPC calls. </param>
    /// <param name="socket">           TCP/IP-based socket of new connection. </param>
    /// <param name="program">          Number of ONC/RPC program handled by this server transport. </param>
    /// <param name="version">          Version number of ONC/RPC program handled. </param>
    /// <param name="bufferSize">       Size of buffer used when receiving and sending chunks of XDR
    ///                                 fragments over TCP/IP. The fragments built up to form ONC/RPC
    ///                                 call and reply messages. </param>
    /// <param name="parent">           Parent server transport which created us. </param>
    /// <param name="transmitTimeout">  The transmit timeout when sending calls or receiving replies. </param>
    public OncRpcTcpConnTransport( IOncRpcDispatchable dispatcher, Socket socket, int program, int version, int bufferSize,
                                   OncRpcTcpTransport parent, int transmitTimeout ) : this( dispatcher, socket,
                                                                new OncRpcProgramInfo[] { new OncRpcProgramInfo( program, version ) },
                                                                bufferSize, parent, transmitTimeout )
    { }

    /// <summary>
    /// Create a new instance of a <see cref="OncRpcTcpConnTransport"/>
    /// which encapsulates TCP/IP-based XDR streams of an ONC/RPC server.
    /// </summary>
    /// <remarks>
    /// This particular server transport handles individual ONC/RPC connections over TCP/IP.
    /// </remarks>
    /// <param name="dispatcher">       Reference to interface of an object capable of dispatching
    ///                                 (handling) ONC/RPC calls. </param>
    /// <param name="socket">           TCP/IP-based socket of new connection. </param>
    /// <param name="info">             Array of program and version number tuples of the ONC/RPC
    ///                                 programs and versions handled by this transport. </param>
    /// <param name="bufferSize">       Size of buffer used when receiving and sending chunks of XDR
    ///                                 fragments over TCP/IP. The fragments built up to form ONC/RPC
    ///                                 call and reply messages. </param>
    /// <param name="parent">           Parent server transport which created us. </param>
    /// <param name="transmitTimeout">  The transmit timeout when sending calls or receiving replies. </param>
    public OncRpcTcpConnTransport( IOncRpcDispatchable dispatcher, Socket socket, OncRpcProgramInfo[] info, int bufferSize, OncRpcTcpTransport parent,
                                                                    int transmitTimeout ) : base( dispatcher, 0, OncRpcProtocol.OncRpcTcp, info )
    {
        this._parent = parent;
        this._transmitTimeout = transmitTimeout;

        // Make sure the buffer is large enough and resize system buffers
        // accordingly, if possible.

        if ( bufferSize < OncRpcTransportBase.MinBufferSizeDefault ) bufferSize = OncRpcTransportBase.MinBufferSizeDefault;
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

    /// <summary>
    /// TCP socket used for stream-based communication with ONC/RPC
    /// clients.
    /// </summary>
    private Socket? _socket;

    /// <summary>
    /// Indicates that <see cref="OncRpcTransportBase.BeginEncoding"/> has been called for the
    /// receiving XDR stream, so that it should be closed later using
    /// <see cref="OncRpcTransportBase.EndDecoding"/>.
    /// </summary>
    private bool _pendingDecoding = false;

    /// <summary>
    /// Indicates that <see cref="OncRpcTransportBase.BeginEncoding"/> has been called for the
    /// sending XDR stream, so in face of exceptions we cannot send an
    /// error reply to the client but only drop the connection.
    /// </summary>
    private bool _pendingEncoding = false;

    /// <summary>
    /// Reference to the TCP/IP transport which created us to handle a
    /// new ONC/RPC connection.
    /// </summary>
    private OncRpcTcpTransport? _parent;

    /// <summary>
    /// Timeout during the phase where data is received within calls, or data is sent within replies.
    /// </summary>
    private readonly int _transmitTimeout;

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

            OncRpcTcpTransport? parent = this._parent;
            if ( parent is not null )
            {
                try
                {
                    parent.RemoveTransport( this );
                }
                catch ( Exception ex )
                { exceptions.Add( ex ); }
                finally
                {
                    this._parent = null;
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

    #region " operation methods "

    /// <summary>   Do not call. </summary>
    /// <exception cref="Exception">    because this method must not be called for an individual
    ///                                 TCP/IP-based server transport. </exception>
    public override void Register()
    {
        throw new InvalidOperationException( $"{nameof( OncRpcTcpTransport.Register )} must not be called for an individual TCP/IP-based server transport." );
    }

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
        call.Decode( this.Decoder! );
        if ( this._pendingDecoding )
        {
            this._pendingDecoding = false;
            this.Decoder!.EndDecoding();
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
            this.Decoder!.EndDecoding();
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
    internal override void BeginEncoding( OncRpcCallHandler callInfo, OncRpcServerReplyMessage state )
    {

        // In case decoding has not been properly finished, do it now to
        // free up pending resources, etc.

        if ( this._pendingDecoding )
        {
            this._pendingDecoding = false;
            this.Decoder!.EndDecoding();
        }

        // Now start encoding using the reply message header first...

        this._pendingEncoding = true;
        this.Encoder!.BeginEncoding( callInfo!.PeerEndPoint! );
        state.Encode( this.Encoder! );
    }

    /// <summary>   Finishes encoding the reply to this ONC/RPC call. </summary>
    /// <remarks>
    /// Afterwards you must not use the XDR stream returned by <see cref="Encoder"/>
    /// any longer.
    /// </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    internal override void EndEncoding()
    {
        // Close the case
        this.Encoder!.EndEncoding();
        this._pendingEncoding = false;
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
    /// <param name="reply">    If not (<see langword="null"/>), then this parameter references the reply to
    ///                         be serialized after the reply message header. </param>
    internal override void Reply( OncRpcCallHandler callInfo, OncRpcServerReplyMessage state, IXdrCodec reply )
    {
        this.BeginEncoding( callInfo, state );
        reply?.Encode( this.Encoder! );
        this.EndEncoding();
    }

    internal override void Reply( OncRpcCallHandler callInfo, OncRpcServerReplyMessage state )
    {
        this.BeginEncoding( callInfo, state );
        this.EndEncoding();
    }

    #endregion

    #region " listen implementation "

    /// <summary>   Handles incoming requests, dispatches them and sending back replies. </summary>
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
    /// @ATECoder: 2023-01-23: add cancellation. </para>
    /// </remarks>
    /// <param name="cancelSource"> A cancellation source that allows processing to be canceled. </param>
    protected override void Listen( CancellationTokenSource cancelSource )
    {
        if ( this._socket is null || this.Decoder is null ) { return; }
        OncRpcCallHandler callInfo = new( this );
        for (; ; )
        {

            // break if cancellation is required
            if ( cancelSource.IsCancellationRequested ) { break; }

            // Start decoding the incoming call. This involves remembering
            // from whom we received the call so we can later Sends back the
            // appropriate reply message.

            try
            {
                this._socket.ReceiveTimeout = 0;
                this._pendingDecoding = true;
                this.Decoder.BeginDecoding();
                callInfo.PeerEndPoint = this.Decoder.RemoteEndPoint;
                this._socket.ReceiveTimeout = this._transmitTimeout;
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
                Trace.TraceError( $"Failed dispatching ONC/RPC Call: {ex}" );
                Trace.Flush();

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
                    if ( ex is OncRpcAuthException exception )
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
