
using System.Collections.Generic;

namespace cc.isr.ONC.RPC.Codecs;

/// <summary>
/// Objects of class <see cref="OncRpcPortmapServersListCodec"/> represent the outcome of the 
/// <see cref="Portmap.OncRpcPortmapServiceProcedure.OncRpcPortmapListServersInfo"/>
/// operation on a portmapper.
/// </summary>
/// <remarks> <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public class OncRpcPortmapServersListCodec : IXdrCodec
{
    /// <summary>
    /// Gets or sets or set the list of server <see cref="OncRpcServerIdentifierCodec"/> objects
    /// describing the currently registered ONC/RPC servers.
    /// </summary>
    /// <value> A list of identifiers of the servers. </value>
    public List<OncRpcServerIdentifierCodec> ServerIdentifiers { get; set; }

    /// <summary>   Initialize an <see cref="OncRpcPortmapServersListCodec"/> object. </summary>
    /// <remarks>
    /// Afterwards, the <see cref="ServerIdentifiers"/> field is initialized to contain no elements.
    /// </remarks>
    public OncRpcPortmapServersListCodec()
    {
        this.ServerIdentifiers = new List<OncRpcServerIdentifierCodec>();
    }

    /// <summary>
    /// Encodes -- that is: serializes -- the result of a 
    /// <see cref="Portmap.OncRpcPortmapServiceProcedure.OncRpcPortmapListServersInfo"/> operating into a XDR stream.
    /// </summary>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    ///
    /// <exception cref="OncRpcException">          Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="System.IO.IOException">    Thrown when an I/O error condition occurs. </exception>
    public virtual void Encode( XdrEncodingStreamBase encoder )
    {
        if ( this.ServerIdentifiers == null )
            encoder.EcodeBoolean( false );
        else
        {

            // Now encode all server <see cref="OncRpcServerIdent"/> objects into the XDR stream. Each
            // object is preceded by a boolean, which indicates to the receiver
            // whether an object follows. After the last object has been
            // encoded the receiver will find a boolean false in the stream.
            int count = this.ServerIdentifiers.Count;
            int index = 0;
            while ( count > 0 )
            {
                encoder.EcodeBoolean( true );
                this.ServerIdentifiers[index].Encode( encoder );
                index++;
                count--;
            }
            encoder.EcodeBoolean( false );
        }
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- the result from a 
    /// <see cref="Portmap.OncRpcPortmapServiceProcedure.OncRpcPortmapListServersInfo"/> remote procedure call from a
    /// XDR stream.
    /// </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    ///
    /// <exception cref="OncRpcException">          Thrown when an ONC/RPC error condition occurs. </exception>
    /// <exception cref="System.IO.IOException">    Thrown when an I/O error condition occurs. </exception>
    public virtual void Decode( XdrDecodingStreamBase decoder )
    {

        this.ServerIdentifiers.Clear();

        // Pull the server identifier Codec off the XDR stream. Each object is
        // preceded by a boolean value indicating whether there is still an
        // object in the pipe.

        while ( decoder.DecodeBoolean() )
            this.ServerIdentifiers.Add( new OncRpcServerIdentifierCodec( decoder ) );
    }
}
