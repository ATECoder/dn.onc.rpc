using System.Text;
using cc.isr.ONC.RPC.Portmap;
using cc.isr.ONC.RPC.Server;
using cc.isr.ONC.RPC.MSTest.Codecs;
using cc.isr.ONC.RPC.Logging;

namespace cc.isr.ONC.RPC.MSTest.Tcp;

/// <summary>   An ONC/RPC TCP server. </summary>
/// <remarks>   <list type="bullet">Mapped error codes:<item>
/// OncRpcException.RPC_SUCCESS -- Visa32.VISA.VI_SUCCESS</item><item>
/// OncRpcException.RPC_SYSTEMERROR -- Visa32.VISA.VI_ERROR_SYSTEM_ERROR</item><item>
/// OncRpcException.RPC_PROGUNAVAIL -- Visa32.VISA.VI_ERROR_INV_EXPR</item><item>
/// </item>
/// </list>
/// Closing a client connected to the Mock local server no longer throws an exception when destroying the link.
/// </remarks>
public partial class OncRpcTcpServer : OncRpcTcpServerBase
{

    #region " construction and cleanup "

    /// <summary>   Default constructor. </summary>
    public OncRpcTcpServer() : this( 0 )
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="port"> The port number where the server will wait for incoming calls. </param>
    public OncRpcTcpServer( int port ) : this( IPAddress.Any, port )
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="device">   current device. </param>
    /// <param name="bindAddr"> The local Internet Address the server will bind to. </param>
    /// <param name="port">     The port number where the server will wait for incoming calls. </param>
    public OncRpcTcpServer( IPAddress bindAddr, int port ) : base( bindAddr, port )
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

    private static OncRpcEmbeddedPortmapService EstablishPortmapService()
    {

        // Ignore all problems during unregistration.

        OncRpcEmbeddedPortmapService epm;

        Logger.Writer.LogVerbose( "Checking for portmap service: " );
        bool externalPortmap = OncRpcEmbeddedPortmapService.IsPortmapRunning();
        if ( externalPortmap )
            Logger.Writer.LogVerbose( "A portmap service is already running." );
        else
            Logger.Writer.LogVerbose( "No portmap service available." );

        // Create embedded portmap service and check whether is has sprung
        // into action.

        Logger.Writer.LogVerbose( "Creating embedded portmap instance: " );
        epm = new OncRpcEmbeddedPortmapService();

        if ( !epm.EmbeddedPortmapInUse() )
            Logger.Writer.LogVerbose( "embedded service not used: " );
        else
            Logger.Writer.LogVerbose( "embedded service started: " );
        if ( epm.EmbeddedPortmapInUse() == externalPortmap )
        {
            Logger.Writer.LogWarning( "ERROR: no service available or both." );
        }
        else
            Logger.Writer.LogVerbose( "Passed." );

        return epm;
    }

    #endregion

    #region " START / STOP "

    private bool _listening;
    /// <summary>   Gets or sets a value indicating whether the listening. </summary>
    /// <value> True if listening, false if not. </value>
    public bool Listening
    {
        get => this._listening;
        set => _ = this.SetProperty( ref this._listening, value );
    }

    /// <summary>   Gets or sets the embedded portmap service. </summary>
    /// <remarks> @atecoder: This was added to allow the disposal of the Portmap service
    /// with unit testing. </remarks>
    /// <value> The embedded portmap service. </value>
    public OncRpcEmbeddedPortmapService? EmbeddedPortmapService { get; private set; }

    /// <summary>
    /// All inclusive convenience method: register server transports with port mapper, then Runs the
    /// call dispatcher until the server is signaled to shut down, and finally deregister the
    /// transports.
    /// </summary>
    /// <remarks>
    /// All inclusive convenience method: register server transports with port mapper, then Runs the
    /// call dispatcher until the server is signaled to shut down, and finally deregister the 
    /// transports.
    /// TODO: Implement on connected upon registration so as to provide connection information to event listeners.
    /// </remarks>
    public override void Run()
    {
        this.EmbeddedPortmapService = OncRpcTcpServer.EstablishPortmapService();
        base.Run();
    }

    /// <summary>
    /// Processes incoming remote procedure call requests from all specified transports.
    /// </summary>
    /// <remarks>
    /// To end processing and to shut the server down signal the <see cref="OncRpcServerStubBase.shutdownSignal"/> 
    /// object. Note that the thread on which <see cref="Run()"/> is called will ignore
    /// any interruptions and will silently swallow them.
    /// </remarks>
    /// <param name="transports">   Array of server transport objects for which processing of remote
    ///                             procedure call requests should be done. </param>
    public override void Run( OncRpcTransportBase[] transports )
    {
        this.Listening = true;
        base.Run( transports );
    }

    /// <summary>
    /// Notifies the RPC server to stop processing of remote procedure call requests as soon as
    /// possible.
    /// </summary>
    /// <remarks>
    /// Notifies the RPC server to stop processing of remote procedure call requests as soon as
    /// possible. Note that each transport has its own thread, so processing will not stop before the
    /// transports have been closed by calling the <see cref="Close(OncRpcTransportBase[])"/>
    /// method of the server.
    /// </remarks>
    public override void StopRpcProcessing()
    {
        this.Listening = false;
        base.StopRpcProcessing();
    }

    /// <summary>   Shuts down this server. </summary>
    public virtual void Shutdown()
    {
        this.StopRpcProcessing();
    }

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
    /// <param name="call">         <see cref="OncRpcCallHandler"/> about the call to handle, 
    ///                             like the caller's Internet address, the ONC/RPC 
    ///                             call header, etc. </param>
    /// <param name="program">      Program number requested by client. </param>
    /// <param name="version">      Version number requested. </param>
    /// <param name="procedure">    Procedure number requested. </param>
    public override void DispatchOncRpcCall( OncRpcCallHandler call, int program, int version, int procedure )
    {
        base.DispatchOncRpcCall( call, program, version, procedure );
        if ( version == RpcProgramConstants.Version1 )
        {
            OncRpcTcpServer.DispatchOncRpcCall( call, ( RemoteProceduresVersion1 ) procedure );
        }
        else if ( version == RpcProgramConstants.Version2 )
        {
            OncRpcTcpServer.DispatchOncRpcCall( call, ( RemoteProceduresVersion2 ) procedure );
        }
        else
        {
            call.ReplyProgramNotAvailable();
        }
    }

    /// <summary>   Dispatch (handle) an ONC/RPC request from a client. </summary>
    /// <param name="call">         The call. </param>
    /// <param name="procedure">    The procedure. </param>
    private static void DispatchOncRpcCall( OncRpcCallHandler call, RemoteProceduresVersion1 procedure )
    {
        switch ( procedure )
        {
            case RemoteProceduresVersion1.Nop:
                {
                    // ping
                    call.RetrieveCall( VoidXdrCodec.VoidXdrCodecInstance );
                    OncRpcTcpServer.Nop();
                    call.Reply( VoidXdrCodec.VoidXdrCodecInstance );
                    break;
                }
            case RemoteProceduresVersion1.Echo:
                {
                    StringXdrCodec args = new();
                    call.RetrieveCall( args );
                    StringXdrCodec result = new( OncRpcTcpServer.EchoInput( args.Value ) );
                    call.Reply( result );
                    break;
                }
            case RemoteProceduresVersion1.ConcatenateInputParameters:
                {
                    StringVectorCodec args = new();
                    call.RetrieveCall( args );
                    StringXdrCodec result = new( OncRpcTcpServer.ConcatenateInputStringVector( args ) );
                    call.Reply( result );
                    break;
                }
            case RemoteProceduresVersion1.CompareInputToFoo:
                {
                    IntXdrCodec args = new();
                    call.RetrieveCall( args );
                    BooleanXdrCodec result = new( OncRpcTcpServer.CompareInputToFoo( args.Value ) );
                    call.Reply( result );
                    break;
                }
            case RemoteProceduresVersion1.ReturnEnumFooValue:
                {
                    call.RetrieveCall( VoidXdrCodec.VoidXdrCodecInstance );
                    IntXdrCodec result = new( OncRpcTcpServer.ReturnEnumFooValue() );
                    call.Reply( result );
                    break;
                }
            case RemoteProceduresVersion1.BuildLinkedList:
                {
                    LinkedListCodec args = new();
                    call.RetrieveCall( args );
                    LinkedListCodec result = OncRpcTcpServer.BuildLinkedList( args );
                    call.Reply( result );
                    break;
                }
            case RemoteProceduresVersion1.RemoteProcedureReadSomeResult:
                {
                    call.RetrieveCall( VoidXdrCodec.VoidXdrCodecInstance );
                    SomeResultCodec result = OncRpcTcpServer.ReadSomeResult();
                    call.Reply( result );
                    break;
                }
            default:
                call.ReplyProcedureNotAvailable();
                break;
        }
    }

    /// <summary>   Dispatch (handle) an ONC/RPC request from a client. </summary>
    /// <param name="call">         <see cref="OncRpcCallHandler"/> about the call to handle, 
    ///                             like the caller's Internet address, the ONC/RPC
    ///                             call header, etc. </param>
    /// <param name="procedure">    Procedure number requested. </param>
    private static void DispatchOncRpcCall( OncRpcCallHandler call, RemoteProceduresVersion2 procedure )
    {
        switch ( procedure )
        {
            case RemoteProceduresVersion2.Nop:
                {
                    call.RetrieveCall( VoidXdrCodec.VoidXdrCodecInstance );
                    OncRpcTcpServer.Nop();
                    call.Reply( VoidXdrCodec.VoidXdrCodecInstance );
                    break;
                }
            case RemoteProceduresVersion2.ConcatenateTwoValues:
                {
                    DualStringsCodec args = new();
                    call.RetrieveCall( args );
                    StringXdrCodec result = new( OncRpcTcpServer.ConcatenateTwoValues( args.Arg1, args.Arg2 ) );
                    call.Reply( result );
                    break;
                }
            case RemoteProceduresVersion2.ConcatenateThreeItems:
                {
                    TripleStringsCodec args = new();
                    call.RetrieveCall( args );
                    StringXdrCodec result = new( OncRpcTcpServer.ConcatenateThreeItems( args.One, args.Two, args.Three ) );
                    call.Reply( result );
                    break;
                }
            case RemoteProceduresVersion2.ReturnYouAreFooValue:
                {
                    IntXdrCodec args = new();
                    call.RetrieveCall( args );
                    StringXdrCodec result = new( OncRpcTcpServer.ReturnYouAreFooValue( args.Value ) );
                    call.Reply( result );
                    break;
                }
            case RemoteProceduresVersion2.LinkListItems:
                {
                    DualLinkedListsCodec args = new();
                    call.RetrieveCall( args );
                    LinkedListCodec result = OncRpcTcpServer.LinkListItems( args.List1, args.List2 );
                    call.Reply( result );
                    break;
                }
            case RemoteProceduresVersion2.ProcessFourArguments:
                {
                    StringTripleIntegerCodec args = new();
                    call.RetrieveCall( args );
                    OncRpcTcpServer.ProcessFourArguments( args.A, args.B, args.C, args.D );
                    call.Reply( VoidXdrCodec.VoidXdrCodecInstance );
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

    /// <summary>   Compare parameters to <see cref="EnumFoo.FOO"/>; return true if <paramref name="expected"/> equals <see cref="EnumFoo.FOO"/>. </summary>
    /// <param name="expected">   expected value. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    public static bool CompareInputToFoo( int expected )
    {
        return expected == ( int ) EnumFoo.FOO;
    }

    /// <summary>   Return <see cref="EnumFoo.FOO"/>. </summary>
    /// <returns>   An int. </returns>
    public static int ReturnEnumFooValue()
    {
        return ( int ) EnumFoo.FOO;
    }

    /// <summary>   Concatenate input string vector. </summary>
    /// <param name="inputCodec">   the input codec. </param>
    /// <returns>   A string. </returns>
    public static string ConcatenateInputStringVector( StringVectorCodec inputCodec )
    {
        StringBuilder result = new();
        int size = inputCodec.GetValues().Length;
        for ( int idx = 0; idx < size; ++idx )
        {
            _ = result.Append( inputCodec.GetValues()[idx].Value );
        }
        return result.ToString();
    }

    /// <summary>   Build a new linked list. </summary>
    /// <param name="linkedListCodec">   the linked list codec input. </param>
    /// <returns>   A <see cref="LinkedListCodec"/>. </returns>
    public static LinkedListCodec BuildLinkedList( LinkedListCodec linkedListCodec )
    {
        LinkedListCodec newNode = new() {
            Foo = 42,
            Next = linkedListCodec
        };
        return newNode;
    }

    /// <summary>   Reads some result. </summary>
    /// <returns>   some result 1. </returns>
    public static SomeResultCodec ReadSomeResult()
    {
        SomeResultCodec res = new();
        return res;
    }

    /// <summary>   Concatenate two values. </summary>
    /// <param name="arg1"> The first argument. </param>
    /// <param name="arg2"> The second argument. </param>
    /// <returns>   A string. </returns>
    public static string ConcatenateTwoValues( string arg1, string arg2 )
    {
        return $"{arg1}{arg2}";
    }

    /// <summary>   Concatenate three items. </summary>
    /// <param name="one">      The one. </param>
    /// <param name="two">      The two. </param>
    /// <param name="three">    The three. </param>
    /// <returns>   A string. </returns>
    public static string ConcatenateThreeItems( string one, string two, string three )
    {
        return $"{one}{two}{three}";
    }

    /// <summary>   Return 'you are Foo' value. </summary>
    /// <param name="foo">  The foo. </param>
    /// <returns>   A string. </returns>
    public static string ReturnYouAreFooValue( int foo )
    {
        return $"You are foo {foo}.";
    }

    /// <summary>   Link linked list <paramref name="l1"/> as next item of linked list <paramref name="l2"/>. </summary>
    /// <param name="l1">   The first <see cref="LinkedListCodec"/>. </param>
    /// <param name="l2">   The second <see cref="LinkedListCodec"/>. </param>
    /// <returns>   An <see cref="LinkedListCodec"/>. </returns>
    public static LinkedListCodec LinkListItems( LinkedListCodec l1, LinkedListCodec l2 )
    {
        l2.Next = l1;
        return l2;
    }

    /// <summary>   Process four arguments. </summary>
    /// <param name="a">    A string to process. </param>
    /// <param name="b">    An int to process. </param>
    /// <param name="c">    An int to process. </param>
    /// <param name="d">    An int to process. </param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Style", "IDE0060:Remove unused parameter", Justification = "<Pending>" )]
    public static void ProcessFourArguments( string a, int b, int c, int d )
    {
    }

    #endregion

}
