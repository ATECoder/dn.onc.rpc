
using System.Net.Sockets;

namespace cc.isr.ONC.RPC.Client;

/// <summary>
/// ONC/RPC client which communicates with ONC/RPC servers over the network using the stream-
/// oriented protocol TCP/IP.
/// </summary>
/// <remarks> <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public class OncRpcTcpClient : OncRpcClientBase
{

    #region " CONSTRUCTION and CLEANP "

    /// <summary>
    /// Constructs a new server <see cref="OncRpcTcpClient"/> object, which connects to the ONC/RPC server at
    /// <paramref name="host"/> for calling remote procedures of the given { <paramref name="program"/>, <paramref name="version"/> }.
    /// </summary>
    /// <remarks>
    /// Note that the construction of an server <see cref="OncRpcTcpClient"/>
    /// object will result in communication with the portmap process at
    /// <paramref name="host"/>.
    /// </remarks>
    /// <param name="host">     The host where the ONC/RPC server resides. </param>
    /// <param name="program">  Program number of the ONC/RPC server to call. </param>
    /// <param name="version">  Program version number. </param>
    ///
    /// <exception cref="OncRpcException">          Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="System.IO.IOException">    Thrown when an I/O error condition occurs. </exception>
    public OncRpcTcpClient( IPAddress host, int program, int version ) : this( host, program, version, 0, 0 )
    {
    }

    /// <summary>
    /// Constructs a new server <see cref="OncRpcTcpClient"/> object, which connects to the ONC/RPC server at
    /// <paramref name="host"/> for calling remote procedures of the given { <paramref name="program"/>, <paramref name="version"/> }.
    /// </summary>
    /// <remarks>
    /// Note that the construction of an server <see cref="OncRpcTcpClient"/>
    /// object will result in communication with the portmap process at
    /// <paramref name="host"/> if <paramref name="port"/> is <c>0</c>. 
    /// </remarks>
    /// <param name="host">     The host where the ONC/RPC server resides. </param>
    /// <param name="program">  Program number of the ONC/RPC server to call. </param>
    /// <param name="version">  Program version number. </param>
    /// <param name="port">     The port number where the ONC/RPC server can be contacted. If <c>0</c>,
    ///                         then the <see cref="OncRpcUdpClient"/> object will ask the
    ///                         portmapper at <paramref name="host"/> for the port number. </param>
    ///
    /// <exception cref="OncRpcException">          Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="System.IO.IOException">    Thrown when an I/O error condition occurs. </exception>
    public OncRpcTcpClient( IPAddress host, int program, int version, int port ) : this( host, program, version, port, 0 )
    {
    }

    /// <summary>
    /// Constructs a new server <see cref="OncRpcTcpClient"/> object, which connects to the ONC/RPC server at
    /// <paramref name="host"/> for calling remote procedures of the given { <paramref name="program"/>, <paramref name="version"/> }.
    /// </summary>
    /// <remarks>
    /// Note that the construction of an server <see cref="OncRpcTcpClient"/>
    /// object will result in communication with the portmap process at
    /// <paramref name="host"/> if <paramref name="port"/> is <c>0</c>.
    /// </remarks>
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
    ///                             will select the <see cref="OncRpcClientBase.DefaultBufferSize"/> (currently
    ///                             8192 bytes). </param>
    ///
    /// <exception cref="OncRpcException">          Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="System.IO.IOException">    Thrown when an I/O error condition occurs. </exception>
    public OncRpcTcpClient( IPAddress host, int program, int version, int port, int bufferSize ) : this( host, program, version, port, bufferSize, -1 )
    {
    }

    /// <summary>
    /// Constructs a new server <see cref="OncRpcTcpClient"/> object, which connects to the ONC/RPC server at
    /// <paramref name="host"/> for calling remote procedures of the given { <paramref name="program"/>, <paramref name="version"/> }.
    /// </summary>
    /// <remarks>
    /// Note that the construction of an server <see cref="OncRpcTcpClient"/>
    /// object will result in communication with the portmap process at
    /// <paramref name="host"/> if <paramref name="port"/> is <c>0</c>.
    /// </remarks>
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
    ///                             will select the <see cref="OncRpcClientBase.DefaultBufferSize"/> (currently
    ///                             8192 bytes). </param>
    /// <param name="timeout">      Maximum timeout in milliseconds when connecting to the ONC/RPC
    ///                             server. If negative, a default implementation-specific timeout
    ///                             setting will apply. <i>Note that this timeout only applies to the
    ///                             connection phase, but <b>not</b> to later communication.</i> </param>
    ///
    /// <exception cref="OncRpcException">          Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="System.IO.IOException">    Thrown when an I/O error condition occurs. </exception>
    public OncRpcTcpClient( IPAddress host, int program, int version, int port, int bufferSize, int timeout ) : base( host, program,
        version, port, OncRpcProtocols.OncRpcTcp )
    {
        // Constructs the inherited part of our object. This will also try to
        // lookup the port of the desired ONC/RPC server, if no port number
        // was specified (port = 0).

        // Let the host operating system choose which port (and network
        // interface) to use. Then set the buffer sizes for sending and
        // receiving UDP datagrams. Finally set the destination of packets.
        if ( bufferSize == 0 ) bufferSize = DefaultBufferSize;
        // default setting
        if ( bufferSize < DefaultMinBufferSize ) bufferSize = DefaultMinBufferSize;

        // Note that we use this.port at this time, because the superclass
        // might have resolved the port number in case the caller specified
        // simply 0 as the port number.

        // Constructs the socket and connect
        this._socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
        IPEndPoint endPoint = new( host, this.Port );
        this._socket.Connect( endPoint );
        if ( timeout >= 0 )
        {
            this._socket.SendTimeout = timeout;
            this._socket.ReceiveTimeout = timeout;
        }
        this._socket.NoDelay = true;
        if ( this._socket.SendBufferSize < bufferSize )
            this._socket.SendBufferSize = bufferSize;
        if ( this._socket.ReceiveBufferSize < bufferSize )
            this._socket.ReceiveBufferSize = bufferSize;

        // Create the necessary encoding and decoding streams, so we can
        // communicate at all.
        this.Encoder = new XdrTcpEncodingStream( this._socket, bufferSize );
        this.Decoder = new XdrTcpDecodingStream( this._socket, bufferSize );
        this.CharacterEncoding = OncRpcClientBase.DefaultEncoding;
    }

    /// <summary>
    /// Close the connection to an ONC/RPC server and free all network-related resources.
    /// </summary>
    ///
    /// <exception cref="OncRpcException">          Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="System.IO.IOException">    Thrown when an I/O error condition occurs. </exception>
    public override void Close()
    {
        if ( this._socket is not null )
        {
            Socket socket = this._socket;
            if ( socket.Connected )
                socket.Shutdown( SocketShutdown.Both );
            this._socket = null;
            socket.Close();
        }

        if ( this.Encoder is not null )
        {
            XdrEncodingStreamBase xdrStream = this.Encoder;
            this.Encoder = null;
            xdrStream.Close();
        }

        if ( this.Decoder is not null )
        {
            XdrDecodingStreamBase xdrStream = this.Decoder;
            this.Decoder = null;
            xdrStream.Close();
        }
    }

    /// <summary>
    /// TCP socket used for stream-oriented communication with an ONC/RPC
    /// server.
    /// </summary>
    private Socket _socket;

    #endregion

    #region " settings "

    /// <summary>
    /// Gets or sets or set the XDR encoding stream used for sending requests via TCP/IP to an
    /// ONC/RPC server.
    /// </summary>
    /// <value> The sending XDR encoding stream. </value>
    internal XdrTcpEncodingStream Encoder { get; private set; }

    /// <summary>
    /// Gets or sets or set the XDR decoding stream used when receiving replies via TCP/IP from an
    /// ONC/RPC server.
    /// </summary>
    /// <value> The receiving XDR decoding stream. </value>
    internal XdrTcpDecodingStream Decoder { get; private set; }

    /// <summary>
    /// Gets or sets the timeout during the phase where data is sent within calls, or data is received within replies.
    /// </summary>
    /// <remarks>
    /// If the flow of data when sending calls or receiving replies blocks longer than the given
    /// timeout, an exception is thrown. The timeout must be positive.
    /// </remarks>
    /// <value> The timeout used during transmission of data. </value>
    public int TransmissionTimeout { get; set; } = DefaultTransmissionTimeout;

    /// <summary>
    /// Gets or sets the encoding to use when serializing strings.
    /// </summary>
    /// <value> The character encoding. </value>
    public override Encoding CharacterEncoding
    {
        get => base.CharacterEncoding;
        set {
            base.CharacterEncoding = value;
            this.Decoder.CharacterEncoding = value;
            this.Encoder.CharacterEncoding = value;
        }
    }

    #endregion

    #region " operations "

    /// <summary>   Calls a remote procedure on an ONC/RPC server. </summary>
    /// <remarks>
    /// Please note that while this method supports call batching by setting the communication
    /// timeout to zero (<see cref="Timeout"/> to <c>0</c>) you should better use
    /// <see cref="BatchCall(int, IXdrCodec, bool)"/>
    /// as it provides better control over when the batch should be flushed to the server.
    /// </remarks>
    /// <param name="procedureNumber">  Procedure number of the procedure to call. </param>
    /// <param name="versionNumber">    Protocol version number. </param>
    /// <param name="requestCodec">     The XDR codec that is sent to the procedure call. </param>
    /// <param name="replyCodec">       The XDR codec that receives the result of the procedure call. </param>
    ///
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    public override void Call( int procedureNumber, int versionNumber, IXdrCodec requestCodec, IXdrCodec replyCodec )
    {
        lock ( this )
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
                    this._socket.ReceiveTimeout = this.TransmissionTimeout;
                    this.Encoder.BeginEncoding( null, 0 );
                    callHeader.Encode( this.Encoder );
                    requestCodec.Encode( this.Encoder );
                    if ( this.Timeout != 0 )
                        this.Encoder.EndEncoding();
                    else
                        this.Encoder.EndEncoding( false );
                }
                catch ( System.IO.IOException e )
                {
                    throw new OncRpcException( OncRpcException.OncRpcCannotSend, e.Message );
                }
                // Receive reply message from server -- at least try to do so...
                // In case of batched calls we don't need no answer, so
                // we can do other, more interesting things.
                if ( this.Timeout == 0 )
                    return;
                try
                {
                    // Keep receiving until we get the matching reply.
                    while ( true )
                    {
                        this._socket.ReceiveTimeout = this.Timeout;
                        this.Decoder.BeginDecoding();
                        this._socket.ReceiveTimeout = this.TransmissionTimeout;

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
                            replyHeader.Decode( this.Decoder );
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
                        if ( refreshesLeft > 0 && replyHeader.ReplyStatus == OncRpcReplyStatus.OncRpcMessageDenied &&
                            replyHeader.RejectStatus == OncRpcRejectStatus.OncRpcAuthError && this.Auth is not null && this.Auth.CanRefreshCredential() )
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
                    throw new OncRpcException( OncRpcException.OncRpcCannotReceive, e.Message );
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
    /// <param name="procedureNumber">  Procedure number of the procedure to call. </param>
    /// <param name="requestCodec">     The XDR codec that is sent to the procedure call. </param>
    /// <param name="flush">            Make sure that all pending batched calls are sent to the
    ///                                 server. </param>
    ///
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    public virtual void BatchCall( int procedureNumber, IXdrCodec requestCodec, bool flush )
    {
        lock ( this )
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
                this._socket.SendTimeout = this.TransmissionTimeout;
                this.Encoder.BeginEncoding( null, 0 );
                callHeader.Encode( this.Encoder );
                requestCodec.Encode( this.Encoder );
                this.Encoder.EndEncoding( flush );
            }
            catch ( System.IO.IOException e )
            {
                throw new OncRpcException( OncRpcException.OncRpcCannotSend, e.Message );
            }
        }
    }

    #endregion

}
