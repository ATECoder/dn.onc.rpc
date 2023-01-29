using cc.isr.ONC.RPC.Logging;
using cc.isr.ONC.RPC.Client;
using cc.isr.ONC.RPC.Codecs;

namespace cc.isr.ONC.RPC.Portmap;

/// <summary>
/// The class <see cref="OncRpcPortmapClient"/> is a specialized ONC/RPC client, which can talk to
/// the portmapper on a given host using the famous UDP/IP datagram-oriented Internet protocol.
/// </summary>
/// <remarks>
/// In addition, it is also possible to contact port mappers using TCP/IP. For this, the
/// constructor of the
/// <see cref="OncRpcPortmapClient"/> class also accepts a protocol parameter
/// (<see cref="OncRpcPortmapClient(IPAddress, OncRpcProtocol, int)"/>).
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
/// 
/// Here are some simple examples of how to use the portmapper proxy object.
/// We first start with one of the most interesting operations, which can be
/// performed on port mappers, querying the port of a local or remote ONC/RPC
/// server. </para> <para>
/// 
/// To query the port number of an ONC/RPC server, we need to contact the
/// portmapper at the host machine where the server is running. The following
/// code snippet just contacts the local portmapper. <see langword="try"/> blocks
/// are omitted for brevity -- but remember that you almost always need to catch
/// <see cref="OncRpcException"/>. </para>
/// <code>
/// using OncRpcPortmapClient portmap = new OncRpcPortmapClient( IPAddress.Loopback );
/// </code> <para>
/// 
/// With the portmapper proxy object in our hands we can now ask for the port number of a
/// particular ONC/RPC server. In this (fictitious) example we ask for the ONC/RPC program
/// (server) number <c>0x49678</c> (by coincidence this happens to be the program number of
/// the <a href="http://www.acplt.org/ks">ACPLT/KS</a>
/// protocol). To ask for the port number of a given program number, use the
/// <see cref="GetPort(int, int, OncRpcProtocol)"/> </para> method.
/// <code>
/// int port;
/// try {
///   port = portmap.GetPort( 0x49678, 1, OncRpcProtocols.ONCRPC_UDP );
/// } 
/// catch ( OncRpcProgramNotRegisteredException e ) {
///   Logger.Writer.LogInformation( "ONC/RPC program server not found" );
///   System.exit(0);
/// } 
/// catch ( OncRpcException e ) {
///   Logger.Writer.LogMemberError( "Could not contact portmapper:", e );
///   System.exit( 0 );
/// }
/// Logger.Writer.LogInformation( $"Program available at port {port}" );
/// </code> <para>
/// 
/// In the call to <see cref="GetPort(int, int, OncRpcProtocol)"/>, the first parameter
/// specifies the ONC/RPC program number, the second parameter specifies the program's version
/// number, and the third parameter specifies the IP protocol to use when issuing ONC/RPC calls.
/// Currently, only <see cref="OncRpcProtocol.OncRpcUdp"/> and <see cref="OncRpcProtocol.OncRpcTcp"/>
/// are supported. But who needs other protocols anyway?! </para> <para>
/// 
/// In case <see cref="GetPort(int, int, OncRpcProtocol)"/>
/// succeeds, it returns the number of the port where the appropriate ONC/RPC server waits for
/// incoming ONC/RPC calls. If the ONC/RPC program is not registered with the particular ONC/RPC
/// portmapper, an <see cref="OncRpcExceptionReason.OncRpcProgramNotRegistered"/>
/// is thrown (which is a subclass of <see cref="OncRpcException"/>
/// with a <see cref="OncRpcException.Reason"/> of <see cref="OncRpcExceptionReason.OncRpcProgramNotRegistered"/>. </para> <para>
/// 
/// A second typical example of how to use the portmapper is retrieving a list of the
/// currently registered servers. We use the <see cref="ListRegisteredServers()"/>
/// method for this purpose in the following example, and print the list we got. </para>
/// <code>
/// OncRpcServerIdent [] list = null;
/// try {
///   list = portmap.ListRegisteredServers();
/// } 
/// catch ( OncRpcException e ) {
///   Logger.Writer.LogMemberError( "error listing registered servers", e );
///   System.exit( 20 );
/// }
/// foreach ( var item in list ) {
///   Logger.Writer.LogInformation( $"{item.program} {item.version} {item.protocol} {item.port}" );
/// }
/// </code> <para>
/// 
/// When you do not need the client proxy object any longer, you should return the resources
/// it occupies to the system. Use the <see cref="Close()"/> method for this. </para>
/// <code>
/// portmap.Close();
/// portmap = null; // Hint to the garbage collector
/// </code> <para>
/// 
/// For another code example, please consult 
/// <see href="../oncrpc.mstest/PortMapper/PortmapGetPortTest.cs"/> </para> <para>
/// 
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public class OncRpcPortmapClient : IDisposable
{

    /// <summary>   Gets or sets the TCP connect timeout default. </summary>
    /// <value> The TCP connect timeout default. </value>
    public static int ConnectTimeoutDefault { get; set; } = 3000;

    #region " construction and cleanup "

    /// <summary>
    /// Constructs and initializes an ONC/RPC client object, which can communicate with the
    /// portmapper at the given host using the specified protocol.
    /// </summary>
    /// <remarks>   2023-01-23. </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="host">     Host where to contact the portmapper. </param>
    /// <param name="protocol"> Protocol to use for contacting the portmapper. This can be either
    ///                         <see cref="OncRpcProtocol.OncRpcUdp"/> or
    ///                         <see cref="OncRpcProtocol.OncRpcTcp"/> (HTTP is currently
    ///                         not supported). </param>
    /// <param name="timeout">  This is the connect timeout in milliseconds for TCP/IP connection
    ///                         operation or UDP/IP transmission. The I/O for UDP and TCP connection
    ///                         is set to the default <see cref="OncRpcTcpClient"/> and <see cref="OncRpcUdpClient"/>
    ///                         default values. </param>
    public OncRpcPortmapClient( IPAddress host, OncRpcProtocol protocol, int timeout )
    {
        switch ( protocol )
        {
            case OncRpcProtocol.OncRpcUdp:
                {
                    this.OncRpcClient = new OncRpcUdpClient( host, OncRpcPortmapConstants.OncRpcPortmapProgramNumber,
                                                              OncRpcPortmapConstants.OncRpcPortmapProgramVersionNumber,
                                                              OncRpcPortmapConstants.OncRpcPortmapPortNumber, OncRpcUdpClient.BufferSizeDefault, timeout ) {
                        IOTimeout = OncRpcUdpClient.IOTimeoutDefault
                    };
                    break;
                }

            case OncRpcProtocol.OncRpcTcp:
                {
                    this.OncRpcClient = new OncRpcTcpClient( host, OncRpcPortmapConstants.OncRpcPortmapProgramNumber,
                                                              OncRpcPortmapConstants.OncRpcPortmapProgramVersionNumber,
                                                              OncRpcPortmapConstants.OncRpcPortmapPortNumber, 0, timeout ) {
                        IOTimeout = OncRpcTcpClient.IOTimeoutDefault,
                        TransmitTimeout = OncRpcTcpClient.TransmitTimeoutDefault
                    };
                    break;
                }

            default:
                {
                    throw new OncRpcException(
                                $"; expected {nameof( OncRpcProtocol.OncRpcUdp )}({OncRpcProtocol.OncRpcUdp}) or {nameof( OncRpcProtocol.OncRpcTcp )}({OncRpcProtocol.OncRpcTcp}); actual: {protocol}",
                                OncRpcExceptionReason.OncRpcUnknownIpProtocol );
                }
        }
    }

    /// <summary>   Closes the connection to the portmapper. </summary>
    public virtual void Close()
    {
        this.OncRpcClient?.Close();
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
        catch ( Exception ex ) { Logger.Writer.LogMemberError( "Exception disposing", ex ); }
        finally
        {
            this.IsDisposed = true;
        }
    }

    /// <summary>   Gets or sets a value indicating whether this object is disposed. </summary>
    /// <value> True if this object is disposed, false if not. </value>
    protected bool IsDisposed { get; private set; }

    /// <summary>
    /// Releases unmanaged, large objects and (optionally) managed resources used by this class.
    /// </summary>
    /// <param name="disposing">    True to release large objects and managed and unmanaged resources;
    ///                             false to release only unmanaged resources and large objects. </param>
    protected virtual void Dispose( bool disposing )
    {
        if ( disposing )
        {
            // dispose managed state (managed objects)
        }

        // free unmanaged resources and override finalizer
        // I am assuming that the socket used in the derived classes include unmanaged resources.
        this.Close();

        // dispose of the portmap client.
        this.OncRpcClient?.Dispose();

        // set large fields to null
    }

    /// <summary>   Finalizer. </summary>
    ~OncRpcPortmapClient()
    {
        if ( this.IsDisposed ) { return; }
        this.Dispose( false );
    }

    #endregion

    #endregion

    #region " members "

    /// <summary>
    /// Gets or sets the particular transport-specific ONC/RPC client used for communicating with the
    /// portmapper.
    /// </summary>
    /// <value> The portmap client proxy object (subclass of <see cref="OncRpcClientBase"/>). </value>
    public OncRpcClientBase OncRpcClient { get; set; }

    #endregion

    #region " actions "

    /// <summary>
    /// Asks the portmapper this <see cref="OncRpcPortmapClient"/> object is a proxy for, for the port
    /// number of a particular ONC/RPC server identified by the information tuple {program number,
    /// program version, protocol}.
    /// </summary>
    /// <exception cref="OncRpcException">  Thrown if the requested program is not available. </exception>
    /// <param name="program">  Program number of the remote procedure call in question. </param>
    /// <param name="version">  Program version number. </param>
    /// <param name="protocol"> Protocol later on used for communication with the ONC/RPC server in
    ///                         question. This can be one of the protocols constants defined in the
    ///                         <see cref="OncRpcProtocol"/> interface. </param>
    /// <returns>   port number of ONC/RPC server in question. </returns>
    public virtual int GetPort( int program, int version, OncRpcProtocol protocol )
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

        // @atecode: re-throwing an OncRcpException( reason: OncRpcPortMapServiceFailure ) exception was changed
        // in favor of letting any exception pass through assuming that the stack trace will reveal the Portmap service
        // as the end point for these exceptions.

        this.OncRpcClient.Call( ( int ) OncRpcPortmapServiceProcedure.OncRpcPortmapGetPortNumber, requestCodec, replyCodec );

        // In case the program is not registered, throw an exception too.
        // @atecode: the specific 'Program Not Registered' exception was removed in favor of
        // keeping things simple and adding a suffix message.
        return replyCodec.Port == 0
            ? throw new OncRpcException( "; the Portmap service returned a port number 0 where a non-zero port, e.g., 1024 for VXI-11, was expected",
                                         OncRpcExceptionReason.OncRpcProgramNotRegistered )
            : replyCodec.Port;
    }

    /// <summary>
    /// Register an ONC/RPC with the given program number, version and protocol at the given port
    /// with the portmapper.
    /// </summary>
    /// <param name="program">  The number of the program to be registered. </param>
    /// <param name="version">  The version number of the program. </param>
    /// <param name="protocol"> The protocol spoken by the ONC/RPC server. Can be one of the
    ///                         <see cref="OncRpcProtocol"/> constants. </param>
    /// <param name="port">     The port number where the ONC/RPC server can be reached. </param>
    /// <returns>
    /// Indicates whether registration succeeded (<see langword="true"/>) or was denied by the portmapper
    /// (<see langword="false"/>).
    /// </returns>
    public virtual bool SetPort( int program, int version, OncRpcProtocol protocol, int port )
    {
        // Fill in the request parameters.
        OncRpcServerIdentifierCodec requestCodec = new( program, version, protocol, port );
        BooleanXdrCodec resultCodec = new( false );

        // Try to contact the portmap process. If something goes "boing"
        // at this stage, then rethrow the exception as a generic portmap
        // failure exception.

        // @atecode: re-throwing an OncRcpException( reason: OncRpcPortMapServiceFailure ) exception was changed
        // in favor of letting any exception pass through assuming that the stack trace will reveal the Portmap service
        // as the end point for these exceptions.

        this.OncRpcClient.Call( ( int ) OncRpcPortmapServiceProcedure.OncRpcPortmapRegisterServer, requestCodec, resultCodec );

        return resultCodec.Value;
    }

    /// <summary>   Unregister an ONC/RPC with the given program number and version. </summary>
    /// <remarks>
    /// The portmapper will remove all entries with the same program number and version, regardless
    /// of the protocol and port number.
    /// </remarks>
    /// <param name="program">  The number of the program to be unregistered. </param>
    /// <param name="version">  The version number of the program. </param>
    /// <returns>
    /// Indicates whether deregistration succeeded (<see langword="true"/>)
    /// or was denied by the portmapper (<see langword="false"/>).
    /// </returns>
    public virtual bool UnsetPort( int program, int version )
    {
        // Fill in the request codec.
        OncRpcServerIdentifierCodec requestCodec = new( program, version, OncRpcProtocol.NotSpecified, 0 );
        BooleanXdrCodec replyCodec = new( false );

        // Try to contact the portmap process. If something goes "boing"
        // at this stage, then rethrow the exception as a generic portmap
        // failure exception.

        // @atecode: re-throwing an OncRcpException( reason: OncRpcPortMapServiceFailure ) exception was changed
        // in favor of letting any exception pass through assuming that the stack trace will reveal the Portmap service
        // as the end point for these exceptions.

        this.OncRpcClient.Call( ( int ) OncRpcPortmapServiceProcedure.OncRpcPortmapUnregisterServer, requestCodec, replyCodec );

        return replyCodec.Value;
    }

    /// <summary>
    /// Retrieves a list of all registered ONC/RPC servers at the same host as the contacted
    /// portmapper.
    /// </summary>
    /// <returns>
    /// vector of server descriptions <see cref="OncRpcServerIdentifierCodec"/>.
    /// </returns>
    public virtual OncRpcServerIdentifierCodec[] ListRegisteredServers()
    {
        // Fill in the request parameters.
        OncRpcPortmapServersListCodec result = new();

        // Try to contact the portmap process. On failure, rethrow the exception 
        // as a generic portmap failure exception.

        // @atecode: re-throwing an OncRcpException( reason: OncRpcPortMapServiceFailure ) exception was changed
        // in favor of letting any exception pass through assuming that the stack trace will reveal the Portmap service
        // as the end point for these exceptions.

        this.OncRpcClient.Call( ( int ) OncRpcPortmapServiceProcedure.OncRpcPortmapListRegisteredServers, VoidXdrCodec.VoidXdrCodecInstance, result );

        // Copy the server identities from the Vector into the vector (array).
        // OncRpcServerIdentifierCodec[] serverIdentifiers = new OncRpcServerIdentifierCodec[result.ServerIdentifiers.Count];
        // result.ServerIdentifiers.CopyTo( serverIdentifiers );
        return result.ServerIdentifiers.ToArray();
    }

    /// <summary>   Pings the Portmap service by calling the null procedure (0). </summary>
    public virtual void PingPortmapService()
    {
        // @atecode: re-throwing an OncRcpException( reason: OncRpcPortMapServiceFailure ) exception was changed
        // in favor of letting any exception pass through assuming that the stack trace will reveal the Portmap service
        // as the end point for these exceptions.

        this.OncRpcClient.Call( ( int ) OncRpcPortmapServiceProcedure.OncRpcPortmapPing, VoidXdrCodec.VoidXdrCodecInstance, VoidXdrCodec.VoidXdrCodecInstance );
    }

    /// <summary>   Attempts to ping the Portmap service by calling the null procedure (0). </summary>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    public virtual bool TryPingPortmapService()
    {
        try
        {
            this.PingPortmapService();
        }
        catch ( Exception )
        {
            return false;
        }
        return true;
    }

    /// <summary>   Attempts to ping the Portmap service by calling the null procedure (0). </summary>
    /// <remarks>   Unit tests shows this to take 3 ms. </remarks>
    /// <param name="ioTimeout">        (Optional) timeout in milliseconds to wait before assuming
    ///                                 that no portmap service is currently available [100]. </param>
    /// <param name="transmitTimeout">  (Optional) The transmit timeout; defaults to 25 ms. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    public static bool TryPingPortmapService( int ioTimeout = 100, int transmitTimeout = 25 )
    {
        return OncRpcPortmapClient.TryPingPortmapService( IPAddress.Loopback, ioTimeout, transmitTimeout );
    }

    /// <summary>   Tries to ping the portmap service on the specified host. </summary>
    /// <remarks>   Unit tests shows this to take 3 ms. </remarks>
    /// <param name="host">             Host where to contact the portmapper. </param>
    /// <param name="ioTimeout">        (Optional) timeout in milliseconds to wait before assuming
    ///                                 that no portmap service is currently available [100]. </param>
    /// <param name="transmitTimeout">  (Optional) The transmit timeout; defaults to 25 ms. </param>
    /// <returns>
    /// <see langword="true"/>, if a portmap service is running and can be contacted.
    /// </returns>
    public static bool TryPingPortmapService( IPAddress host, int ioTimeout = 100, int transmitTimeout = 25 )
    {
        using OncRpcPortmapClient portmap = new( host, OncRpcProtocol.OncRpcUdp, transmitTimeout );
        portmap.OncRpcClient.IOTimeout = ioTimeout;
        return portmap.TryPingPortmapService();
    }

    #endregion

}
