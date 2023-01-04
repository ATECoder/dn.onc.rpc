namespace cc.isr.ONC.RPC.Server;

/// <summary>
/// The class <see cref="OncRpcServerTransportRegistrationInfo"/> holds information about
/// (possibly multiple) registration of server transports for individual program and version
/// numbers.
/// </summary>
/// <remarks> <para>
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public class OncRpcServerTransportRegistrationInfo
{
    /// <summary>   Constructor. </summary>
    /// <param name="program">  The program number of the ONC/RPC program handled by a server
    ///                         transport. </param>
    /// <param name="version">  Version number of the ONC/RPC program handled. </param>
    public OncRpcServerTransportRegistrationInfo( int program, int version )
    {
        this.Program = program;
        this.Version = version;
    }

    /// <summary>   Gets or sets the program number of the ONC/RPC program handled by a server. </summary>
    /// <value> The program. </value>
    public int Program { get; set; }

    /// <summary>   Gets or sets the version number of the ONC/RPC program. </summary>
    /// <value> The version. </value>
    public int Version { get; set; }
}
