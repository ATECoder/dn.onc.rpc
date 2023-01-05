
using System.Net.Sockets;
using System.IO;
using cc.isr.XDR;

namespace cc.isr.ONC.RPC;

/// <summary>
/// ONC/RPC client which communicates with ONC/RPC servers over the network using the datagram-
/// oriented protocol UDP/IP.
/// </summary>
/// <remarks> <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public class OncRpcUdpClient : OncRpcClientBase
{

    #region " CONSTRUCTION and CLEANUP "

    /// <summary>
    /// Constructs a new <see cref="OncRpcUdpClient"/> object, which connects to the ONC/RPC server at
    /// <paramref name="host"/> for calling remote procedures of the given { <paramref name="program"/>,
    /// <paramref name="version"/> }.
    /// </summary>
    /// <remarks>
    /// Note that the construction of an <see cref="OncRpcUdpClient"/>
    /// object will result in communication with the portmap process at
    /// <paramref name="host"/> if <paramref name="port"/> is <c>0</c>.
    /// </remarks>
    /// <param name="host">     The host where the ONC/RPC server resides. </param>
    /// <param name="program">  Program number of the ONC/RPC server to call. </param>
    /// <param name="version">  Program version number. </param>
    /// <param name="port">     The port number where the ONC/RPC server can be contacted. If <c>0</c>
    ///                         , then the <see cref="OncRpcUdpClient"/> object will ask the portmapper at <paramref name="host"/>
    ///                         for the port number. </param>
    ///
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="IOException">      Thrown when an I/O error condition occurs. </exception>
    public OncRpcUdpClient( IPAddress host, int program, int version, int port ) : this( host, program, version, port, OncRpcClientBase.DefaultBufferSize )
    {
    }

    /// <summary>
    /// Constructs a new <see cref="OncRpcUdpClient"/> object, which connects to the ONC/RPC server at
    /// <paramref name="host"/> for calling remote procedures of the given { <paramref name="program"/>, 
    /// <paramref name="version"/> }.
    /// </summary>
    /// <remarks>
    /// Note that the construction of an <see cref="OncRpcUdpClient"/>
    /// object will result in communication with the portmap process at
    /// <paramref name="host"/> if <paramref name="port"/> is <c>0</c>.
    /// </remarks>
    /// <param name="host">         The host where the ONC/RPC server resides. </param>
    /// <param name="program">      Program number of the ONC/RPC server to call. </param>
    /// <param name="version">      Program version number. </param>
    /// <param name="port">         The port number where the ONC/RPC server can be contacted. If 
    ///                             <c>0</c>, then the <see cref="OncRpcUdpClient"/> object will ask
    ///                             the portmapper at <paramref name="host"/> for the port number. </param>
    /// <param name="bufferSize">   The buffer size used for sending and receiving UDP datagrams. </param>
    ///
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="IOException">      Thrown when an I/O error condition occurs. </exception>
    public OncRpcUdpClient( IPAddress host, int program, int version, int port, int bufferSize ) : base( host, program, version, port, OncRpcProtocols.OncRpcUdp )
    {
        this.RetransmissionTimeout = base.Timeout;

        // Constructs the inherited part of our object. This will also try to
        // lookup the port of the desired ONC/RPC server, if no port number
        // was specified (port = 0).


        // Let the host operating system choose which port (and network
        // interface) to use. Then set the buffer sizes for sending and
        // receiving UDP datagrams. Finally set the destination of packets.

        if ( bufferSize < OncRpcClientBase.DefaultMinBufferSize ) bufferSize = OncRpcClientBase.DefaultMinBufferSize;
        this._socket = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );
        if ( this._socket.SendBufferSize < bufferSize )
            this._socket.SendBufferSize = bufferSize;
        if ( this._socket.ReceiveBufferSize < bufferSize )
            this._socket.ReceiveBufferSize = bufferSize;

        // Note: we don't do a
        //   socket.connect(host, this.port);
        // here anymore. XdrUdpEncodingStream long since then supported
        // specifying the destination of an ONC/RPC UDP packet when
        // start serialization. In addition, connecting a UDP socket disables
        // the socket's ability to receive broadcasts. Without connecting you
        // can send an ONC/RPC call to the broadcast address of the network
        // and receive multiple replies.

        // Create the necessary encoding and decoding streams, so we can
        // communicate at all.

        this._encoder = new XdrUdpEncodingStream( this._socket, bufferSize );
        this._decoder = new XdrUdpDecodingStream( this._socket, bufferSize );
    }

    /// <summary>
    /// Close the connection to an ONC/RPC server and free all network-related resources.
    /// </summary>
    ///
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    public override void Close()
    {
        if ( this._socket != null )
        {
            this._socket.Close();
            this._socket = null;
        }
        if ( this._encoder != null )
        {
            try
            {
                this._encoder.Close();
            }
            catch ( IOException )
            {
            }
            this._encoder = null;
        }
        if ( this._decoder != null )
        {
            try
            {
                this._decoder.Close();
            }
            catch ( IOException )
            {
            }
            this._decoder = null;
        }
    }

    /// <summary>
    /// UDP socket used for datagram-based communication with an ONC/RPC
    /// server.
    /// </summary>
    private Socket _socket;

    /// <summary>
    /// XDR encoding stream used for sending requests via UDP/IP to an ONC/RPC server.
    /// </summary>
    private XdrUdpEncodingStream _encoder;

    /// <summary>
    /// XDR decoding stream used when receiving replies via UDP/IP from an ONC/RPC server.
    /// </summary>
    private XdrUdpDecodingStream _decoder;

    #endregion

    #region " SETTINGS "

    /// <summary>
    /// Gets or sets the retransmission mode used when resending ONC/RPC calls. Default mode is
    /// <see cref="OncRpcUdpRetransmissionMode.OncRpcFixedTimeout">fixed timeout mode</see>.
    /// </summary>
    /// <value> The retransmission mode. </value>
    public int RetransmissionMode { get; set; } = OncRpcUdpRetransmissionMode.OncRpcFixedTimeout;

    /// <summary>
    /// Gets or sets the retransmission timeout used for resending ONC/RPC calls when an ONC/RPC
    /// server does not answer fast enough. The default retransmission timeout is identical to the
    /// overall <see cref="OncRpcClientBase.Timeout"/> for ONC/RPC calls (thus UDP/IP-based clients
    /// will not retransmit lost calls). A timeout of zero indicates batched calls.
    /// </summary>
    /// <remarks>
    /// The default retransmission timeout is <see cref="OncRpcClientBase.DefaultTimeout"/> (30
    /// seconds). The retransmission timeout must be greater than 0. To disable retransmission of
    /// lost calls, set the retransmission timeout to be the same value as the timeout.
    /// The timeout gets modified depending on the <see cref="RetransmissionMode"/>
    /// </remarks>
    /// <value> The retransmission timeout. </value>
    public int RetransmissionTimeout { get; set; }

    /// <summary>   Set the character encoding for serializing strings. </summary>
    /// <param name="characterEncoding">    the encoding to use for serializing strings. If 
    ///                                     <see langword="null"/>, the system's default encoding is to be used. </param>
    public override void SetCharacterEncoding( string characterEncoding )
    {
        this._decoder.CharacterEncoding = characterEncoding;
        this._encoder.CharacterEncoding = characterEncoding;
    }

    /// <summary>   Get the character encoding for serializing strings. </summary>
    /// <returns>
    /// the encoding currently used for serializing strings. If <see langword="null"/>, then the
    /// system's default encoding is used.
    /// </returns>
    public override string GetCharacterEncoding()
    {
        return this._decoder.CharacterEncoding;
    }

    #endregion

    #region " OPERATIONS "

    /// <summary>   Calls a remote procedure on an ONC/RPC server. </summary>
    /// <remarks>
    /// The <see cref="OncRpcUdpClient"/> uses a similar timeout scheme as
    /// the genuine Sun C implementation of ONC/RPC: it starts with a timeout of one second when
    /// waiting for a reply. If no reply is received within this time frame, the client doubles the
    /// timeout, sends a new request and then waits again for a reply. In every case the client will
    /// wait no longer than the total timeout set through the <see cref="OncRpcClientBase.Timeout"/> property.
    /// </remarks>
    /// <exception cref="OncRpcException">          Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="OncRpcTimeoutException">   Thrown when an ONC/RPC Timeout error condition
    ///                                             occurs. </exception>
    /// <param name="procedureNumber">  Procedure number of the procedure to call. </param>
    /// <param name="versionNumber">    Protocol version number. </param>
    /// <param name="requestCodec">     The XDR codec that is sent to the procedure call. </param>
    /// <param name="replyCodec">       The XDR codec that receives the result of the procedure call. </param>
    public override void Call( int procedureNumber, int versionNumber, IXdrCodec requestCodec, IXdrCodec replyCodec )
    {
        lock ( this )
            //Refresh:
            for ( int refreshesLeft = 1; refreshesLeft >= 0; --refreshesLeft )
            {
                bool refreshFlag = false;

                // First, build the ONC/RPC call header. Then put the sending
                // stream into a known state and encode the parameters to be
                // sent. Finally tell the encoding stream to send all its data
                // to the server. Then wait for an answer, receive it and decode
                // it. So that's the bottom line of what we do right here.

                this.NextTransactionId();

                // We only create our request message once and reuse it in case
                // retransmission should be necessary -- with the exception being
                // credential refresh. In this case we need to create a new
                // request message.

                OncRpcClientCallMessage callHeader = new( this.MessageId, this.Program, versionNumber, procedureNumber, this.Auth );
                OncRpcClientReplyMessage replyHeader = new( this.Auth );
                long stopTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond + this.Timeout;
                int resendTimeout = this.RetransmissionTimeout;
                do
                {

                    // Now enter the great loop where we send calls out to the server
                    // and then sit there waiting for a reply. If none comes, we first
                    // resend our call after one second, then two seconds, four seconds,
                    // and so on, until we have reached the timeout for the call in total.
                    // Note that this setting only applies if exponential back-off
                    // retransmission has been selected. Per default we do not retransmit
                    // any more, in order to be in line with the SUNRPC implementations.

                    try
                    {

                        // Send call message to server. Remember that we've already
                        // "connected" the datagram socket, so the destination for the 
                        // datagram packets is already set.

                        this._encoder.BeginEncoding( this.Host, this.Port );
                        callHeader.Encode( this._encoder );
                        requestCodec.Encode( this._encoder );
                        this._encoder.EndEncoding();
                    }
                    catch ( IOException e )
                    {
                        throw new OncRpcException( OncRpcException.OncRpcCannotSend, e.Message );
                    }

                    // Receive reply message from server -- at least try to do so...
                    // In case of batched calls we don't need no answer, so
                    // we can do other, more interesting things.

                    if ( this.Timeout == 0 )
                        return;

                    // Wait for an answer to arrive...

                    for (; ; )
                    {
                        try
                        {
                            long currentTimeout = stopTime - DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                            if ( currentTimeout > resendTimeout )
                                currentTimeout = resendTimeout;
                            else
                                if ( currentTimeout < 1 )

                                // as setSoTimeout interprets a timeout of zero as
                                // infinite (?§$@%&!!!) we need to ensure that we
                                // have a finite timeout, albeit maybe an infinitesimal
                                // finite one.
                                currentTimeout = 1;
                            this._socket.ReceiveTimeout = ( int ) currentTimeout;
                            this._decoder.BeginDecoding();

                            // Only accept incoming reply if it comes from the same
                            // address we've sent the ONC/RPC call to. Otherwise throw
                            // away the datagram packet containing the reply and start
                            // over again, waiting for the next reply to arrive.

                            if ( this.Host.Equals( this._decoder.GetSenderAddress() ) )
                            {

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
                                    replyHeader.Decode( this._decoder );
                                }
                                catch ( OncRpcException e )
                                {

                                    // ** SF bug #1262106 **

                                    // We ran into some sort of trouble. Usually this will have
                                    // been a buffer underflow. Whatever, end the decoding process
                                    // and ensure this way that the next call has a chance to start
                                    // from a clean state.

                                    this._decoder.EndDecoding();
                                    throw e;
                                }

                                // Only deserialize the result, if the reply matches the call
                                // and if the reply signals a successful call. In case of an
                                // unsuccessful call (which mathes our call nevertheless) throw
                                // an exception.

                                if ( replyHeader.MessageId == callHeader.MessageId )
                                {
                                    if ( !replyHeader.SuccessfullyAccepted() )
                                    {
                                        this._decoder.EndDecoding();

                                        // Check whether there was an authentication
                                        // problem. In this case first try to refresh the
                                        // credentials.

                                        if ( refreshesLeft > 0 && replyHeader.ReplyStatus == OncRpcReplyStatus.OncRpcMessageDenied &&
                                            replyHeader.RejectStatus == OncRpcRejectStatus.OncRpcAuthError && this.Auth != null && this.Auth.CanRefreshCredential() )
                                        {

                                            // Think about using a TAB size of four ;)

                                            // Another instance of "CONTINUE considered
                                            // useful"...

                                            refreshFlag = true;
                                            break;
                                        }
                                        //  continue Refresh;

                                        // Nope. No chance. This gets tough.

                                        throw replyHeader.NewException();
                                    }

                                    // The reply header is okay and the call had been
                                    // accepted by the ONC/RPC server, so we can now
                                    // proceed to decode the outcome of the RPC.

                                    try
                                    {
                                        replyCodec.Decode( this._decoder );
                                    }
                                    catch ( OncRpcException e )
                                    {

                                        // ** SF bug #1262106 **

                                        // We ran into some sort of trouble. Usually this will have
                                        // been a buffer underflow. Whatever, end the decoding process
                                        // and ensure this way that the next call has a chance to start
                                        // from a clean state.

                                        this._decoder.EndDecoding();
                                        throw e;
                                    }

                                    // Free pending resources of buffer and exit the call loop,
                                    // returning the reply to the caller through the result
                                    // object.

                                    this._decoder.EndDecoding();
                                    return;
                                }
                            }
                        }
                        catch ( SocketException )
                        {

                            // The message id did no match -- probably some
                            // old UDP datagram which just popped up from the
                            // middle of the Internet.

                            // Yet another case of "CONTINUE considered not
                            // harmful"...

                            // [Nothing to do here, just wait for the next datagram]

                            // IP address of received UDP datagram is not the same
                            // as the IP address of the ONC/RPC server.

                            // [Nothing to do here, just wait for the next datagram]

                            // Note that we only catch timeouts here, but no other
                            // exceptions. Those others will go up further until someone
                            // catches them. The timeouts are caught, so they can do no
                            // damage but instead we start another round of sending a
                            // request and waiting for a reply. Reminds me of NASA and
                            // their "Mars Polar Lander"...

                            // Note that we need to leave the inner waiting loop here,
                            // as we might need to resend the (lost) RPC request
                            // datagram.

                            //Console.Out.WriteLine("This is supposed to be ignored");
                            //Console.Out.WriteLine(e.Message);
                            //Console.Out.WriteLine(e.StackTrace);
                            break;
                        }
                        catch ( IOException e )
                        {

                            // Argh. Trouble with the transport. Seems like we can't
                            // receive data. Gosh. Go away!

                            try
                            {
                                this._decoder.EndDecoding();
                            }
                            catch ( IOException )
                            {
                            }
                            // skip UDP record
                            throw new OncRpcException( OncRpcException.OncRpcCannotReceive, e.Message );
                        }
                        catch ( OncRpcException e )
                        {

                            // Ooops. An ONC/RPC exception. Let us rethrow this one,
                            // as we won't have nothin' to do with it...

                            try
                            {
                                this._decoder.EndDecoding();
                            }
                            catch ( IOException )
                            {
                            }
                            // skip UDP record

                            // Well, in case we got not a *reply* RPC message back,
                            // we keep listening for messages.

                            if ( e.Reason != OncRpcException.OncRpcWrongMessageType )
                                throw e;
                        }

                        // We cannot make use of the reply we just received, so
                        // we need to dump it.

                        // This should raise no exceptions, when skipping the UDP
                        // record. So if one is raised, we will rethrow an ONC/RPC
                        // exception instead.

                        try
                        {
                            this._decoder.EndDecoding();
                        }
                        catch ( IOException e )
                        {
                            throw new OncRpcException( OncRpcException.OncRpcCannotReceive, e.Message );
                        }
                    }
                    // @jmw 12/18/2009 fix for refresh/continue in the c# implementation
                    if ( refreshFlag )
                        break;

                    // We only reach this code part beyond the inner waiting
                    // loop if we Runs in a timeout and might need to retransmit

                    // According to the retransmission strategy chosen, update the
                    // current retransmission (resending) timeout.

                    if ( this.RetransmissionMode == OncRpcUdpRetransmissionMode.OncRpcExponentialTimeout )
                        resendTimeout *= 2;
                }
                while ( DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond < stopTime );
                // @jmw 12/18/2009 don't want to throw an exception here
                if ( refreshFlag )
                    continue;

                // That's it -- this shitty server does not talk to us. Now, due to
                // the indecent language used in the previous sentence, this software
                // cannot be exported any longer to some countries of the world.
                // But this is surely not my problem, but rather theirs. So go away
                // and hide yourself in the dark with all your zombies (or maybe
                // kangaroos).

                throw new OncRpcTimeoutException();
            }
    }

    /// <summary>   Broadcast a remote procedure call to several ONC/RPC servers. </summary>
    /// <remarks>
    /// For this you'll need to specify either a multi-cast address or the subnet's broadcast address
    /// when creating a <see cref="OncRpcUdpClient"/>. For every reply received, an event containing the reply is sent to
    /// the <see cref="IOncRpcBroadcastListener">listener</see>, which is the last parameter to the this
    /// method. <para>
    /// In contrast to the
    /// <see cref="OncRpcClientBase.Call(int, IXdrCodec, IXdrCodec)"/>
    /// method, <see cref="BroadcastCall"/> will only send the ONC/RPC call once. It will then wait
    /// for answers until the timeout as set by <see cref="OncRpcClientBase.Timeout"/>
    /// expires without resending the reply. </para> <para>
    /// Note that you might experience unwanted results when using
    /// authentication types other than <see cref="OncRpcClientAuthNone"/>, causing
    /// messed up authentication protocol handling objects. This depends on the type of
    /// authentication used. For <see cref="OncRpcAuthType.OncRpcAuthTypeUnix"/> nothing bad happens as long as none of the
    /// servers replies with a shorthand verifier. If it does, then this shorthand will be used on
    /// all subsequent ONC/RPC calls, something you probably do not want at all. </para>
    /// </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="procedureNumber">  Procedure number of the procedure to call. </param>
    /// <param name="requestCodec">     The XDR codec that is sent to the procedure call. </param>
    /// <param name="replyCodec">       The XDR codec that receives the result of the procedure call. </param>
    /// <param name="listener">         Listener which will get an <see cref="OncRpcBroadcastEvent"/>
    ///                                 for every reply received. </param>
    public virtual void BroadcastCall( int procedureNumber, IXdrCodec requestCodec, IXdrCodec replyCodec, IOncRpcBroadcastListener listener )
    {
        lock ( this )
        {

            // First, build the ONC/RPC call header. Then put the sending
            // stream into a known state and encode the parameters to be
            // sent. Finally tell the encoding stream to broadcast all its data.
            // Then wait for answers, receive and decode them one at a time.

            this.NextTransactionId();
            OncRpcClientCallMessage callHeader = new( this.MessageId, this.Program, this.Version, procedureNumber, this.Auth );
            OncRpcClientReplyMessage replyHeader = new( this.Auth );

            // Broadcast the call. Note that we send the call only once and will
            // never resend it.

            try
            {

                // Send call message to server. Remember that we've already
                // "connected" the datagram socket, so the destination of the 
                // datagram packets is already set.

                this._encoder.BeginEncoding( this.Host, this.Port );
                callHeader.Encode( this._encoder );
                requestCodec.Encode( this._encoder );
                this._encoder.EndEncoding();
            }
            catch ( IOException e )
            {
                throw new OncRpcException( OncRpcException.OncRpcCannotSend, e.Message );
            }

            // Now enter the great loop where sit waiting for replies to our
            // broadcast call to come in. In every case, we wait until the
            // (total) timeout expires.

            // @atecoder: fix timeout; was ill defined.
            DateTime stopTime = DateTime.Now.Add( TimeSpan.FromMilliseconds( this.Timeout ) );
            do
                try
                {

                    // Calculate timeout until the total timeout is reached, so
                    // we can try to meet the overall deadline.

                    TimeSpan currentTimeout = stopTime - DateTime.Now;
                    if ( currentTimeout.Ticks < 0 )
                        currentTimeout = new TimeSpan( 0 );
                    // @atecoder: fix timeout; was .Seconds, that is, 1000 times larger.
                    this._socket.ReceiveTimeout = currentTimeout.Milliseconds;

                    // Then wait for datagrams to arrive...

                    this._decoder.BeginDecoding();
                    replyHeader.Decode( this._decoder );

                    // Only deserialize the result, if the reply matches the call
                    // and if the reply signals a successful call. In case of an
                    // unsuccessful call (which matches our call nevertheless) throw
                    // an exception.

                    if ( replyHeader.MessageId == callHeader.MessageId )
                    {
                        if ( !replyHeader.SuccessfullyAccepted() )

                            // We got a notification of a rejected call. We silently
                            // ignore such replies and continue listening for other
                            // replies.

                            this._decoder.EndDecoding();

                        replyCodec.Decode( this._decoder );

                        // Notify a potential listener of the reply.

                        if ( listener != null )
                        {
                            OncRpcBroadcastEvent evt = new( this, this._decoder.GetSenderAddress(), procedureNumber, requestCodec, replyCodec );
                            listener.ReplyReceived( evt );
                        }

                        // Free pending resources of buffer and exit the call loop,
                        // returning the reply to the caller through the result
                        // object.

                        this._decoder.EndDecoding();
                    }
                    else

                        // This should raise no exceptions, when skipping the UDP
                        // record. So if one is raised, we will rethrow an ONC/RPC
                        // exception instead.

                        try
                        {
                            this._decoder.EndDecoding();
                        }
                        catch ( IOException e )
                        {
                            throw new OncRpcException( OncRpcException.OncRpcCannotReceive, e.Message );
                        }
                }
                catch ( SocketException )
                {
                }
                catch ( IOException e )
                {

                    // Note that we only catch timeouts here, but no other
                    // exceptions. Those others will go up further until someone
                    // catches them. If we get the timeout we know that it
                    // could be time to leave the stage and so we fall through
                    // to the total timeout check.


                    // Argh. Trouble with the transport. Seems like we can't
                    // receive data. Gosh. Go away!

                    throw new OncRpcException( OncRpcException.OncRpcCannotReceive, e.Message );
                }
            while ( DateTime.Now < stopTime );
            return;
        }
    }

    #endregion

}
