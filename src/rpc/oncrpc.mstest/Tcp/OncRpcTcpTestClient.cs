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
        this._coreClient = OncRpcClientBase.NewOncRpcClient( host, RpcProgramConstants.ProgramNumber, version, 0, OncRpcProtocols.OncRpcTcp );

        // this._coreClient = new GenOncRpcCoreClient( host, RpcProgramConstants.ProgramNumber, version, OncRpcProtocols.ONCRPC_TCP );
        this.Connected = true;
    }

    /// <summary>   Gets or sets a value indicating whether the ONC/RPC client is connected. </summary>
    /// <value> True if connected, false if not. </value>
    public bool Connected { get; private set; }

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
        this._coreClient?.Close();
        this._coreClient = null;
    }

    #endregion

    #region " Properties "

    /// <summary>   Gets or sets the version. </summary>
    /// <value> The version. </value>
    public int Version { get; private set; }

    /// <summary>   Gets or sets the host. </summary>
    /// <value> The host. </value>
    public IPAddress Host { get; private set; } = IPAddress.Loopback;

    #endregion

    #region " Procedure Calls "

    /// <summary>   Calls the specified procedure </summary>
    /// <param name="procedureNumber">  The procedure number. </param>
    /// <param name="versionNumber">    The version number. </param>
    /// <param name="parameters">       Options for controlling the operation. </param>
    /// <param name="result">           The result. </param>
    private void Call( int procedureNumber, int versionNumber, IXdrCodec parameters, IXdrCodec result )
    {
        this._coreClient?.Call( procedureNumber, versionNumber, parameters, result );
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
        VoidXdrCodec args = VoidXdrCodec.VoidXdrCodecInstance;
        VoidXdrCodec result = VoidXdrCodec.VoidXdrCodecInstance;
        this.Call( this.Version == 1 ? RemoteProceduresVersion1.Nop : RemoteProceduresVersion2.Nop, this.Version, args, result );
    }

    /// <summary>
    /// Call remote procedure <see cref="RemoteProceduresVersion1.Echo"/>.
    /// </summary>
    /// <param name="arg1"> parameter (of type <see cref="string"/>) to the remote procedure call. </param>
    /// <returns>   Result from remote procedure call (of type String). </returns>
    public virtual string CallRemoteProcedureEcho( string arg1 )
    {
        StringXdrCodec args = new( arg1 );
        StringXdrCodec result = new();
        this.Call( RemoteProceduresVersion1.Echo, this.Version, args, result );
        return result.Value;
    }

    /// <summary>
    /// Call remote procedure <see cref="RemoteProceduresVersion1.ConcatenateInputParameters"/>.
    /// </summary>
    /// <param name="arg1"> parameter (of type <see cref="StringVectorCodec"/>) to the remote
    ///                     procedure call. </param>
    /// <returns>   Result from remote procedure call (of type String). </returns>
    public virtual string CallRemoteProcedureConcatenateInputParameters( StringVectorCodec arg1 )
    {
        StringXdrCodec result = new();
        this.Call( RemoteProceduresVersion1.ConcatenateInputParameters, this.Version, arg1, result );
        return result.Value;
    }

    /// <summary>
    /// Call remote procedure <see cref="RemoteProceduresVersion1.CompareInputToFoo"/>.
    /// </summary>
    /// <param name="arg1"> parameter (of type <see cref="EnumFoo"/> ) to the remote procedure call. </param>
    /// <returns>   Result from remote procedure call (of type boolean). </returns>
    public virtual bool CallRemoteProcedureCompareInputToFoo( int arg1 )
    {
        IntXdrCodec args = new( arg1 );
        BooleanXdrCodec result = new();
        this.Call( RemoteProceduresVersion1.CompareInputToFoo, this.Version, args, result );
        return result.Value;
    }

    /// <summary>
    /// Call remote procedure <see cref="RemoteProceduresVersion1.ReturnEnumFooValue"/>.
    /// </summary>
    /// <returns>   Result from remote procedure call (of type <see cref="EnumFoo"/>). </returns>
    public virtual int CallRemoteProcedureReturnEnumFooValue()
    {
        VoidXdrCodec args = VoidXdrCodec.VoidXdrCodecInstance;
        IntXdrCodec result = new();
        this.Call( RemoteProceduresVersion1.ReturnEnumFooValue, this.Version, args, result );
        return result.Value;
    }

    /// <summary>
    /// Call remote procedure <see cref="RemoteProceduresVersion1.BuildLinkedList"/>.
    /// </summary>
    /// <param name="arg1"> parameter (of type <see cref="LinkedListCodec"/>) to the remote
    ///                     procedure call. </param>
    /// <returns>
    /// Result from remote procedure call (of type <see cref="LinkedListCodec"/>).
    /// </returns>
    public virtual LinkedListCodec CallRemoteProcedureBuildLinkedList( LinkedListCodec arg1 )
    {
        LinkedListCodec result = new();
        this.Call( RemoteProceduresVersion1.BuildLinkedList, this.Version, arg1, result );
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
        VoidXdrCodec args = VoidXdrCodec.VoidXdrCodecInstance;
        SomeResultCodec result = new();
        this.Call( RemoteProceduresVersion1.RemoteProcedureReadSomeResult, this.Version, args, result );
        return result;
    }

    /// <summary>   Call remote procedure <see cref="RemoteProceduresVersion2.ConcatenateTwoValues"/>. </summary>
    /// <param name="arg1"> parameter (of type <see cref="string"/>) to the remote procedure call. </param>
    /// <param name="arg2"> parameter (of type <see cref="string"/>) to the remote procedure call. </param>
    /// <returns>   Result from remote procedure call (of type <see cref="string"/>). </returns>
    public virtual string CallRemoteProcedureConcatenateTwoValues( string arg1, string arg2 )
    {
        DualStringsCodec args = new() {
            Arg1 = arg1,
            Arg2 = arg2
        };
        StringXdrCodec result = new();
        this.Call( RemoteProceduresVersion2.ConcatenateTwoValues, RpcProgramConstants.Version2, args, result );
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
        TripleStringsCodec args = new() {
            One = one,
            Two = two,
            Three = three
        };
        StringXdrCodec result = new();
        this.Call( RemoteProceduresVersion2.ConcatenateThreeItems, RpcProgramConstants.Version2, args, result );
        return result.Value;
    }

    /// <summary>
    /// Call remote procedure <see cref="RemoteProceduresVersion2.ReturnYouAreFooValue"/>.
    /// </summary>
    /// <param name="foo">  parameter (of type ENUMFOO) to the remote procedure call. </param>
    /// <returns>   Result from remote procedure call (of type String). </returns>
    public virtual string CallRemoteProcedureReturnYouAreFooValue( int foo )
    {
        IntXdrCodec args = new( foo );
        StringXdrCodec result = new();
        this.Call( RemoteProceduresVersion2.ReturnYouAreFooValue, RpcProgramConstants.Version2, args, result );
        return result.Value;
    }

    /// <summary>
    /// Call remote procedure <see cref="RemoteProceduresVersion2.LinkListItems"/>.
    /// </summary>
    /// <param name="list1">    parameter (of type <see cref="LinkedListCodec"/>) to the remote
    ///                         procedure call. </param>
    /// <param name="list2">    parameter (of type <see cref="LinkedListCodec"/>) to the remote
    ///                         procedure call. </param>
    /// <returns>
    /// Result from remote procedure call (of type <see cref="LinkedListCodec"/>).
    /// </returns>
    public virtual LinkedListCodec CallRemoteProcedureLinkListItems( LinkedListCodec list1, LinkedListCodec list2 )
    {
        DualLinkedListsCodec args = new() {
            List1 = list1,
            List2 = list2
        };
        LinkedListCodec result = new();
        this.Call( RemoteProceduresVersion2.LinkListItems, RpcProgramConstants.Version2, args, result );
        return result;
    }

    /// <summary>   Call remote procedure <see cref="RemoteProceduresVersion2.ProcessFourArguments"/>. </summary>
    /// <param name="a">    parameter (of type String) to the remote procedure call. </param>
    /// <param name="b">    parameter (of type ENUMFOO) to the remote procedure call. </param>
    /// <param name="c">    parameter (of type ENUMFOO) to the remote procedure call. </param>
    /// <param name="d">    parameter (of type int) to the remote procedure call. </param>
    public virtual void CallRemoteProcedureProcessFourArguments( string a, int b, int c, int d )
    {
        StringTripleIntegerCodec args = new() {
            A = a,
            B = b,
            C = c,
            D = d
        };
        VoidXdrCodec result = VoidXdrCodec.VoidXdrCodecInstance;
        this.Call( RemoteProceduresVersion2.ProcessFourArguments, RpcProgramConstants.Version2, args, result );
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
