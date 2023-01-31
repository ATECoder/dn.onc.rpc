using System.Text;
using cc.isr.ONC.RPC.Server;
using cc.isr.ONC.RPC.MSTest.Codecs;

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
                    StringXdrCodec request = new();
                    call.RetrieveCall( request );
                    StringXdrCodec result = new( OncRpcTcpServer.EchoInput( request.Value ) );
                    call.Reply( result );
                    break;
                }
            case RemoteProceduresVersion1.ConcatenateInputParameters:
                {
                    StringVectorCodec request = new();
                    call.RetrieveCall( request );
                    StringXdrCodec result = new( OncRpcTcpServer.ConcatenateInputStringVector( request ) );
                    call.Reply( result );
                    break;
                }
            case RemoteProceduresVersion1.CompareInputToFoo:
                {
                    IntXdrCodec request = new();
                    call.RetrieveCall( request );
                    BooleanXdrCodec result = new( OncRpcTcpServer.CompareInputToFoo( request.Value ) );
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
            case RemoteProceduresVersion1.PrependLinkedList:
                {
                    LinkedListCodec request = new();
                    call.RetrieveCall( request );
                    LinkedListCodec result = OncRpcTcpServer.PrependLinkedList( request );
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
                    DualStringsCodec request = new();
                    call.RetrieveCall( request );
                    StringXdrCodec result = new( OncRpcTcpServer.ConcatenateTwoValues( request.FirstValue, request.SecondValue ) );
                    call.Reply( result );
                    break;
                }
            case RemoteProceduresVersion2.ConcatenateThreeItems:
                {
                    TripleStringsCodec request = new();
                    call.RetrieveCall( request );
                    StringXdrCodec result = new( OncRpcTcpServer.ConcatenateThreeItems( request.One, request.Two, request.Three ) );
                    call.Reply( result );
                    break;
                }
            case RemoteProceduresVersion2.ReturnYouAreFooValue:
                {
                    IntXdrCodec request = new();
                    call.RetrieveCall( request );
                    StringXdrCodec result = new( OncRpcTcpServer.ReturnYouAreFooValue( request.Value ) );
                    call.Reply( result );
                    break;
                }
            case RemoteProceduresVersion2.LinkListItems:
                {
                    DualLinkedListsCodec request = new();
                    call.RetrieveCall( request );
                    LinkedListCodec result = OncRpcTcpServer.LinkListItems( request.List1, request.List2 );
                    call.Reply( result );
                    break;
                }
            case RemoteProceduresVersion2.ProcessFourArguments:
                {
                    StringTripleIntegerCodec request = new();
                    call.RetrieveCall( request );
                    OncRpcTcpServer.ProcessFourArguments( request.A, request.B, request.C, request.D );
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
        StringBuilder reply = new();
        foreach ( StringCodec stringCodec in inputCodec.GetValues() )
            _ = reply.Append( stringCodec.Value );
        return reply.ToString();
    }

    /// <summary>   Echo a linked list. </summary>
    /// <param name="linkedListCodec">   the linked list codec input. </param>
    /// <returns>   A <see cref="LinkedListCodec"/>. </returns>
    public static LinkedListCodec EchoLinkedList( LinkedListCodec linkedListCodec )
    {
        LinkedListCodec newNode = new( linkedListCodec );
        return newNode;
    }

    /// <summary>   Prepend a node to the given linked list. </summary>
    /// <param name="linkedListCodec">   the linked list codec input. </param>
    /// <returns>   A <see cref="LinkedListCodec"/>. </returns>
    public static LinkedListCodec PrependLinkedList( LinkedListCodec linkedListCodec )
    {
        LinkedListCodec newNode
            = new() {
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
    /// <remarks>   2023-01-26. </remarks>
    /// <param name="firstValue">   The first parameter of type <see cref="string"/> to concatenate
    ///                             and to encode and decode. </param>
    /// <param name="secondValue">  The second parameter of type <see cref="string"/> to concatenate
    ///                             and to encode and decode. </param>
    /// <returns>   A string. </returns>
    public static string ConcatenateTwoValues( string firstValue, string secondValue )
    {
        return $"{firstValue}{secondValue}";
    }

    /// <summary>   Concatenate three items. </summary>
    /// <remarks>   2023-01-21. </remarks>
    /// <param name="one">      The first parameter of type <see cref="string"/> to concatenate. </param>
    /// <param name="two">      The second parameter of type <see cref="string"/> to concatenate. </param>
    /// <param name="three">    The third parameter of type <see cref="string"/> to concatenate. </param>
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

    /// <summary>   Link linked list <paramref name="l2"/> as next item of linked list <paramref name="l1"/>. </summary>
    /// <param name="l1">   The first <see cref="LinkedListCodec"/>. </param>
    /// <param name="l2">   The second <see cref="LinkedListCodec"/>. </param>
    /// <returns>   An <see cref="LinkedListCodec"/>. </returns>
    public static LinkedListCodec LinkListItems( LinkedListCodec l1, LinkedListCodec l2 )
    {
        l1.Next = l2;
        return l1;
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
