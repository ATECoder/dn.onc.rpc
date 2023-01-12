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
/// A collection of remote procedures for ONC/RPC program version 1.
/// </summary>
public enum RemoteProceduresVersion1
{
    /// <summary>   The no-operation (Null) remote procedure. </summary>
    [System.ComponentModel.Description( "The no-operation (Null) remote procedure null" )]
    Nop = 0,

    /// <summary>   The echo remote procedure. </summary>
    [System.ComponentModel.Description( "The echo remote procedure" )]
    Echo = 1,

    /// <summary>   The remote procedure for concatenating the input parameters. </summary>
    [System.ComponentModel.Description( "The remote procedure for concatenating the input parameters." )]
    ConcatenateInputParameters = 2,

    /// <summary>   The remote procedure for comparing the input parameters to <see cref="EnumFoo.FOO"/>. </summary>
    [System.ComponentModel.Description( "The remote procedure for comparing the input parameters to EnumFoo.FOO" )]
    CompareInputToFoo = 3,

    /// <summary>   The remote procedure for returning the <see cref="EnumFoo.FOO"/> value. </summary>
    [System.ComponentModel.Description( "The remote procedure for returning the EnumFoo.FOO value." )]
    ReturnEnumFooValue = 4,

    /// <summary>   The remote procedure for building a linked list. </summary>
    [System.ComponentModel.Description( "The remote procedure for building a linked list." )]
    BuildLinkedList = 5,

    /// <summary>   The first remote procedure read. </summary>
    [System.ComponentModel.Description( "The first remote procedure read." )]
    RemoteProcedureReadSomeResult = 42,

}

/// <summary>   A class that enumerates the remote procedures for ONC/RPC program version 2. </summary>
public enum RemoteProceduresVersion2
{
    /// <summary>   The no-operation (Null) remote procedure null. </summary>
    [System.ComponentModel.Description( "The no-operation (Null) remote procedure null." )]
    Nop = 0,

    /// <summary>   The remote procedure for returning the <see cref="EnumFoo.FOO"/> preceded with 'You Are Foo'. </summary>
    [System.ComponentModel.Description( "The remote procedure for returning the 'EnumFoo.FOO' preceded with 'You Are Foo'." )]
    ReturnYouAreFooValue = 3,

    /// <summary>   The remote procedure for concatenating two values. </summary>
    [System.ComponentModel.Description( "The remote procedure for concatenating two values." )]
    ConcatenateTwoValues = 42,

    /// <summary>   The remote procedure for concatenating three items. </summary>
    [System.ComponentModel.Description( "The remote procedure for concatenating three items." )]
    ConcatenateThreeItems = 43,

    /// <summary>   The remote procedure for linking to linked lists. </summary>
    [System.ComponentModel.Description( "The remote procedure for linking to linked lists." )]
    LinkListItems = 55,

    /// <summary>   The remote procedure for processing four arguments. </summary>
    [System.ComponentModel.Description( "The remote procedure for processing four arguments." )]
    ProcessFourArguments = 100,

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
public enum EnumFoo
{
    /// <summary>   the foo. </summary>
    [System.ComponentModel.Description( "The FOO" )]
    FOO = 0,

    /// <summary>   the bar. </summary>
    [System.ComponentModel.Description( "The BAR" )]
    BAR = 1,
}


