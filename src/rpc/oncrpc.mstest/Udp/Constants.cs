
namespace cc.isr.ONC.RPC.MSTest.Udp;

/// <summary>   A collection of constants used by the "Gen" ONC/RPC program. </summary>
/// <remarks>   2022-12-22. </remarks>
public static class RpcProgramConstants
{
    /// <summary>   (Immutable) the RPC gen program number. </summary>
    public const int ProgramNumber = 0x20049678;
    /// <summary>   (Immutable) the RPC program version one. </summary>
    public const int Version = 1;
}

/// <summary>
/// A class that enumerates the remote procedures for ONC/RPC program.
/// </summary>
/// <remarks>   2022-12-28. </remarks>
public static class RemoteProcedures
{
    /// <summary>   (Immutable) the no-operation (Null) remote procedure null. </summary>
    public const int Nop = 0;

    /// <summary>   (Immutable) the Echo remote procedure null. </summary>
    public const int Echo = 2;

    /// <summary>   (Immutable) the request server shutdown procedure. </summary>
    public const int RequestServerShutdown = 42;

}

