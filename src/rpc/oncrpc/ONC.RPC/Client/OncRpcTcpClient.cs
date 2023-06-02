using System.Net.Sockets;

namespace cc.isr.ONC.RPC.Client;

/// <summary>
/// ONC/RPC client which communicates with ONC/RPC servers over the network using the stream-
/// oriented protocol TCP/IP.
/// </summary>
/// <remarks> <para>
///  
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public class OncRpcTcpClient : OncRpcClientBase
{

    /// <summary>   Gets or sets the default timeout for sending calls or receiving replies. </summary>
    /// <remarks>
    /// This timeout interval is used to set the
    /// <see cref="System.Net.Sockets.Socket"/> send and receive timeouts
    /// during TCP RPC calls.
    /// </remarks>
    /// <value> The transmit timeout default. </value>
    public static int TransmitTimeoutDefault { get; set; } = 1000;

    /// <summary>   Gets or sets the default timeout for TCP I/O calls. </summary>
    /// <remarks>
    /// This timeout interval is used to set the timeouts members of the RPC server using the <see cref="IXdrCodec"/>
    /// payloads that are transmitted as part of the RPC calls.
    /// </remarks>
    /// <value> The i/o timeout default. </value>
    public static int IOTimeoutDefault { get; set; } = 3000;

    /// <summary>   Gets or sets the default timeout for connection to a TCP Socket. </summary>
    /// <value> The connect timeout default. </value>
    public static int ConnectTimeoutDefault { get; set; } = 2000;



    #region " construction and cleanup "

    /// <summary>
    /// Constructs a new server <see cref="OncRpcTcpClient"/> object, which connects to the ONC/RPC server at
    /// <paramref name="host"/> for calling remote procedures of the given { <paramref name="program"/>, <paramref name="version"/> }.
    /// </summary>
    /// <remarks>
    /// Note that the construction of an server <see cref="OncRpcTcpClient"/>
    /// object will result in communication with the portmap process at
    /// <paramref name="host"/> if <paramref name="port"/> is <c>0</c>.
    /// The <see cref="OncRpcClientBase.TransmitTimeout"/> and <see cref="OncRpcClientBase.IOTimeout"/>
    /// must be set at the construction of this class.
    /// </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="host">         The host where the ONC/RPC server resides. </param>
    /// <param name="program">      Program number of the ONC/RPC server to call. </param>
    /// <param name="version">      Program version number. </param>
    /// <param name="port">         The port number where the ONC/RPC server can be contacted. If 
    ///                             <c>0</c>, then the <see cref="OncRpcUdpClient"/> object will ask
    ///                             the portmapper at <paramref name="host"/> for the port number. </param>
    /// <param name="bufferSize">   Size of receive and send buffers. In contrast to UDP-based
    ///                             ONC/RPC clients, messages larger than the specified buffer size
    ///                             can still be sent and received. The buffer is only necessary to
    ///                             handle the messages and the underlaying streams will break up
    ///                             long messages automatically into suitable pieces. Specifying zero
    ///                             will select the <see cref="OncRpcClientBase.BufferSizeDefault"/> (currently
    ///                             8192 bytes). </param>
    /// <param name="connectTimeout">      Maximum timeout in milliseconds when connecting to the ONC/RPC
    ///                             server. If negative, a default implementation-specific timeout
    ///                             setting will apply. <i>Note that this timeout only applies to the
    ///                             connection phase, but <b>not</b> to later communication.</i> </param>
    public OncRpcTcpClient( IPAddress host, int program, int version, int port,
                                            int bufferSize, int connectTimeout ) : base( host, program, version, port, OncRpcProtocol.OncRpcTcp )
    {

        // Constructs the inherited part of our object. This will also try to
        // lookup the port of the desired ONC/RPC server, if no port number
        // was specified (port = 0).

        // Let the host operating system choose which port (and network
        // interface) to use. Then set the buffer sizes for sending and
        // receiving UDP datagrams. Finally set the destination of packets.
        if ( bufferSize == 0 ) bufferSize = BufferSizeDefault;
        // default setting
        if ( bufferSize < MinBufferSizeDefault ) bufferSize = MinBufferSizeDefault;

        // Note that we use this.port at this time, because the superclass
        // might have resolved the port number in case the caller specified
        // simply 0 as the port number.

        // Construct the socket and set the timeouts to the initial connect timeout

        this._socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp ) {
            SendTimeout = connectTimeout,
            ReceiveTimeout = connectTimeout,
            NoDelay = true
        };
        this._socket.SendBufferSize = Math.Min( this._socket.SendBufferSize, bufferSize );
        this._socket.ReceiveBufferSize = Math.Min( this._socket.ReceiveBufferSize, bufferSize );

        // connect

        IPEndPoint endPoint = new( host, this.Port );
        this._socket.Connect( endPoint );

        // Create the necessary encoding and decoding streams, so we can
        // communicate at all.
        this.Encoder = new XdrTcpEncodingStream( this._socket, bufferSize );
        this.Decoder = new XdrTcpDecodingStream( this._socket, bufferSize );
        this.CharacterEncoding = EncodingDefault;

        // set the default timeouts.

        this.IOTimeout = OncRpcTcpClient.IOTimeoutDefault;
        this.TransmitTimeout = OncRpcTcpClient.TransmitTimeoutDefault;
    }

    /// <summary>
    /// TCP socket used for stream-oriented communication with an ONC/RPC
    /// server.
    /// </summary>
    private Socket? _socket;

    /// <summary>
    /// Releases unmanaged, large objects and (optionally) managed resources used by this class.
    /// </summary>
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
    /// Gets or sets or set the XDR encoding stream used for sending requests via TCP/IP to an
    /// ONC/RPC server.
    /// </summary>
    /// <value> The sending XDR encoding stream. </value>
    internal XdrTcpEncodingStream? Encoder { get; private set; }

    /// <summary>
    /// Gets or sets or set the XDR decoding stream used when receiving replies via TCP/IP from an
    /// ONC/RPC server.
    /// </summary>
    /// <value> The receiving XDR decoding stream. </value>
    internal XdrTcpDecodingStream? Decoder { get; private set; }

    /// <summary>
    /// Gets or sets the encoding to use when serializing strings.
    /// </summary>
    /// <value> The character encoding. </value>
    public override Encoding CharacterEncoding
    {
        get => base.CharacterEncoding;
        set {
            base.CharacterEncoding = value;
            if ( this.Decoder is not null ) this.Decoder.CharacterEncoding = value;
            if ( this.Encoder is not null ) this.Encoder.CharacterEncoding = value;
        }
    }

    #endregion

    #region " actions "

    private readonly object _lock = new object();

    /// <summary>   Calls a remote procedure on an ONC/RPC server. </summary>
    /// <remarks>
    /// Note!: <para>
    /// While this code was set for using the <see cref="OncRpcClientBase.IOTimeout"/>
    /// with a value of zero as a flag for batch operation, the same timeout is used for 
    /// setting the XDR request I/O timeout. In order to maintain the API of the 
    /// <see cref="Call(int, int, IXdrCodec, IXdrCodec)"/> method intact, a batch member was 
    ///                                                    
    /// </para>
    /// Please note that while this method supports call batching by setting the communication
    /// timeout to zero (<see cref="Timeout"/> to <c>0</c>) you should better use
    /// <see cref="BatchCall(int, IXdrCodec, bool)"/>
    /// as it provides better control over when the batch should be flushed to the server.
    /// </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="procedureNumber">  Procedure number of the procedure to call. </param>
    /// <param name="versionNumber">    Protocol version number. </param>
    /// <param name="requestCodec">     The XDR codec that is sent to the procedure call. </param>
    /// <param name="replyCodec">       The XDR codec that receives the result of the procedure call. </param>
    public override void Call( int procedureNumber, int versionNumber, IXdrCodec requestCodec, IXdrCodec replyCodec )
    {
        if ( this._socket is null || this.Encoder is null || this.Decoder is null ) return;
        lock ( this._lock )
            // Refresh:
            for ( int refreshesLeft = 1; refreshesLeft >= 0; --refreshesLeft )
            {
                // First, build the ONC/RPC call header. Then put the sending
                // stream into a known state and encode the parameters to be
                // sent. Finally tell the encoding stream to send all its data
                // to the server. Then wait for an answer, receive it and decode
                // it. So that's the bottom line of what we do right here.
                this.NextTransactionId();
                OncRpcClientCallMessage callHeader = new( this.MessageId, this.Program, versionNumber, procedureNumber, this.Auth );
                OncRpcClientReplyMessage replyHeader = new( this.Auth );

                // Send call message to server. If we receive an IOException,
                // then we'll throw the appropriate ONC/RPC (client) exception.
                // Note that we use a connected stream, so we don't need to
                // specify a destination when beginning serialization.
                try
                {
                    this._socket!.ReceiveTimeout = this.TransmitTimeout;
                    this.Encoder!.BeginEncoding( new IPEndPoint( IPAddress.Any, 0 ) );
                    callHeader.Encode( this.Encoder! );
                    requestCodec.Encode( this.Encoder! );
                    // @ATECoder: thus was replace in the next statement if ( this.IOTimeout != 0 )
                    if ( !this.UseCallBatching )
                        this.Encoder?.EndEncoding();
                    else
                        this.Encoder?.EndEncoding( false );
                }
                catch ( System.IO.IOException e )
                {
                    throw new OncRpcException( OncRpcExceptionReason.OncRpcCannotSend, e );
                }
                // Receive reply message from server -- at least try to do so...
                // In case of batched calls we don't need no answer, so
                // we can do other, more interesting things.
                // @ATECoder: thus was replace in the next statement if ( this.IOTimeout == 0 )
                if ( this.UseCallBatching )
                    return;
                try
                {
                    // Keep receiving until we get the matching reply.
                    while ( true )
                    {
                        this._socket.ReceiveTimeout = this.IOTimeout;
                        this.Decoder!.BeginDecoding();
                        this._socket.ReceiveTimeout = this.TransmitTimeout;

                        // First, pull off the reply message header of the
                        // XDR stream. In case we also received a verifier
                        // from the server and this verifier was invalid, broken
                        // or tampered with, we will get an
                        // OncRpcAuthenticationException right here, which will
                        // propagate up to the caller. If the server reported
                        // an authentication problem itself, then this will
                        // be handled as any other rejected ONC/RPC call.
                        try
                        {
                            replyHeader.Decode( this.Decoder! );
                        }
                        catch ( OncRpcException e )
                        {
                            // ** SF bug #1262106 **

                            // We ran into some sort of trouble. Usually this will have
                            // been a buffer underflow. Whatever, end the decoding process
                            // and ensure this way that the next call has a chance to start
                            // from a clean state.
                            this.Decoder!.EndDecoding();
                            throw e;
                        }
                        // Only deserialize the result, if the reply matches the
                        // call. Otherwise skip this record.
                        if ( replyHeader.MessageId == callHeader.MessageId )
                            break;
                        this.Decoder.EndDecoding();
                    }
                    // Make sure that the call was accepted. In case of unsuccessful
                    // calls, throw an exception, if it's not an authentication
                    // exception. In that case try to refresh the credential first.
                    if ( !replyHeader.SuccessfullyAccepted() )
                    {
                        this.Decoder.EndDecoding();

                        // Check whether there was an authentication
                        // problem. In this case first try to refresh the
                        // credentials.
                        if ( refreshesLeft > 0 && replyHeader.ReplyStatus == OncRpcReplyStatus.OncRpcMessageDenied
                                               && replyHeader.RejectStatus == OncRpcRejectStatus.OncRpcAuthError
                                               && (this.Auth?.CanRefreshCredential() ?? false) )
                            // continue Refresh;
                            continue;
                        // Nope. No chance. This gets tough.
                        throw replyHeader.NewException();
                    }
                    try
                    {
                        replyCodec.Decode( this.Decoder );
                    }
                    catch ( OncRpcException e )
                    {
                        // ** SF bug #1262106 **

                        // We ran into some sort of trouble. Usually this will have
                        // been a buffer underflow. Whatever, end the decoding process
                        // and ensure this way that the next call has a chance to start
                        // from a clean state.
                        this.Decoder.EndDecoding();
                        throw e;
                    }
                    // Free pending resources of buffer and exit the call loop,
                    // returning the reply to the caller through the result
                    // object.
                    this.Decoder.EndDecoding();
                    return;
                }
                catch ( System.IO.IOException e )
                {
                    // Argh. Trouble with the transport. Seems like we can't
                    // receive data. Gosh. Go away!
                    throw new OncRpcException( OncRpcExceptionReason.OncRpcCannotReceive, e );
                }
            }
    }

    /// <summary>   Issues a batched call for a remote procedure to an ONC/RPC server. </summary>
    /// <remarks>
    /// Below is a small example (exception handling omitted for clarity):
    /// <code>
    /// OncRpcTcpClient client = new ( IPAddress.Loopback, myProgramNumber, myProgramVersion, OncRpcProtocols.OncRpcTCP );
    /// client.CallBatch(42, myRequestCodec, false);
    /// client.CallBatch(42, myOtherRequestCodec, false);
    /// client.CallBatch(42, myFinalRequestCodec, true);
    /// </code>
    /// In the example above, three calls are batched in a row and only be sent all together with the
    /// third call. Note that batched calls must not expect replies, with the only exception being
    /// the last call in a batch:
    /// <code>
    /// client.CallBatch(42, myRequestCodec, false);
    /// client.CallBatch(42, myOtherRequestCodec, false);
    /// client.call( 43, myFinalRequestCodec, myFinalResultCodec );
    /// </code>
    /// </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="procedureNumber">  Procedure number of the procedure to call. </param>
    /// <param name="requestCodec">     The XDR codec that is sent to the procedure call. </param>
    /// <param name="flush">            Make sure that all pending batched calls are sent to the
    ///                                 server. </param>
    public virtual void BatchCall( int procedureNumber, IXdrCodec requestCodec, bool flush )
    {
        if ( this._socket is null || this.Encoder is null ) { return; }
        lock ( this._lock )
        {
            // First, build the ONC/RPC call header. Then put the sending
            // stream into a known state and encode the parameters to be
            // sent. Finally tell the encoding stream to send all its data
            // to the server. We don't then need to wait for an answer. And
            // we don't need to take care of credential refreshes either.
            this.NextTransactionId();
            OncRpcClientCallMessage callHeader = new( this.MessageId, this.Program, this.Version, procedureNumber, this.Auth );

            // Send call message to server. If we receive an IOException,
            // then we'll throw the appropriate ONC/RPC (client) exception.
            // Note that we use a connected stream, so we don't need to
            // specify a destination when beginning serialization.
            try
            {
                this._socket.SendTimeout = this.TransmitTimeout;
                this.Encoder.BeginEncoding( new IPEndPoint( IPAddress.Any, 0 ) );
                callHeader.Encode( this.Encoder );
                requestCodec.Encode( this.Encoder );
                this.Encoder.EndEncoding( flush );
            }
            catch ( System.IO.IOException e )
            {
                throw new OncRpcException( OncRpcExceptionReason.OncRpcCannotSend, e );
            }
        }
    }

    #endregion

}
