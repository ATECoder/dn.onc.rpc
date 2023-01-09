using cc.isr.ONC.RPC.Portmap;

namespace cc.isr.ONC.RPC.Client;

/// <summary>
/// The abstract <see cref="OncRpcClientBase"/> class is the foundation for protocol-specific ONC/RPC
/// clients.
/// </summary>
/// <remarks>
/// It encapsulates protocol-independent functionality, like port resolving, if no port
/// was specified for the ONC/RPC server to contact. This class also provides the method skeleton,
/// for instance for executing procedure calls. <para>
/// In order to communicate with an ONC/RPC server, you need to create an
/// ONC/RPC client, represented by classes derived from <see cref="OncRpcClientBase"/>. The most
/// generic way to generate an ONC/RPC client is as follows: use
/// <see cref="NewOncRpcClient(IPAddress, int, int, OncRpcProtocols)"/> and specify: </para>
/// <list type="bullet"> <item>
/// the host (of class <see cref="IPAddress"/>) where the ONC/RPC server resides, </item><item>
/// the ONC/RPC program number of the server to contact,</item><item>
/// the program's version number,</item><item>
/// and finally the IP protocol to use when talking to the server. This can be either
/// <see cref="OncRpcProtocols.OncRpcUdp"/> or <see cref="OncRpcProtocols.OncRpcTcp"/>.</item></list> <para>
/// 
/// The next code snippet shows how to create an ONC/RPC client, which can
/// communicate over UDP/IP with the ONC/RPC server for program number
/// <c>0x49678</c> on the same host (by coincidence, this is the program
/// number of the <a href="http://www.acplt.org/ks">ACPLT/KS</a> protocol). </para>
/// <code>
/// OncRpcClient client;
/// try {
///     int program = 0x49678;
///     int version = 1;
///     client = OncRpcClient.NewOncRpcClient(IPAddress.Loopback, program, version, OncRpcProtocols.ONCRPC_UDP);
/// }
/// catch ( OncRpcProgramNotRegisteredException e ) {
///     Console.WriteLine("ONC/RPC program server not found");
///     System.exit(0);
/// } 
/// catch ( OncRpcException e ) {
///    Console.WriteLine("Could not contact port mapper:");
///    Console.WriteLine( e.ToString() );
///    System.exit(0);
/// } 
/// catch ( IOException e )  {
///    Console.WriteLine("Could not contact port mapper:");
///    Console.WriteLine( e.ToString() );
///    System.exit(0);
/// }
/// </code> <para>
/// This code snippet also shows exception handling. The most common error you'll see is
/// probably an <see cref="OncRpcExceptionReason.OncRpcProgramNotRegistered"/> exception,
/// in case no such program number is currently registered at the specified host.
/// In case no ONC/RPC port mapper is available at the specified host, you might get an
/// <see cref="OncRpcExceptionReason.OncRpcProcedureCallTimedOut"/> instead.
/// You might also get an IOException when using TCP/IP and the server
/// cannot be contacted because it does not accept new connections.  </para> <para>
/// Instead of calling <see cref="NewOncRpcClient(IPAddress, int, int, OncRpcProtocols)"/>
/// you can also directly create objects of classes <see cref="OncRpcTcpClient"/>
/// and <see cref="OncRpcUdpClient"/>
/// if you know at compile time which kind of IP protocol you will use. </para> <para>
/// With a client proxy in your hands, you can now issue ONC/RPC calls. As
/// a really, really simple example -- did I say "simple example"? -- we start
/// with the famous ONC/RPC ping call. This call sends no parameters and expects
/// no return from an ONC/RPC server. It is just used to check whether a server
/// is still responsive. </para>
/// <code>
/// Console.Write("pinging server: ");
/// try {
///    client.Call(0, XdrVoid.XdrVoidInstance, XdrVoid.XdrVoidInstance);
/// }
/// catch ( OncRpcException e ) {
///    Console.WriteLine("method call failed unexpectedly:");
///    Console.WriteLine( e.ToString() );
///    System.exit(1);
/// }
/// Console.WriteLine("server is alive.");
/// </code> <para>
/// By definition, the ONC/RPC ping call has program number 0 and expects no parameters and
/// replies with no result. Thus we just specify an empty parameter and result in the form of the
/// static <see cref="VoidXdrCodec.VoidXdrCodecInstance"/> object, when calling the ping procedure in 
/// the server using the <see cref="Call(int, IXdrCodec, IXdrCodec)"/> method. </para> <para>
/// For more complex and sometimes more useful ONC/RPC calls, you will need
/// to write appropriate ONC/RPC parameter and reply classes. Unfortunately, at this time there's
/// no compiler available to compile <c>.x</c> files, which define the XDR data structures, into 
/// appropriate classes. </para> <para>
/// For the next example, let's pretend our server provides the answer to all questions when
/// called with procedure number 42. Let's also pretend that this ONC/RPC call expects a question
/// in form of a string and returns the answer as an integer. So we need to define two classes,
/// one for the call's parameters and one for the reply. But let us first examine the class
/// containing a call's parameters: </para>
/// <code>
/// class StringCodec : XdrCodec 
/// {
///   public String question;
///   public void Encode(XdrEncodingStream encoder)
///   encoder.EncodeString(question);
/// }
/// public void Decode(XdrDecodingStream decoder)
/// {
///   question = decoder.DecodeString();
/// }
/// </code> <para>
/// The <c>StringCodec</c> class implements <see cref="IXdrCodec"/>, so instances
/// of it can be sent and received over the network using Sun's XDR protocol. What exactly is
/// sent over the wire is up to the two methods <see cref="IXdrCodec.Encode(XdrEncodingStreamBase)"/>
/// and <see cref="IXdrCodec.Decode(XdrDecodingStreamBase)"/>. The <see cref="IXdrCodec.Encode"/>
/// method encodes the data to be sent over the network, whereas <see cref="IXdrCodec.Decode"/>
/// restores the object's state from the data received over the network. In our example, 
/// these methods either send or receive a string. </para> <para>
/// The class defining the reply of our the-answer-to-all-questions ONC/RPC
/// call is now straightforward:</para>
/// <code>
/// class AnswerCodec : XdrCodec
/// {
///   public int DefinitiveAnswer { get; set; }
///   public void Encode(XdrEncodingStream encoder) {
///     encoder.EncodeInt(this.DefinitiveAnswer);
///   }
///   public void Decode(XdrDecodingStream decoder) {
///     this.DefinitiveAnswer = decoder.DecodeInt();
///   }
/// }
/// </code> <para>
/// Finally, to ask a question, you need to create the parameter object and fill it with the
/// parameters to be sent. Then create the object later receiving the reply. Finally issue the
/// ONC/RPC call: </para>
/// <code>
/// StringCodec parameters = new StringCoded();
/// parameters.Question = "What is the final answer to all our questions?";
/// AnswerCoded answer = new AnswerCodec();
/// try {
///   client.Call(42, parameters, answer);
/// } 
/// catch ( OncRpcException e ) {
/// } 
/// catch ( IOException e ) {
/// }
/// Console.WriteLine(parameters.question);
/// Console.WriteLine($"And the answer is: {answer.DefinitiveAnswer}");
/// </code> <para>
/// When you do not need the client proxy object any longer, you should return the resources
/// it occupies to the system. Use the <see cref="Close()"/> method for this.</para>
/// <code>
/// client.Close();
/// client = null; 
/// </code>
/// <see cref="OncRpcClientAuthBase">Authentication</see> can be done as follows:  <para>
/// just create an authentication object and hand it over to the ONC/RPC client object. </para>
/// <code>
/// OncRpcClientAuthBase auth = new OncRpcClientAuthUnix("marvin@ford.prefect", 42, 1001, new int[0]);
/// client.setAuth(auth);
/// </code> <para>
/// The <see cref="OncRpcClientAuthUnix"/> <see cref="OncRpcAuthType.OncRpcAuthTypeUnix"/>
/// will handle shorthand credentials (of type <see cref="OncRpcAuthType.OncRpcAuthTypeShortHandUnix"/>) transparently. If you do
/// not set any authentication object after creating an ONC/RPC client object, <see cref="OncRpcAuthType.OncRpcAuthTypeNone"/>
/// is used automatically. </para> <para>
/// TCP-based ONC/RPC clients also support call batching (exception handling
/// omitted for clarity): </para>
/// <code>
/// OncRpcTcpClient client = new OncRpcTcpClient(IPAddress.Loopback,
///                                              myProgramNumber, myProgramVersion,
///                                              OncRpcProtocols.OncRpcTCP);
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
/// client.Call(43, myFinalRequestCodec, myFinalResultCodec);
/// </code>
/// Remote Tea authors: Harald Albrecht, Jay Walters.
/// </remarks>
public abstract class OncRpcClientBase : IDisposable
{

    /// <summary>   (Immutable) the default buffer size. </summary>
    public const int DefaultBufferSize = 8192;

    /// <summary>   (Immutable) the default minimum buffer size. </summary>
    public const int DefaultMinBufferSize = 1024;

    /// <summary>   (Immutable) the default timeout. </summary>
    public const int DefaultTimeout = 30000;

    /// <summary>   (Immutable) the default transmission timeout. </summary>
    public const int DefaultTransmissionTimeout = 30000;

    #region " construction and cleanup "

    /// <summary>   Constructs an <see cref="OncRpcClientBase"/> object (the generic part). </summary>
    /// <remarks>
    /// If no port number is given (that is, <paramref name="port"/> is <c>0</c>), then a port lookup
    /// using the portmapper at <paramref name="host"/> is done.
    /// </remarks>
    /// <param name="host">     Host address where the desired ONC/RPC server resides. </param>
    /// <param name="program">  Program number of the desired ONC/RPC server. </param>
    /// <param name="version">  Version number of the desired ONC/RPC server. </param>
    /// <param name="port">     Port number of the ONC/RPC server. Specify <c>0</c>
    ///                         if this is not known and the portmap process located at host should
    ///                         be contacted to find out the port. </param>
    /// <param name="protocol"> <see cref="OncRpcProtocols">Protocol</see> to be used for
    ///                         ONC/RPC calls. This information is necessary, so port lookups through 
    ///                         the portmapper can be done. </param>
    internal OncRpcClientBase( IPAddress host, int program, int version, int port, OncRpcProtocols protocol )
    {
        // Set up the basics...
        this.Host = host;
        this.Program = program;
        this.Version = version;

        // Initialize the message identifier with some more-or-less random
        // value.
        long seed = DateTime.Now.Ticks;
        this.MessageId = ( int ) seed ^ ( int ) (seed >> (32 & 0x1f));

        // If the port number of the ONC/RPC server to contact is not yet
        // known, try to find it out. For this we need to contact the portmap
        // process at the given host and ask it for the desired program.

        // In case of tunneling through the HTTP protocol, we accept a port
        // number of zero and do not resolve it. This task is left up to
        // the other end of the HTTP tunnel (at the web server).
        if ( port == 0 && protocol != OncRpcProtocols.OncRpcHttp )
        {
            OncRpcPortmapClient portmap = new( host );
            try
            {
                port = portmap.GetPort( program, version, protocol );
            }
            finally
            {
                portmap.Close();
            }
        }
        this.Port = port;
    }

    /// <summary>
    /// Creates a new ONC/RPC client object, which can handle the requested
    /// <paramref name="protocol"/>.
    /// </summary>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="host">     Host address where the desired ONC/RPC server resides. </param>
    /// <param name="program">  Program number of the desired ONC/RPC server. </param>
    /// <param name="version">  Version number of the desired ONC/RPC server. </param>
    /// <param name="protocol"> <see cref="OncRpcProtocols">Protocol</see>
    ///                         to be used for ONC/RPC calls. </param>
    /// <returns>   An OncRpcClient. </returns>
    public static OncRpcClientBase NewOncRpcClient( IPAddress host, int program, int version, OncRpcProtocols protocol )
    {
        return NewOncRpcClient( host, program, version, 0, protocol );
    }

    /// <summary>
    /// Creates a new ONC/RPC client object, which can handle the requested
    /// <paramref name="protocol"/>.
    /// </summary>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="host">     Host address where the desired ONC/RPC server resides. </param>
    /// <param name="program">  Program number of the desired ONC/RPC server. </param>
    /// <param name="version">  Version number of the desired ONC/RPC server. </param>
    /// <param name="port">     Port number of the ONC/RPC server. Specify <c>0</c>
    ///                         if this is not known and the portmap process located at host should
    ///                         be contacted to find out the port. </param>
    /// <param name="protocol"> <see cref="OncRpcProtocols">Protocol</see>
    ///                         to be used for ONC/RPC calls. </param>
    /// <returns>   An OncRpcClient. </returns>
    public static OncRpcClientBase NewOncRpcClient( IPAddress host, int program, int version, int port, OncRpcProtocols protocol )
    {
        switch ( protocol )
        {
            case OncRpcProtocols.OncRpcUdp:
                {
                    // Now we need to create a protocol client object, which will know
                    // how to create the network connection and how to send and receive
                    // data to and from it.
                    return new OncRpcUdpClient( host, program, version, port );
                }

            case OncRpcProtocols.OncRpcTcp:
                {
                    return new OncRpcTcpClient( host, program, version, port );
                }

            default:
                {
                    throw new OncRpcException(
                                $"; expected {nameof( OncRpcProtocols.OncRpcUdp )}({OncRpcProtocols.OncRpcUdp}) or {nameof( OncRpcProtocols.OncRpcTcp )}({OncRpcProtocols.OncRpcTcp}); actual: {protocol}",
                                 OncRpcExceptionReason.OncRpcUnknownIpProtocol );
                }
        }
    }

    /// <summary>
    /// Closes the connection to an ONC/RPC server and free all network-related resources.
    /// </summary>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    public virtual void Close()
    {
    }

    #region " disposable implementation "

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
    /// resources.
    /// </summary>
    /// <remarks> 
    /// Takes account of and updates <see cref="IsDisposed"/>.
    /// Encloses <see cref="Dispose(bool)"/> within a try...finaly block.
    /// </remarks>
    public void Dispose()
    {
        if ( this.IsDisposed ) { return; }
        try
        {
            // Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            this.Dispose( true );

            // uncomment the following line if Finalize() is overridden above.
            GC.SuppressFinalize( this );
        }
        finally
        {
            this.IsDisposed = true;
        }
    }

    /// <summary>   Gets or sets a value indicating whether this object is disposed. </summary>
    /// <value> True if this object is disposed, false if not. </value>
    protected bool IsDisposed { get; private set; }

    /// <summary>
    /// Releases the unmanaged resources used by the XdrDecodingStreamBase and optionally releases
    /// the managed resources.
    /// </summary>
    /// <param name="disposing">    True to release both managed and unmanaged resources; false to
    ///                             release only unmanaged resources. </param>
    protected virtual void Dispose( bool disposing )
    {
        if ( disposing )
        {
            // dispose managed state (managed objects)
        }

        // free unmanaged resources and override finalizer
        // I am assuming that the socket used in the derived classes include unmanaged resources.
        this.Close();

        // set large fields to null
    }

    /// <summary>   Finalizer. </summary>
    ~OncRpcClientBase()
    {
        if ( this.IsDisposed ) { return; }
        this.Dispose( false );
    }

    #endregion

    #endregion

    #region " actions "

    /// <summary>   Calls a remote procedure on an ONC/RPC server. </summary>
    /// <remarks>
    /// The <see cref="OncRpcUdpClient"/> uses a similar timeout scheme as
    /// the genuine Sun C implementation of ONC/RPC: it starts with a timeout of one second when
    /// waiting for a reply. If no reply is received within this time frame, the client doubles the
    /// timeout, sends a new request and then waits again for a reply. In every case the client will
    /// wait no longer than the total timeout set through the <see cref="Timeout"/> property.
    /// </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="procedureNumber">  Procedure number of the procedure to call. </param>
    /// <param name="requestCodec">     The XDR codec that is sent to the procedure call. </param>
    /// <param name="replyCodec">       The XDR codec that receives the result of the procedure call. </param>
    public virtual void Call( int procedureNumber, IXdrCodec requestCodec, IXdrCodec replyCodec )
    {
        lock ( this )
            // Use the default version number as specified for this client.
            this.Call( procedureNumber, this.Version, requestCodec, replyCodec );
    }

    /// <summary>   Calls a remote procedure on an ONC/RPC server. </summary>
    /// <remarks>   2023-01-03. </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="procedureNumber">  Procedure number of the procedure to call. </param>
    /// <param name="versionNumber">    Protocol version number. </param>
    /// <param name="requestCodec">     The XDR codec that is sent to the procedure call. </param>
    /// <param name="replyCodec">       The XDR codec that receives the result of the procedure call. </param>
    public abstract void Call( int procedureNumber, int versionNumber, IXdrCodec requestCodec, IXdrCodec replyCodec );

    #endregion

    #region " members "

    /// <summary>
    /// Gets or sets the timeout (in milliseconds) for communication with an ONC/RPC server.
    /// </summary>
    /// <remarks>
    /// The <see cref="Call(int, IXdrCodec, IXdrCodec)"/> method will throw a <see cref="ThreadInterruptedException"/>
    /// exception if no answer from the ONC/RPC server is received within the timeout time span. <para>
    /// The default timeout value is 30 seconds (30,000 milliseconds). </para><para>
    /// The timeout must be non-negative. A timeout of zero indicated batched calls, for which no
    /// reply message is expected. </para>
    /// </remarks>
    /// <value> The timeout in milliseconds. </value>
    public int Timeout { get; set; } = DefaultTimeout;

    /// <summary>
    /// Gets or sets (private) the program number of the ONC/RPC server to communicate with.
    /// </summary>
    /// <value> The ONC/RPC program number. </value>
    public int Program { get; private set; }

    /// <summary>   Gets or sets (private) the version number of the ONC/RPC program. </summary>
    /// <value> The ONC/RPC version number of ONC/RPC program.. </value>
    public int Version { get; private set; }

    /// <summary>
    /// Gets or sets the Internet address of the host where the ONC/RPC server we want to communicate
    /// with is located.
    /// </summary>
    /// <value> The IP address of the ONC/RPC server. </value>
    public IPAddress Host { get; private set; }

    /// <summary>
    /// Get or set (private) the port number at which the ONC/RPC server can be contacted.
    /// </summary>
    /// <value> The port number of ONC/RPC server.. </value>
    public int Port { get; private set; }

    /// <summary>
    /// Get or set the <see cref="OncRpcClientAuthBase">Authentication protocol</see> object to be used
    /// when issuing ONC/RPC calls.
    /// </summary>
    /// <value>
    /// The authentication protocol handling object encapsulating authentication information.
    /// </value>
    public OncRpcClientAuthBase Auth { get; set; }

    /// <summary>   Gets or sets the default encoding. </summary>
    /// <remarks>
    /// The default encoding for VXI-11 is <see cref="Encoding.ASCII"/>, which is a subset of <see cref="Encoding.UTF8"/>
    /// </remarks>
    /// <value> The default encoding. </value>
    public static Encoding DefaultEncoding { get; set; } = Encoding.UTF8;

    /// <summary>
    /// Gets or sets the encoding to use when serializing strings. 
    /// </summary>
    /// <value> The character encoding. </value>
    public virtual Encoding CharacterEncoding { get; set; } = XdrDecodingStreamBase.DefaultEncoding;

    /// <summary>   Create next transaction (message) identifier. </summary>
    internal virtual void NextTransactionId()
    {
        this.MessageId++;
    }

    /// <summary>
    /// Gets or sets or set (private) the message id (also sometimes known as "transaction id") used
    /// for the next call message.
    /// </summary>
    /// <remarks>
    /// The message (transaction) identifiers are used to match corresponding ONC/RPC call and reply messages.
    /// </remarks>
    /// <value> The identifier of the next transaction. </value>
    internal int MessageId { get; private set; }

    #endregion

}
