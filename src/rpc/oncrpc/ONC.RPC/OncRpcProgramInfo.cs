namespace cc.isr.ONC.RPC;

/// <summary>
/// The class <see cref="OncRpcProgramInfo"/> holds individual program and their
/// a program and their associated version numbers.
/// </summary>
/// <remarks> Used by the port mapper to enumerate the programs supported on the server. <para>
///  
/// Remote Tea authors: Harald Albrecht, Jay Walters.</para>
/// </remarks>
public class OncRpcProgramInfo
{
    /// <summary>   Constructor. </summary>
    /// <param name="program">  The program number of the ONC/RPC program handled by a server transport. </param>
    /// <param name="version">  Version number of the ONC/RPC program handled by the server. </param>
    public OncRpcProgramInfo( int program, int version )
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
