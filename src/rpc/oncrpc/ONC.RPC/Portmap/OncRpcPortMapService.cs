using System.Net.Sockets;

using cc.isr.ONC.RPC.Codecs;
using cc.isr.ONC.RPC.EnumExtensions;
using cc.isr.ONC.RPC.Server;

namespace cc.isr.ONC.RPC.Portmap;

/// <summary>
/// The class <see cref="OncRpcPortMapService"/> implements a C#-based ONC/RPC port map service, speaking the
/// widely deployed protocol version 2.
/// </summary>
/// <remarks>
/// This class can be either used stand-alone (a static <c>main</c> is provided for this
/// purpose) or as part of an application. In this case you should check first for another
/// portmap already running before starting your own one.<para>
/// 
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public class OncRpcPortMapService : OncRpcServerStubBase, IOncRpcDispatchable
{

    #region " construction and cleanup "

    /// <summary> 
    /// Creates a new portmap service instance. Creates the transport registration information and UDP and TCP-
    /// based transports, which will be bound later to port 111.
    /// </summary>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    public OncRpcPortMapService()
    {
        // We only need to register one {program, version}.
        OncRpcProgramInfo[] registeredPrograms = new OncRpcProgramInfo[] {
            new OncRpcProgramInfo(OncRpcPortmapConstants.OncRpcPortmapProgramNumber, OncRpcPortmapConstants.OncRpcPortmapProgramVersionNumber )
        };
        this.SetRegisteredPrograms( registeredPrograms );

        // We support both UDP and TCP-based transports for ONC/RPC portmap
        // calls, and these transports are bound to the well-known port 111.
        OncRpcTransportBase[] transports = new OncRpcTransportBase[] {
            new OncRpcUdpTransport(this, OncRpcPortmapConstants.OncRpcPortmapPortNumber, registeredPrograms, OncRpcPortMapService.BufferSizeDefault),
            new OncRpcTcpTransport( this, OncRpcPortmapConstants.OncRpcPortmapPortNumber, registeredPrograms, OncRpcPortMapService.BufferSizeDefault)
        };
        this.SetTransports( transports );

        this.CharacterEncoding = XdrTcpEncodingStream.EncodingDefault;

        // Finally, we add ourself to the list of registered ONC/RPC servers.
        // This is just a convenience.
        this.ServerIdentifierCodecs.Add( new OncRpcServerIdentifierCodec( OncRpcPortmapConstants.OncRpcPortmapProgramNumber,
                                                                          OncRpcPortmapConstants.OncRpcPortmapProgramVersionNumber,
                                                                          OncRpcProtocol.OncRpcTcp,
                                                                          OncRpcPortmapConstants.OncRpcPortmapPortNumber ) );
        this.ServerIdentifierCodecs.Add( new OncRpcServerIdentifierCodec( OncRpcPortmapConstants.OncRpcPortmapProgramNumber,
                                                                          OncRpcPortmapConstants.OncRpcPortmapProgramVersionNumber,
                                                                          OncRpcProtocol.OncRpcUdp,
                                                                          OncRpcPortmapConstants.OncRpcPortmapPortNumber ) );

        // Determine all local IP addresses assigned to this host.
        // Once again, take care of broken JDKs, which cannot handle
        // InetAdress.getLocalHost() properly. Sigh.
        try
        {
            IPAddress loopback = IPAddress.Loopback;

            // Get host name
            string strHostName = Dns.GetHostName();
            IPAddress[] addrs = Dns.GetHostAddresses( strHostName );

            // Check whether the loop-back address is already included in
            // the address list for this host. If not, add it to the list.
            bool loopbackIncluded = false;
            foreach ( IPAddress addr in addrs )
            {
                if ( addr.Equals( loopback ) )
                {
                    loopbackIncluded = true;
                    break;
                }
            }

            if ( loopbackIncluded )
                this._locals = addrs;
            else
            {
                this._locals = new IPAddress[addrs.Length + 1];
                this._locals[0] = loopback;
                Array.Copy( addrs, 0, this._locals, 1, addrs.Length );
            }
        }
        catch ( SocketException )
        {
            // @jmw: need to debug this and see if it's the right exception to catch here

            // Trouble getting all addresses for this host (which might
            // have been caused by some dumb security manager -- yeah, as
            // if managers were not dumb by definition), so fall back to
            // allowing only the loop back address.
            this._locals = new IPAddress[1];
            this._locals[0] = IPAddress.Loopback;
        }
    }

    #endregion

    #region " defaults "

    /// <summary>   Gets or sets the default buffer size. </summary>
    /// <value> The buffer size default. </value>
    public static int BufferSizeDefault { get; set; } = 32768;

    #endregion

    #region " members "

    /// <summary>  Gets List of IP addresses assigned to this host. </summary>
    /// <value> The locals. </value>
    private readonly IPAddress[] _locals = Array.Empty<IPAddress>();

    /// <summary>   Gets the List of IP addresses assigned to this host.. </summary>
    /// <returns>   An array of IP address. </returns>
    public IPAddress[] GetLocals()
    {
        return this._locals;
    }

    /// <summary>
    /// Gets or sets the <see cref="List{T}"/> of registered servers codecs <see cref="OncRpcServerIdentifierCodec"/>.
    /// </summary>
    /// <value> the <see cref="List{T}"/> of <see cref="OncRpcServerIdentifierCodec"/>. </value>
    public List<OncRpcServerIdentifierCodec> ServerIdentifierCodecs { get; set; } = new();

    /// <summary>   Returns the <see cref="IXdrCodec"/> listing the registered ONC/RPC servers. </summary>
    /// <returns>   list of ONC/RPC server descriptions (program, version, protocol, port). </returns>
    internal virtual OncRpcPortmapServersListCodec RegisteredServersCodec()
    {
        OncRpcPortmapServersListCodec result = new() { ServerIdentifiers = this.ServerIdentifierCodecs };
        return result;
    }

    #endregion

    #region " actions "

    /// <summary>   Lookup port for (program, version, protocol). </summary>
    /// <remarks>
    /// If no suitable registration entry if found and an entry with another version, but the same
    /// program and version number is found, this is returned instead. This is compatible with the
    /// way Sun's portmap implementation works.
    /// </remarks>
    /// <param name="serverIdentification"> server identification (program, version, protocol) to
    ///                                     look up. The port field is not used. </param>
    /// <returns>
    /// port number where server listens for incoming ONC/RPC calls, or <c>0</c>, if no server is
    /// registered for (program, protocol).
    /// </returns>
    internal virtual OncRpcGetPortCodec GetPort( OncRpcServerIdentifierCodec serverIdentification )
    {
        OncRpcServerIdentifierCodec? identification = null;
        OncRpcGetPortCodec reply = new();
        foreach ( OncRpcServerIdentifierCodec svr in this.ServerIdentifierCodecs )
        {
            if ( svr.Program == serverIdentification.Program && svr.Protocol == serverIdentification.Protocol )
            {
                // (program, protocol) already matches. If it has the same
                // version, then we're done. Otherwise we remember this
                // entry for possible later usage and search further through
                // the list.
                if ( svr.Version == serverIdentification.Version )
                {
                    reply.Port = svr.Port;
                    return reply;
                }
                identification = svr;
            }
        }

        // Return port of "best" match, if one was found at all, otherwise
        // just return 0, which indicates an invalid UDP/TCP port.
        reply.Port = identification is null ? 0 : identification.Port;
        return reply;
    }

    /// <summary>   Register a port number for a particular (program, version, protocol). </summary>
    /// <remarks>
    /// Note that a caller cannot register the same (program, version, protocol) for another port.
    /// In this case we return false. Thus, a caller first needs to deregister any old entries which
    /// it wishes to update. Always add new registration entries to the end of the list (vector).
    /// </remarks>
    /// <param name="serverIdentification">   (program, version, protocol, port) to register. </param>
    /// <returns>   <see langword="true"/> if registration succeeded. </returns>
    internal virtual BooleanXdrCodec SetPort( OncRpcServerIdentifierCodec serverIdentification )
    {
        if ( serverIdentification.Program != OncRpcPortmapConstants.OncRpcPortmapProgramNumber )
        {
            // Only accept registration attempts for anything other than
            // the port mapper. We do not want clients to play tricks on us.

            foreach ( OncRpcServerIdentifierCodec svr in this.ServerIdentifierCodecs )
            {
                if ( svr.Program == serverIdentification.Program &&
                    svr.Version == serverIdentification.Version &&
                     svr.Protocol == serverIdentification.Protocol )
                    // In case (program, version, protocol) is already
                    // registered only accept, if the port stays the same.
                    // This will silently accept double registrations (i.e.,
                    // due to duplicated UDP calls).
                    return new BooleanXdrCodec( svr.Port == serverIdentification.Port );
            }

            // Add new registration entry to end of the list.

            this.ServerIdentifierCodecs.Add( serverIdentification );
            return new BooleanXdrCodec( true );
        }
        return new BooleanXdrCodec( false );
    }

    /// <summary>
    /// Deregister all port settings for a particular (program, version) for all transports (TCP, UDP,
    /// etc.).
    /// </summary>
    /// <remarks>
    /// While these are strange semantics, they are compatible with Sun's portmap implementation.
    /// </remarks>
    /// <param name="serverIdentification"> (program, version) to deregister. The protocol and port
    ///                                     fields are not used. </param>
    /// <returns>   <see langword="true"/> if deregistration succeeded. </returns>
    internal virtual BooleanXdrCodec UnsetPort( OncRpcServerIdentifierCodec serverIdentification )
    {
        bool ok = false;
        if ( serverIdentification.Program != OncRpcPortmapConstants.OncRpcPortmapProgramNumber )
        {
            // Only allow clients to deregister ONC/RPC servers other than
            // the portmap entries.
            int size = this.ServerIdentifierCodecs.Count;
            for ( int idx = size - 1; idx >= 0; --idx )
            {
                OncRpcServerIdentifierCodec svr = ( OncRpcServerIdentifierCodec ) this.ServerIdentifierCodecs[idx]!;
                if ( svr.Program == serverIdentification.Program && svr.Version == serverIdentification.Version )
                {
                    this.ServerIdentifierCodecs.RemoveAt( idx );
                    ok = true;
                }
            }
        }
        return new BooleanXdrCodec( ok );
    }

    /// <summary>
    /// Checks whether the address given belongs to one of the local addresses of this host.
    /// </summary>
    /// <param name="addr"> IP address to check. </param>
    /// <returns>
    /// <see langword="true"/> if address specified belongs to one of the
    /// local addresses of this host.
    /// </returns>
    internal virtual bool IsLocalAddress( IPAddress addr )
    {
        foreach ( IPAddress localAddress in this._locals )
            if ( addr.Equals( localAddress ) ) return true;
        return false;
    }

    /// <summary>   Query if 'endPoint' is local end point. </summary>
    /// <param name="endPoint"> The end point. </param>
    /// <returns>   True if local end point, false if not. </returns>
    internal virtual bool IsLocalEndPoint( IPEndPoint? endPoint )
    {
        return endPoint is not null && this.IsLocalAddress( endPoint.Address );
    }

    /// <summary>   Dispatch incoming ONC/RPC calls to the individual handler functions. </summary>
    /// <remarks>   The CALLIT method is currently unimplemented. </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="call">         The ONC/RPC call, with references to the transport and XDR
    ///                             streams to use for retrieving parameters and sending replies. </param>
    /// <param name="program">      the portmap's program number, 100000. </param>
    /// <param name="version">      the portmap's protocol version, 2. </param>
    /// <param name="procedure">    the procedure to call. </param>
    public virtual void DispatchOncRpcCall( OncRpcCallHandler call, int program, int version, int procedure )
    {
        // Make sure it's the right program and version that we can handle.
        // (defensive programming)
        if ( program == OncRpcPortmapConstants.OncRpcPortmapProgramNumber )
            if ( version == OncRpcPortmapConstants.OncRpcPortmapProgramVersionNumber )
                switch ( procedure.ToPortmapServiceProcedure() )
                {
                    case OncRpcPortmapServiceProcedure.OncRpcPortmapPing:
                        {
                            // handle NULL call.
                            call.RetrieveCall( VoidXdrCodec.VoidXdrCodecInstance );
                            call.Reply( VoidXdrCodec.VoidXdrCodecInstance );
                            break;
                        }

                    case OncRpcPortmapServiceProcedure.OncRpcPortmapGetPortNumber:
                        {
                            // handle port query
                            OncRpcServerIdentifierCodec requestCodec = new();
                            call.RetrieveCall( requestCodec );
                            OncRpcGetPortCodec replyCodec = this.GetPort( requestCodec );
                            call.Reply( replyCodec );
                            break;
                        }

                    case OncRpcPortmapServiceProcedure.OncRpcPortmapRegisterServer:
                        {
                            // handle port registration

                            // ensure that no remote client tries to register
                            OncRpcServerIdentifierCodec requestCodec = new();
                            call.RetrieveCall( requestCodec );
                            BooleanXdrCodec replyCodec = this.IsLocalEndPoint( call.PeerEndPoint )
                                ? this.SetPort( requestCodec )
                                : new BooleanXdrCodec( false );
                            call.Reply( replyCodec );
                            break;
                        }

                    case OncRpcPortmapServiceProcedure.OncRpcPortmapUnregisterServer:
                        {
                            // handle port deregistration
                            OncRpcServerIdentifierCodec requestCodec = new();
                            call.RetrieveCall( requestCodec );
                            BooleanXdrCodec replyCodec = this.IsLocalEndPoint( call.PeerEndPoint )
                                ? this.UnsetPort( requestCodec )
                                : new BooleanXdrCodec( false );
                            call.Reply( replyCodec );
                            break;
                        }

                    case OncRpcPortmapServiceProcedure.OncRpcPortmapListRegisteredServers:
                        {
                            // list all registrations
                            call.RetrieveCall( VoidXdrCodec.VoidXdrCodecInstance );
                            OncRpcPortmapServersListCodec replyCodec = this.RegisteredServersCodec();
                            call.Reply( replyCodec );
                            break;
                        }

                    default:
                        {
                            // unknown/unimplemented procedure
                            call.ReplyProcedureNotAvailable();
                            break;
                        }
                }
            else
                call.ReplyProgramVersionMismatch( OncRpcPortmapConstants.OncRpcPortmapProgramVersionNumber, OncRpcPortmapConstants.OncRpcPortmapProgramVersionNumber );
        else
            call.ReplyProgramNotAvailable();
    }

    #endregion

}
