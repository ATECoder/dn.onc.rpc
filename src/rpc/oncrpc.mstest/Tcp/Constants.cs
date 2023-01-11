
namespace cc.isr.ONC.RPC.MSTest.Tcp;

/// <summary>   A collection of constants used by the "Gen" ONC/RPC program. </summary>
public static class RpcProgramConstants
{
    /// <summary>   (Immutable) the RPC gen program number. </summary>
    public const int ProgramNumber = 0x20049678;
    /// <summary>   (Immutable) the RPC program version one. </summary>
    public const int Version1 = 1;
    /// <summary>   (Immutable) the RPC program version two. </summary>
    public const int Version2 = 2;
}

/// <summary>
/// A class that enumerates the remote procedures for ONC/RPC program version 1.
/// </summary>
public static class RemoteProceduresVersion1
{
    /// <summary>   (Immutable) the no-operation (Null) remote procedure null. </summary>
    public const int Nop = 0;

    /// <summary>   (Immutable) the echo remote procedure. </summary>
    public const int Echo = 1;

    /// <summary>   (Immutable) the remote procedure for concatenating the input parameters. </summary>
    public const int ConcatenateInputParameters = 2;

    /// <summary>   (Immutable) the remote procedure for comparing the input parameters to <see cref="EnumFoo.FOO"/>. </summary>
    public const int CompareInputToFoo = 3;

    /// <summary>   (Immutable) the remote procedure for returning the <see cref="EnumFoo.FOO"/> value. </summary>
    public const int ReturnEnumFooValue = 4;

    /// <summary>   (Immutable) the remote procedure for building a linked list. </summary>
    public const int BuildLinkedList = 5;

    /// <summary>   (Immutable) the first remote procedure read. </summary>
    public const int RemoteProcedureReadSomeResult = 42;

}

/// <summary>   A class that enumerates the remote procedures for ONC/RPC program version 2. </summary>
public static class RemoteProceduresVersion2
{
    /// <summary>   (Immutable) the no-operation (Null) remote procedure null. </summary>
    public const int Nop = 0;

    /// <summary>   (Immutable) the remote procedure for returning the <see cref="EnumFoo.FOO"/> preceded with 'You Are Foo'. </summary>
    public const int ReturnYouAreFooValue = 3;

    /// <summary>   (Immutable) the remote procedure for concatenating two values. </summary>
    public const int ConcatenateTwoValues = 42;

    /// <summary>   (Immutable) the remote procedure for concatenating three items. </summary>
    public const int ConcatenateThreeItems = 43;

    /// <summary>   (Immutable) the remote procedure for linking to linked lists. </summary>
    public const int LinkListItems = 55;

    /// <summary>   (Immutable) the remote procedure for processing four arguments. </summary>
    public const int ProcessFourArguments = 100;

}

public static class AuthenticationConstants
{

    /// <summary>   (Immutable) name of the machine. </summary>
    public const string MachineName = "limedevb";

    /// <summary>   (Immutable) the user identity. </summary>
    public const int UserIdentity = 42;

    /// <summary>   (Immutable) the group identity. </summary>
    public const int GroupIdentity = 815;

}

/// <summary>   Enumeration (collection of constants). </summary>
public class EnumFoo
{
    /// <summary>   (Immutable) the foo. </summary>
    public const int FOO = 0;

    /// <summary>   (Immutable) the bar. </summary>
    public const int BAR = 1;
}


