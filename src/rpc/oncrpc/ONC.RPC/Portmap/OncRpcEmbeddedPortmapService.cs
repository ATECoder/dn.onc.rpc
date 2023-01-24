using cc.isr.ONC.RPC.Codecs;

namespace cc.isr.ONC.RPC.Portmap
{
    /// <summary>   A service for accessing embedded Portmap information. </summary>
    /// <remarks>   2023-01-23. </remarks>
    public class OncRpcEmbeddedPortmapService : OncRpcPortMapService
    {
        /// <summary>   Creates a new instance of an embeddable portmap service. </summary>
        public OncRpcEmbeddedPortmapService()
        {
        }

        /// <summary>
        /// Deregister all port settings for a particular (program, version) for all transports (TCP, UDP,
        /// etc.).
        /// </summary>
        /// <remarks>
        /// This method basically falls back to the implementation provided by the <c>rpcgen</c>
        /// superclass, but checks whether there are other ONC/RPC programs registered. If not, it
        /// signals itself to shut down the portmap service.
        /// </remarks>
        /// <param name="serverIdentification"> the server identification, which includes the program and
        ///                                     version to deregister. The protocol and port fields are
        ///                                     not used. </param>
        /// <returns>   <see langword="true"/> if deregistration succeeded. </returns>
        internal override BooleanXdrCodec UnsetPort( OncRpcServerIdentifierCodec serverIdentification )
        {
            BooleanXdrCodec ok = base.UnsetPort( serverIdentification );
            if ( ok.Value )
            {

                // Check for registered programs other than OncRpcPortmapConstants.OncRpcPortmapProgramNumber.

                bool onlyPmap = true;
                foreach ( OncRpcServerIdentifierCodec codec in this.ServerIdentifierCodecs)
                {
                    // check if the server has registered programs other than a portmap 
                    if ( codec.Program != OncRpcPortmapConstants.OncRpcPortmapProgramNumber )
                    {
                        onlyPmap = false;
                        break;
                    }
                }

                // If only portmap-related entries are left, then shut down this portmap service
                // by sending the shutdown signal
                if ( onlyPmap && this.Running )
                    this.StopRpcProcessing();
            }
            return ok;
        }
    }
}
