using cc.isr.ONC.RPC.Client;
using cc.isr.ONC.RPC.MSTest.Codecs;

namespace cc.isr.ONC.RPC.MSTest.Tcp;

/// <summary>   An ONC/RPC TCP Test client. </summary>
public class OncRpcTcpTestClient : IDisposable
{

    #region " construction and cleanup "

    private OncRpcClientBase? _coreClient;

    /// <summary>   Connect. </summary>
    /// <param name="host">     The host. </param>
    /// <param name="version">  The version. </param>
    public void Connect( IPAddress host, int version )
    {
        this.Host = host;
        this.Version = version;
        this._coreClient = OncRpcClientBase.NewOncRpcClient( host, RpcProgramConstants.ProgramNumber, version, 0,
                                                             OncRpcProtocol.OncRpcTcp, Client.OncRpcTcpClient.IOTimeoutDefault );
    }

    /// <summary>   Gets or sets a value indicating whether the ONC/RPC client is connected. </summary>
    /// <value> True if connected, false if not. </value>
    public bool Connected => this._coreClient is not null;

    /// <summary>   Query if this object is disposed. </summary>
    /// <returns>   True if disposed, false if not. </returns>
    public bool IsDisposed()
    {
        return this._coreClient is null;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
    /// resources.
    /// </summary>
    void IDisposable.Dispose()
    {
        this.Dispose( true );
        // Take this object off the finalization(Queue) and prevent finalization code 
        // from executing a second time.
        GC.SuppressFinalize( this );
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
    /// resources.
    /// </summary>
    /// <param name="disposing">    True to release both managed and unmanaged resources; false to
    ///                             release only unmanaged resources. </param>
    private void Dispose( bool disposing )
    {
        if ( !this.IsDisposed() && disposing )
            this.Close();
    }

    /// <summary>   Closes this object. </summary>
    public void Close()
    {
        try
        {
            this._coreClient?.Close();
        }
        catch ( Exception )
        {
            throw;
        }
        finally
        {
            this._coreClient = null;
        }
    }

    #endregion

    #region " members "

    /// <summary>   Gets or sets the version. </summary>
    /// <value> The version. </value>
    public int Version { get; private set; }

    /// <summary>   Gets or sets the host. </summary>
    /// <value> The host. </value>
    public IPAddress Host { get; private set; } = IPAddress.Loopback;

    #endregion

    #region " procedure calls "

    /// <summary>   Calls the specified procedure. </summary>
    /// <remarks>   2023-01-21. </remarks>
    /// <param name="procedureNumber">  The procedure number. </param>
    /// <param name="versionNumber">    The version number. </param>
    /// <param name="request">          parameter of type <see cref="IXdrCodec"/> to send to the
    ///                                 remote procedure call. </param>
    /// <param name="reply">            parameter of type <see cref="IXdrCodec"/> to receive the
    ///                                 reply from the remote procedure call. </param>
    private void Call( int procedureNumber, int versionNumber, IXdrCodec request, IXdrCodec reply )
    {
        this._coreClient?.Call( procedureNumber, versionNumber, request, reply );
    }

    /// <summary>   Sets the authentication to be used when making ONC/RPC calls. </summary>
    /// <param name="auth"> Authentication protocol handling object encapsulating authentication
    ///                     information. </param>
    private void SetAuth( OncRpcClientAuthBase auth )
    {
        if ( this._coreClient is not null ) this._coreClient.Auth = auth ?? new OncRpcClientAuthNone();
    }

    /// <summary>   Clears the authentication. </summary>
    private void ClearAuth()
    {
        if ( this._coreClient is not null ) this._coreClient.Auth = new OncRpcClientAuthNone();
    }

    /// <summary>
    /// Call remote procedure <see cref="RemoteProceduresVersion1.Nop"/>.
    /// </summary>
    public virtual void CallRemoteProcedureNull()
    {
        VoidXdrCodec request = VoidXdrCodec.VoidXdrCodecInstance;
        VoidXdrCodec result = VoidXdrCodec.VoidXdrCodecInstance;
        this.Call( this.Version == 1 ? ( int ) RemoteProceduresVersion1.Nop : ( int ) RemoteProceduresVersion2.Nop, this.Version, request, result );
    }

    /// <summary>   Call remote procedure <see cref="RemoteProceduresVersion1.Echo"/>. </summary>
    /// <param name="value">    parameter of type <see cref="string"/> to send to the remote
    ///                         procedure call. </param>
    /// <returns>   Result from remote procedure call (of type <see cref="string"/>). </returns>
    public virtual string CallRemoteProcedureEcho( string value )
    {
        StringXdrCodec request = new( value );
        StringXdrCodec result = new();
        this.Call( ( int ) RemoteProceduresVersion1.Echo, this.Version, request, result );
        return result.Value;
    }

    /// <summary>
    /// Call remote procedure <see cref="RemoteProceduresVersion1.ConcatenateInputParameters"/>.
    /// </summary>
    /// <param name="value">    parameter of type <see cref="StringVectorCodec"/> to send to the
    ///                         remote procedure call. </param>
    /// <returns>   Result from remote procedure call (of type <see cref="string"/>). </returns>
    public virtual string CallRemoteProcedureConcatenateInputParameters( StringVectorCodec value )
    {
        StringXdrCodec result = new();
        this.Call( ( int ) RemoteProceduresVersion1.ConcatenateInputParameters, this.Version, value, result );
        return result.Value;
    }

    /// <summary>
    /// Call remote procedure <see cref="RemoteProceduresVersion1.CompareInputToFoo"/>.
    /// </summary>
    /// <param name="value">    parameter of type <see cref="EnumFoo"/> to send to the remote
    ///                         procedure call. </param>
    /// <returns>   Result from remote procedure call (of type boolean). </returns>
    public virtual bool CallRemoteProcedureCompareInputToFoo( EnumFoo value )
    {
        IntXdrCodec request = new( ( int ) value );
        BooleanXdrCodec result = new();
        this.Call( ( int ) RemoteProceduresVersion1.CompareInputToFoo, this.Version, request, result );
        return result.Value;
    }

    /// <summary>
    /// Call remote procedure <see cref="RemoteProceduresVersion1.ReturnEnumFooValue"/>.
    /// </summary>
    /// <returns>   Result from remote procedure call (of type <see cref="EnumFoo"/>). </returns>
    public virtual int CallRemoteProcedureReturnEnumFooValue()
    {
        VoidXdrCodec request = VoidXdrCodec.VoidXdrCodecInstance;
        IntXdrCodec result = new();
        this.Call( ( int ) RemoteProceduresVersion1.ReturnEnumFooValue, this.Version, request, result );
        return result.Value;
    }

    /// <summary>
    /// Call remote procedure <see cref="RemoteProceduresVersion1.PrependLinkedList"/>.
    /// </summary>
    /// <param name="request">  the request of type <see cref="LinkedListCodec"/> to send to the
    ///                         remote procedure call. </param>
    /// <returns>   Result from remote procedure call (of type <see cref="LinkedListCodec"/>). </returns>
    public virtual LinkedListCodec CallRemoteProcedurePrependLinkedList( LinkedListCodec request )
    {
        LinkedListCodec result = new();
        this.Call( ( int ) RemoteProceduresVersion1.PrependLinkedList, this.Version, request, result );
        return result;
    }

    /// <summary>
    /// Call remote procedure <see cref="RemoteProceduresVersion1.RemoteProcedureReadSomeResult"/>.
    /// </summary>
    /// <returns>
    /// Result from remote procedure call (of type <see cref="SomeResultCodec"/>).
    /// </returns>
    public virtual SomeResultCodec CallRemoteProcedureReadSomeResult()
    {
        VoidXdrCodec request = VoidXdrCodec.VoidXdrCodecInstance;
        SomeResultCodec result = new();
        this.Call( ( int ) RemoteProceduresVersion1.RemoteProcedureReadSomeResult, this.Version, request, result );
        return result;
    }

    /// <summary>
    /// Call remote procedure <see cref="RemoteProceduresVersion2.ConcatenateTwoValues"/>.
    /// </summary>
    /// <param name="arg1"> parameter of type <see cref="string"/> to concatenate and send to the
    ///                     remote procedure call. </param>
    /// <param name="arg2"> parameter of type <see cref="string"/> to concatenate and send to the
    ///                     remote procedure call. </param>
    /// <returns>   Result from remote procedure call (of type <see cref="string"/>). </returns>
    public virtual string CallRemoteProcedureConcatenateTwoValues( string arg1, string arg2 )
    {
        DualStringsCodec request = new() {
            Arg1 = arg1,
            Arg2 = arg2
        };
        StringXdrCodec result = new();
        this.Call( ( int ) RemoteProceduresVersion2.ConcatenateTwoValues, RpcProgramConstants.Version2, request, result );
        return result.Value;
    }

    /// <summary>
    /// Call remote procedure <see cref="RemoteProceduresVersion2.ConcatenateThreeItems"/>.
    /// </summary>
    /// <param name="one">      parameter (of type <see cref="string"/>) to the remote procedure
    ///                         call. </param>
    /// <param name="two">      parameter (of type <see cref="string"/>) to the remote procedure
    ///                         call. </param>
    /// <param name="three">    parameter (of type <see cref="string"/>) to the remote procedure
    ///                         call. </param>
    /// <returns>   Result from remote procedure call (of type <see cref="string"/>). </returns>
    public virtual string CallRemoteProcedureConcatenatedThreeItems( string one, string two, string three )
    {
        TripleStringsCodec request = new() {
            One = one,
            Two = two,
            Three = three
        };
        StringXdrCodec result = new();
        this.Call( ( int ) RemoteProceduresVersion2.ConcatenateThreeItems, RpcProgramConstants.Version2, request, result );
        return result.Value;
    }

    /// <summary>
    /// Call remote procedure <see cref="RemoteProceduresVersion2.ReturnYouAreFooValue"/>.
    /// </summary>
    /// <param name="value">    parameter of type <see cref="EnumFoo"/> to send to the remote
    ///                         procedure call. </param>
    /// <returns>   Result from remote procedure call (of type <see cref="string"/>). </returns>
    public virtual string CallRemoteProcedureReturnYouAreFooValue( EnumFoo value )
    {
        IntXdrCodec request = new( ( int ) value );
        StringXdrCodec result = new();
        this.Call( ( int ) RemoteProceduresVersion2.ReturnYouAreFooValue, RpcProgramConstants.Version2, request, result );
        return result.Value;
    }

    /// <summary>
    /// Call remote procedure <see cref="RemoteProceduresVersion2.LinkListItems"/>.
    /// </summary>
    /// <param name="list1">    parameter of type <see cref="LinkedListCodec"/> to link and send to
    ///                         the remote procedure call. </param>
    /// <param name="list2">    parameter of type <see cref="LinkedListCodec"/> to link and send to
    ///                         the remote procedure call. </param>
    /// <returns>   Result from remote procedure call (of type <see cref="LinkedListCodec"/>). </returns>
    public virtual LinkedListCodec CallRemoteProcedureLinkListItems( LinkedListCodec list1, LinkedListCodec list2 )
    {
        DualLinkedListsCodec request = new() {
            List1 = list1,
            List2 = list2
        };
        LinkedListCodec result = new();
        this.Call( ( int ) RemoteProceduresVersion2.LinkListItems, RpcProgramConstants.Version2, request, result );
        return result;
    }

    /// <summary>   Call remote procedure <see cref="RemoteProceduresVersion2.ProcessFourArguments"/>. </summary>
    /// <param name="a">    parameter of type <see cref="string"/> to send to the remote procedure call. </param>
    /// <param name="b">    parameter of type <see cref="EnumFoo"/> to send to the remote procedure call. </param>
    /// <param name="c">    parameter of type <see cref="EnumFoo"/> to send to the remote procedure call. </param>
    /// <param name="d">    parameter <see cref="int"/> to send to the remote procedure call. </param>
    public virtual void CallRemoteProcedureProcessFourArguments( string a, int b, int c, int d )
    {
        StringTripleIntegerCodec request = new() {
            A = a,
            B = b,
            C = c,
            D = d
        };
        VoidXdrCodec result = VoidXdrCodec.VoidXdrCodecInstance;
        this.Call( ( int ) RemoteProceduresVersion2.ProcessFourArguments, RpcProgramConstants.Version2, request, result );
    }


    /// <summary>   Call authenticate. </summary>
    /// <param name="machineName">      Name of the machine. </param>
    /// <param name="userIdentity">     The user identity. </param>
    /// <param name="groupIdentity">    The group identity. </param>
    public void CallAuthenticate( string machineName, int userIdentity, int groupIdentity )
    {
        OncRpcClientAuthBase auth = new OncRpcClientAuthUnix( machineName, userIdentity, groupIdentity, Array.Empty<int>() );
        this.SetAuth( auth );
        try
        {
            this.CallRemoteProcedureNull();
            // may have to do an echo test?
            // this.CallRemoteProcedureEcho( "authenticate" );
        }
        catch
        {
            throw;
        }
        finally
        {
            this.ClearAuth();
        }
    }

    #endregion

}
