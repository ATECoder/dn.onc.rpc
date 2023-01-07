using cc.isr.ONC.RPC.Client;
using cc.isr.ONC.RPC.Codecs;
using cc.isr.XDR.Codecs;

namespace cc.isr.ONC.RPC.Portmap;

/// <summary>
/// The class <see cref="OncRpcPortmapClient"/> is a specialized ONC/RPC client, which can talk to
/// the portmapper on a given host using the famous UDP/IP datagram-oriented Internet protocol.
/// </summary>
/// <remarks>
/// In addition, it is also possible to contact port mappers using TCP/IP. For this, the
/// constructor of the
/// <see cref="OncRpcPortmapClient"/> class also accepts a protocol parameter
/// (<see cref="OncRpcPortmapClient(IPAddress, int)"/>).
/// Technically spoken, instances of <see cref="OncRpcPortmapClient"/> are proxy objects.
/// <see cref="OncRpcPortmapClient"/> objects currently speak protocol version
/// 2. The newer transport-independent protocol versions 3 and 4 are
/// <b>not</b> supported as the transport-independent ONC/RPC implementation is not
/// that widely in use due to the brain-damaged design of XTI. If you should
/// ever have programmed using XTI (transport independent interface) then you'll
/// know what I mean and probably agree with me. Otherwise, in case you find XTI
/// the best thing since the Win32 API, please implement the 
/// <see href="https://www.freesoft.org/CIE/RFC/1833/2.htm">RPCBIND</see> program protocol
/// versions 3 and 4 and give it to the community -- thank you. <para>
/// Here are some simple examples of how to use the portmapper proxy object.
/// We first start with one of the most interesting operations, which can be
/// performed on port mappers, querying the port of a local or remote ONC/RPC
/// server. </para> <para>
/// To query the port number of an ONC/RPC server, we need to contact the
/// portmapper at the host machine where the server is running. The following
/// code snippet just contacts the local portmapper. <see langword="try"/> blocks
/// are omitted for brevity -- but remember that you almost always need to catch
/// <see cref="OncRpcException"/> as well as <see cref="System.IO.IOException"/>. </para>
/// <code>
/// OncRpcPortmapClient portmap = new OncRpcPortmapClient( IPAddress.Loopback );
/// </code> <para>
/// With the portmapper proxy object in our hands we can now ask for the port number of a
/// particular ONC/RPC server. In this (fictitious) example we ask for the ONC/RPC program
/// (server) number <c>0x49678</c> (by coincidence this happens to be the program number of
/// the <a href="http://www.acplt.org/ks">ACPLT/KS</a>
/// protocol). To ask for the port number of a given program number, use the
/// <see cref="GetPort(int, int, int)"/> </para> method.
/// <code>
/// int port;
/// try {
///   port = portmap.GetPort( 0x49678, 1, OncRpcProtocols.ONCRPC_UDP );
/// } 
/// catch ( OncRpcProgramNotRegisteredException e ) {
///   Console.WriteLine("ONC/RPC program server not found");
///   System.exit(0);
/// } 
/// catch ( OncRpcException e ) {
///   Console.WriteLine( "Could not contact portmapper:" );
///   Console.WriteLine( e.ToString() );
///   System.exit( 0 );
/// }
/// Console.WriteLine( $"Program available at port {port}" );
/// </code> <para>
/// In the call to <see cref="GetPort(int, int, int)"/>, the first parameter
/// specifies the ONC/RPC program number, the second parameter specifies the program's version
/// number, and the third parameter specifies the IP protocol to use when issuing ONC/RPC calls.
/// Currently, only <see cref="OncRpcProtocols.OncRpcUdp"/> and <see cref="OncRpcProtocols.OncRpcTcp"/>
/// are supported. But who needs other protocols anyway?! </para> <para>
/// In case <see cref="GetPort(int, int, int)"/>
/// succeeds, it returns the number of the port where the appropriate ONC/RPC server waits for
/// incoming ONC/RPC calls. If the ONC/RPC program is not registered with the particular ONC/RPC
/// portmapper, an <see cref="OncRpcProgramNotRegisteredException"/>
/// is thrown (which is a subclass of <see cref="OncRpcException"/>
/// with a <see cref="OncRpcException.Reason"/> of <see cref="OncRpcException.OncRpcProgramNotRegistered"/>. </para> <para>
/// A second typical example of how to use the portmapper is retrieving a list of the
/// currently registered servers. We use the <see cref="ListServers()"/>
/// method for this purpose in the following example, and print the list we got. </para>
/// <code>
/// OncRpcServerIdent [] list = null;
/// try {
///   list = portmap.ListServers();
/// } 
/// catch ( OncRpcException e ) {
///   Console.WriteLine( e.ToString() );
///   System.exit( 20 );
/// }
/// foreach ( var item in list ) {
///   Console.WriteLine( $"{item.program} {item.version} {item.protocol} {item.port}" );
/// }
/// </code> <para>
/// When you do not need the client proxy object any longer, you should return the resources
/// it occupies to the system. Use the <see cref="Close()"/> method for this. </para>
/// <code>
/// portmap.Close();
/// portmap = null; // Hint to the garbage collector
/// </code> <para>
/// For another code example, please consult 
/// <see href="../oncrpc.mstest/PortMapper/PortmapGetPortTest.cs"/> </para> <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public class OncRpcPortmapClient
{
    /// <summary>
    /// Constructs and initializes an ONC/RPC client object, which can communicate with the
    /// portmapper at the specified host using the UDP/IP datagram-oriented Internet protocol.
    /// </summary>
    /// <param name="host"> Host where to contact the portmapper. </param>
    ///
    /// <exception cref="OncRpcException">          Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="System.IO.IOException">    Thrown when an I/O error condition occurs. </exception>
    public OncRpcPortmapClient( IPAddress host ) : this( host, OncRpcProtocols.OncRpcUdp, 0 )
    {
    }

    /// <summary>
    /// Constructs and initializes an ONC/RPC client object, which can communicate with the
    /// portmapper at the given host using the specified protocol.
    /// </summary>
    /// <param name="host">     Host where to contact the portmapper. </param>
    /// <param name="protocol"> Protocol to use for contacting the portmapper. This can be either 
    ///                         <see cref="OncRpcProtocols.OncRpcUdp"/> or
    ///                         <see cref="OncRpcProtocols.OncRpcTcp"/> (HTTP is currently
    ///                         not supported). </param>
    ///
    /// <exception cref="OncRpcException">          Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="System.IO.IOException">    Thrown when an I/O error condition occurs. </exception>
    public OncRpcPortmapClient( IPAddress host, int protocol ) : this( host, protocol, -1 )
    {
    }

    /// <summary>
    /// Constructs and initializes an ONC/RPC client object, which can communicate with the
    /// portmapper at the given host using the specified protocol.
    /// </summary>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="host">     Host where to contact the portmapper. </param>
    /// <param name="protocol"> Protocol to use for contacting the portmapper. This can be either
    ///                         <see cref="OncRpcProtocols.OncRpcUdp"/> or
    ///                         <see cref="OncRpcProtocols.OncRpcTcp"/> (HTTP is currently
    ///                         not supported). </param>
    /// <param name="timeout">  Timeout in milliseconds for connection operation. This parameter
    ///                         applies only when using TCP/IP for talking to the portmapper. A
    ///                         negative timeout indicates that the implementation-specific timeout
    ///                         setting of the <see cref="System.Net.Sockets.Socket"/>, which defaults to <c>0</c>,
    ///                         should be used instead. </param>
    ///
    /// <exception cref="OncRpcException">          Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="System.IO.IOException">    Thrown when an I/O error condition occurs. </exception>
    public OncRpcPortmapClient( IPAddress host, int protocol, int timeout )
    {
        switch ( protocol )
        {
            case OncRpcProtocols.OncRpcUdp:
                {
                    this.PortmapClient = new OncRpcUdpClient( host, OncRpcPortmapConstants.OncRpcPortmapProgramNumber,
                                                              OncRpcPortmapConstants.OncRpcPortmapProgramVersionNumber,
                                                              OncRpcPortmapConstants.OncRpcPortmapPortNumber );
                    break;
                }

            case OncRpcProtocols.OncRpcTcp:
                {
                    this.PortmapClient = new OncRpcTcpClient( host, OncRpcPortmapConstants.OncRpcPortmapProgramNumber,
                                                              OncRpcPortmapConstants.OncRpcPortmapProgramVersionNumber,
                                                              OncRpcPortmapConstants.OncRpcPortmapPortNumber, 0, timeout );
                    // default buff size
                    break;
                }

            default:
                {
                    throw new OncRpcException( OncRpcException.OncRpcUnknownIpProtocol );
                }
        }
    }

    /// <summary>   Closes the connection to the portmapper. </summary>
    public virtual void Close()
    {
        this.PortmapClient.Close();
    }

    /// <summary>
    /// Gets or sets the particular transport-specific ONC/RPC client used for communicating with the
    /// portmapper.
    /// </summary>
    /// <value> The portmap client proxy object (subclass of <see cref="OncRpcClientBase"/>). </value>
    public OncRpcClientBase PortmapClient { get; set; }

    /// <summary>
    /// Asks the portmapper this <see cref="OncRpcPortmapClient"/> object is a proxy for, for the port
    /// number of a particular ONC/RPC server identified by the information tuple {program number,
    /// program version, protocol}.
    /// </summary>
    /// <exception cref="OncRpcException">                      if the portmapper is not available
    ///                                                         (reason is <see cref="OncRpcException.OncRpcPortMapServiceFailure"/> ). </exception>
    /// <exception cref="OncRpcProgramNotRegisteredException">  if the requested program is not available. </exception>
    /// <param name="program">  Program number of the remote procedure call in question. </param>
    /// <param name="version">  Program version number. </param>
    /// <param name="protocol"> Protocol later on used for communication with the ONC/RPC server in
    ///                         question. This can be one of the protocols constants defined in the
    ///                         <see cref="OncRpcProtocols"/> interface. </param>
    /// <returns>   port number of ONC/RPC server in question. </returns>
    public virtual int GetPort( int program, int version, int protocol )
    {
        // Fill in the request parameters. Note that requestCodec.Port is
        // not used. BTW - it is automatically initialized as 0 by the
        // constructor of the OncRpcServerIdentifierCodec class.
        OncRpcServerIdentifierCodec requestCodec = new( program, version, protocol, 0 );
        OncRpcGetPortCodec replyCodec = new();

        // Try to contact the portmap process. If something goes "boing"
        // at this stage, then rethrow the exception as a generic portmap
        // failure exception. Otherwise, if the port number returned is
        // zero, then no appropriate server was found. In this case,
        // throw an exception, that the program requested could not be
        // found.
        try
        {
            this.PortmapClient.Call( OncRpcPortmapServiceProcedure.OncRpcPortmapGetPortNumber, requestCodec, replyCodec );
        }
        catch ( OncRpcException )
        {
            throw new OncRpcException( OncRpcException.OncRpcPortMapServiceFailure );
        }

        // In case the program is not registered, throw an exception too.
        return replyCodec.Port == 0
            ? throw new OncRpcProgramNotRegisteredException()
            : replyCodec.Port;
    }

    /// <summary>
    /// Register an ONC/RPC with the given program number, version and protocol at the given port
    /// with the portmapper.
    /// </summary>
    /// <exception cref="OncRpcException">  if the portmapper is not available (reason is
    ///                                     <see cref="OncRpcException.OncRpcPortMapServiceFailure"/>). </exception>
    /// <param name="program">  The number of the program to be registered. </param>
    /// <param name="version">  The version number of the program. </param>
    /// <param name="protocol"> The protocol spoken by the ONC/RPC server. Can be one of the
    ///                         <see cref="OncRpcProtocols"/> constants. </param>
    /// <param name="port">     The port number where the ONC/RPC server can be reached. </param>
    /// <returns>
    /// Indicates whether registration succeeded (<see cref="T:true"/>) or was denied by the portmapper
    /// (<see cref="T:false"/>).
    /// </returns>
    public virtual bool SetPort( int program, int version, int protocol, int port )
    {
        // Fill in the request parameters.
        OncRpcServerIdentifierCodec requestCodec = new( program, version, protocol, port );
        BooleanXdrCodec resultCodec = new( false );

        // Try to contact the portmap process. If something goes "boing"
        // at this stage, then rethrow the exception as a generic portmap
        // failure exception.
        try
        {
            this.PortmapClient.Call( OncRpcPortmapServiceProcedure.OncRpcPortmapRegisterServer, requestCodec, resultCodec );
        }
        catch ( OncRpcException )
        {
            throw new OncRpcException( OncRpcException.OncRpcPortMapServiceFailure );
        }
        return resultCodec.Value;
    }

    /// <summary>   Unregister an ONC/RPC with the given program number and version. </summary>
    /// <remarks>
    /// The portmapper will remove all entries with the same program number and version, regardless
    /// of the protocol and port number.
    /// </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="program">  The number of the program to be unregistered. </param>
    /// <param name="version">  The version number of the program. </param>
    /// <returns>
    /// Indicates whether deregistration succeeded (<see cref="T:true"/>)
    /// or was denied by the portmapper (<see cref="T:false"/>).
    /// </returns>
    /// <exception cref="OncRpcException">  if the portmapper is not available (reason is
    ///                                     <see cref="OncRpcException.OncRpcPortMapServiceFailure"/>). </exception>
    public virtual bool UnsetPort( int program, int version )
    {
        // Fill in the request codec.
        OncRpcServerIdentifierCodec requestCodec = new( program, version, 0, 0 );
        BooleanXdrCodec replyCodec = new( false );

        // Try to contact the portmap process. If something goes "boing"
        // at this stage, then rethrow the exception as a generic portmap
        // failure exception.
        try
        {
            this.PortmapClient.Call( OncRpcPortmapServiceProcedure.OncRpcPortmapUnregisterServer, requestCodec, replyCodec );
        }
        catch ( OncRpcException e )
        {
            // Temp output
            Console.WriteLine( e.Message );
            Console.WriteLine( e.StackTrace );
            throw new OncRpcException( OncRpcException.OncRpcPortMapServiceFailure );
        }
        return replyCodec.Value;
    }

    /// <summary>
    /// Retrieves a list of all registered ONC/RPC servers at the same host as the contacted
    /// portmapper.
    /// </summary>
    /// <remarks>   2022-12-24. </remarks>
    /// <exception cref="OncRpcException">  if the portmapper is not available with 
    ///                                     <see cref="OncRpcException.Reason"/> = <see cref="OncRpcException.OncRpcPortMapServiceFailure"/>). 
    ///                                     </exception>
    /// <returns>
    /// vector of server descriptions <see cref="OncRpcServerIdentifierCodec"/>.
    /// </returns>
    public virtual OncRpcServerIdentifierCodec[] ListServers()
    {
        // Fill in the request parameters.
        OncRpcPortmapServersListCodec result = new();

        // Try to contact the portmap process. On failure, rethrow the exception 
        // as a generic portmap failure exception.
        try
        {
            this.PortmapClient.Call( OncRpcPortmapServiceProcedure.OncRpcPortmapListServersInfo, VoidXdrCodec.VoidXdrCodecInstance, result );
        }
        catch ( OncRpcException )
        {
            throw new OncRpcException( OncRpcException.OncRpcPortMapServiceFailure );
        }

        // Copy the server identities from the Vector into the vector (array).
        OncRpcServerIdentifierCodec[] info = new OncRpcServerIdentifierCodec[result.ServerIdentifiers.Count];
        result.ServerIdentifiers.CopyTo( info );
        return info;
    }

    /// <summary>   Pings the portmapper (try to call procedure 0). </summary>
    /// <exception cref="OncRpcException">  if the portmapper is not available (reason is
    ///                                     <see cref="OncRpcException.OncRpcPortMapServiceFailure"/> ). </exception>
    public virtual void Ping()
    {
        try
        {
            this.PortmapClient.Call( 0, VoidXdrCodec.VoidXdrCodecInstance, VoidXdrCodec.VoidXdrCodecInstance );
        }
        catch ( OncRpcException )
        {
            throw new OncRpcException( OncRpcException.OncRpcPortMapServiceFailure );
        }
    }

}