using cc.isr.ONC.RPC.Portmap;

namespace cc.isr.ONC.RPC.Server;

/// <summary>
/// Tags classes as being able to dispatch and handle ONC/RPC requests from clients.
/// </summary>
/// <remarks>
/// This interface is used as follows for dispatching and handling ONC/RPC calls: 
/// <list type="bullet"> <item>
/// First check which procedure the client intends to call. This information
/// is delivered through the <c>procedure</c> parameter. In case you do not handle multiple
/// programs within the same dispatcher, you can ignore the <c>program</c> parameter as
/// well as <c>version</c>. </item><item>
/// Retrieve appropriate parameters for this intended procedure using the
/// <see cref="OncRpcCallHandler.RetrieveCall(IXdrCodec)"/>
/// method of the <see cref="OncRpcCallHandler"/> object also supplied to the
/// dispatcher through the <c>call</c> parameter. </item><item>
/// Do whatever you need to do for this ONC/RPC call and make up an appropriate reply to be sent
/// back to the client in the next step.</item><item>
/// Sends back the reply by calling the <see cref="OncRpcCallHandler.Reply(IXdrCodec)"/>
/// method of the <see cref="OncRpcCallHandler"/> object </item> </list> <para>
/// 
/// Here's a simple example only showing how to handle the famous
/// procedure <c>0</c>: this is the "ping" procedure which can be used to test whether the
/// server is still living. The example also shows how to handle calls for procedures which are
/// not implemented (not defined) by calling
/// <see cref="OncRpcCallHandler.ReplyProcedureNotAvailable()"/>.</para> <para>
/// 
/// In case the dispatcher throws an exception, the affected ONC/RPC server
/// transport will send a system error indication
/// <see cref="OncRpcCallHandler.ReplySystemError()"/>
/// to the client. No error indication will be sent if the exception resulted from an I/O
/// problem. Note that if you do not explicitly Sends back a reply, no reply is sent at all,
/// making batched calls possible. </para>
/// <code>
/// public void DispatchOncRpcCall(OncRpcCallInformation call, int program, int version, int procedure)
///   switch ( procedure ) 
///   {
///     case 0:
///       XdrVoid v = new XdrVoid();
///       call.RetrieveCall(v);
///       call.Reply(v);
///       break;
///     default:
///       call.FailProcedureUnavailable();
///   }
/// }
/// </code>
/// In addition, there are also lower-level methods available for retrieving parameters and
/// sending replies, in case you need to break up deserialization and serialization into several
/// steps. The following code snipped shows how to use them. Here, the objects <c>Foo</c>
/// and <c>Bar</c> represents call parameter objects, while <c>Baz</c> and <c>Blah</c>
/// are used to sent back the reply data.
/// <code>
/// public void DispatchOncRpcCall(OncRpcCallInformation call, int program, int version, int procedure)
///   switch ( procedure ) {
///     case 42:
///       // Retrieve call parameters.
///       XdrDecodingStream decoder = call.GetXdrDecodingStream();
///       foo.Decode(decoder);
///       bar.Decode(decoder);
///       call.EndDecoding();
///       // Handle particular ONC/RPC call...
///       // Sends back reply.
///       call.BeginEncoding();
///       XdrEncodingStream encoder = call.GetXdrEncodingStream();
///       baz.Encode(encoder);
///       blah.Encode(encoder);
///       call.EndEncoding();
///       break;
///     }
/// }
/// </code> <para>
/// 
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public interface IOncRpcDispatchable
{
    /// <summary>   Dispatch (handle) an ONC/RPC request from a client. </summary>
    /// <remarks>
    /// This interface has some fairly deep semantics, so please read the description above for how
    /// to use it properly. For background information about fairly deep semantics, please also refer
    /// to <i>Gigzales</i>, <i>J</i>.: Semantics considered harmful. Addison-Reilly, 1992, ISBN 0-542-
    /// 10815-X. <para>
    /// 
    /// See the introduction to this class for examples of how to use
    /// this interface properly.</para>
    /// </remarks>
    /// <param name="call">         <see cref="OncRpcCallHandler"/>
    ///                             about the call to handle, like the caller's Internet
    ///                             address, the ONC/RPC call header, etc. </param>
    /// <param name="program">      Program number requested by client. </param>
    /// <param name="version">      Version number requested. </param>
    /// <param name="procedure">    Procedure number requested. </param>
    void DispatchOncRpcCall( OncRpcCallHandler call, int program, int version, int procedure );
}
