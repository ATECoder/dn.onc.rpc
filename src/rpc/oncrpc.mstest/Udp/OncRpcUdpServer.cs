using cc.isr.ONC.RPC.Server;

namespace cc.isr.ONC.RPC.MSTest.Udp;

/// <summary>   An ONC/RPC UDP server. </summary>
/// <remarks>
/// <list type="bullet">Mapped error codes:<item>
/// OncRpcException.RPC_SUCCESS -- Visa32.VISA.VI_SUCCESS</item><item>
/// OncRpcException.RPC_SYSTEMERROR -- Visa32.VISA.VI_ERROR_SYSTEM_ERROR</item><item>
/// OncRpcException.RPC_PROGUNAVAIL -- Visa32.VISA.VI_ERROR_INV_EXPR</item><item>
/// </item>
/// </list>
/// Closing a client connected to the Mock local server no longer throws an exception when destroying the link.
/// </remarks>
public partial class OncRpcUdpServer : OncRpcUdpServerBase
{

    #region " construction and cleanup "

    /// <summary>   Default constructor. </summary>
    public OncRpcUdpServer() : this( 0 )
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="port"> The port number where the server will wait for incoming calls. </param>
    public OncRpcUdpServer( int port ) : this( IPAddress.Any, port )
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="device">   current device. </param>
    /// <param name="bindAddr"> The local Internet Address the server will bind to. </param>
    /// <param name="port">     The port number where the server will wait for incoming calls. </param>
    public OncRpcUdpServer( IPAddress bindAddr, int port ) : base( bindAddr, port )
    {
        this._iPv4Address = bindAddr is null ? string.Empty : bindAddr.ToString();
        this._readMessage = string.Empty;
        this._writeMessage = string.Empty;
    }

    #endregion

    #region " Server Properties "

    private int _portNumber;
    /// <summary>   Gets or sets the port number. </summary>
    /// <value> The port number. </value>
    public int PortNumber
    {
        get => this._portNumber;
        set => _ = this.SetProperty( ref this._portNumber, value );
    }

    private string _iPv4Address;
    /// <summary>   Gets or sets the IPv4 address. </summary>
    /// <value> The IPv4 address. </value>
    public string IPv4Address
    {
        get => this._iPv4Address;
        set => _ = this.SetProperty( ref this._iPv4Address, value );
    }

    #endregion

    #region " I/O messages "

    private string _writeMessage;
    /// <summary>   Gets or sets a message that was sent to the device. </summary>
    /// <value> The message that was sent to the device. </value>
    public string WriteMessage
    {
        get => this._writeMessage;
        set => _ = this.SetProperty( ref this._writeMessage, value );
    }

    private string _readMessage;
    /// <summary>   Gets or sets a message that was received from the device. </summary>
    /// <value> A message that was received from the device. </value>
    public string ReadMessage
    {
        get => this._readMessage;
        set => _ = this.SetProperty( ref this._readMessage, value );
    }

    #endregion

    #region " Port mapper "

#if false
    private static void EstablishPortmapService()
    {

        // Ignore all problems during unregistration.

        OncRpcEmbeddedPortmapService epm;

        Logger.Writer.LogInformation( "Checking for portmap service: " );
        bool externalPortmap = OncRpcEmbeddedPortmapService.TryPingPortmapService();
        if ( externalPortmap )
            Logger.Writer.LogInformation( "A portmap service is already running." );
        else
            Logger.Writer.LogInformation( "No portmap service available." );

        // Create embedded portmap service and check whether is has sprung
        // into action.

        Logger.Writer.LogInformation( "Creating embedded portmap instance: " );
        try
        {
            epm = new OncRpcEmbeddedPortmapService();

            if ( !epm.EmbeddedPortmapInUse() )
                Logger.Writer.LogInformation( "embedded service not used: " );
            else
                Logger.Writer.LogInformation( "embedded service started: " );

            if ( epm.EmbeddedPortmapInUse() == externalPortmap )
            {
                Logger.Writer.LogWarning( "ERROR: no service available or both." );
                return;
            }
        }
        catch ( IOException e )
        {
            Logger.Writer.LogMemberError( $"ERROR: failed:", e );
        }
        catch ( OncRpcException e )
        {
            Logger.Writer.LogMemberError( $"ERROR: failed:", e );
        }

        Logger.Writer.LogInformation( "    Passed." );
    }
#endif

    #endregion

    #region " Handle Procedure calls "

    /// <summary>   Dispatch (handle) an ONC/RPC request from a client. </summary>
    /// <remarks>
    /// This interface has some fairly deep semantics, so please read the description above for how
    /// to use it properly. For background information about fairly deep semantics, please also refer
    /// to <i>Gigzales</i>, <i>J</i>.: Semantics considered harmful. Addison-Reilly, 1992, ISBN 0-542-
    /// 10815-X. <para>
    /// 
    /// See the introduction to this class for examples of how to use this interface properly.</para>
    /// </remarks>
    /// <param name="call">         <see cref="OncRpcCallHandler"/> about the call to handle, like the 
    ///                             caller's Internet address, the ONC/RPC call header, etc. </param>
    /// <param name="program">      Program number requested by client. </param>
    /// <param name="version">      Version number requested. </param>
    /// <param name="procedure">    Procedure number requested. </param>
    public override void DispatchOncRpcCall( OncRpcCallHandler call, int program, int version, int procedure )
    {
        base.DispatchOncRpcCall( call, program, version, procedure );
        if ( version == 1 )
        {
            OncRpcUdpServer.ProcessVersion1Calls( call, procedure );
        }
        else if ( version == 2 )
        {
            OncRpcUdpServer.ProcessVersion2Calls( call, procedure );
        }
        else if ( program == 42 && version == 42 && procedure == RemoteProcedures.RequestServerShutdown )
        {
            call.RetrieveCall( VoidXdrCodec.VoidXdrCodecInstance );
            call.Reply( VoidXdrCodec.VoidXdrCodecInstance );
            this.StopRpcProcessing();
        }
        else
        {
            call.ReplyProgramNotAvailable();
        }
    }

    /// <summary>   Process the version 1 calls. </summary>
    /// <param name="call">         The call. </param>
    /// <param name="procedure">    The procedure. </param>
    private static void ProcessVersion1Calls( OncRpcCallHandler call, int procedure )
    {
        switch ( procedure )
        {
            case RemoteProcedures.Nop:
                {
                    // ping
                    call.RetrieveCall( VoidXdrCodec.VoidXdrCodecInstance );
                    OncRpcUdpServer.Nop();
                    call.Reply( VoidXdrCodec.VoidXdrCodecInstance );
                    break;
                }
            case RemoteProcedures.Echo:
                {
                    StringXdrCodec request = new();
                    call.RetrieveCall( request );
                    StringXdrCodec result = new( OncRpcUdpServer.EchoInput( request.Value ) );
                    call.Reply( result );
                    break;
                }
            default:
                call.ReplyProcedureNotAvailable();
                break;
        }
    }

    /// <summary>   Process the version 2 calls. </summary>
    /// <param name="call">         <see cref="OncRpcCallHandler"/> about the call to handle, like the
    ///                             caller's Internet address, the ONC/RPC
    ///                             call header, etc. </param>
    /// <param name="procedure">    Procedure number requested. </param>
    private static void ProcessVersion2Calls( OncRpcCallHandler call, int procedure )
    {
        switch ( procedure )
        {
            case RemoteProcedures.Nop:
                {
                    // ping
                    call.RetrieveCall( VoidXdrCodec.VoidXdrCodecInstance );
                    OncRpcUdpServer.Nop();
                    call.Reply( VoidXdrCodec.VoidXdrCodecInstance );
                    break;
                }
            case RemoteProcedures.Echo:
                {
                    StringXdrCodec request = new();
                    call.RetrieveCall( request );
                    StringXdrCodec result = new( OncRpcUdpServer.EchoInput( request.Value ) );
                    call.Reply( result );
                    break;
                }
            default:
                call.ReplyProcedureNotAvailable();
                break;
        }
    }

    #endregion

    #region " Remote Procedures "

    /// <summary>   No operation. </summary>
    public static void Nop()
    {
        // definitely nothing to do here...
    }

    /// <summary>   Echo the specified parameters. </summary>
    /// <param name="input">   value to echo. </param>
    /// <returns>   A string. </returns>
    public static string EchoInput( string input )
    {
        return input;
    }

    #endregion

}
