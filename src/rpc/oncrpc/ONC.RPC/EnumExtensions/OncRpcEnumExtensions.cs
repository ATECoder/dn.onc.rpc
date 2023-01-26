using System.ComponentModel;
using System.Reflection;

using cc.isr.ONC.RPC.Portmap;

namespace cc.isr.ONC.RPC.EnumExtensions;

/// <summary>   A support class for enum extensions. </summary>
public static class OncRpcEnumExtensions
{

    /// <summary>   Gets a description from an Enum. </summary>
    /// <param name="value">    An enum constant representing the value option. </param>
    /// <returns>   The description. </returns>
    public static string GetDescription( this Enum value )
    {
        return
            value
                .GetType()
                .GetMember( value.ToString() )
                .FirstOrDefault()
                ?.GetCustomAttribute<DescriptionAttribute>()
                ?.Description
            ?? value.ToString();
    }

    /// <summary>   An int extension method that converts a value to a message type. </summary>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="value">    An enum constant representing the enum value. </param>
    /// <returns>   An <see cref="OncRpcMessageType"/>. </returns>
    public static OncRpcMessageType ToMessageType( this int value )
    {
        return Enum.IsDefined( typeof( OncRpcMessageType ), value )
            ? ( OncRpcMessageType ) value
            : throw new ArgumentException( $"{typeof( int )} value of {value} cannot be cast to {nameof( OncRpcMessageType )}" );
    }

    /// <summary>   An int extension method that converts a value to a <see cref="OncRpcAuthStatus"/>. </summary>
    /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
    ///                                         illegal values. </exception>
    /// <param name="value">    An enum constant representing the enum value. </param>
    /// <returns>   Value as the OncRpcAuthStatus. </returns>
    public static OncRpcAuthStatus ToAuthStatus( this int value )
    {
        return Enum.IsDefined( typeof( OncRpcAuthStatus ), value )
            ? ( OncRpcAuthStatus ) value
            : throw new ArgumentException( $"{typeof( int )} value of {value} cannot be cast to {nameof( OncRpcAuthStatus )}" );
    }

    /// <summary>   An int extension method that converts a value to a <see cref="OncRpcAuthType"/>. </summary>
    /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
    ///                                         illegal values. </exception>
    /// <param name="value">    An enum constant representing the enum value. </param>
    /// <returns>   Value as the OncRpcAuthType. </returns>
    public static OncRpcAuthType ToAuthType( this int value )
    {
        return Enum.IsDefined( typeof( OncRpcAuthType ), value )
            ? ( OncRpcAuthType ) value
            : throw new ArgumentException( $"{typeof( int )} value of {value} cannot be cast to {nameof( OncRpcAuthType )}" );
    }

    /// <summary>   An int extension method that converts a value to an <see cref="OncRpcAcceptStatus"/>. </summary>
    /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
    ///                                         illegal values. </exception>
    /// <param name="value">    An enum constant representing the enum value. </param>
    /// <returns>   Value as the OncRpcAcceptStatus. </returns>
    public static OncRpcAcceptStatus ToAcceptStatus( this int value )
    {
        return Enum.IsDefined( typeof( OncRpcAcceptStatus ), value )
            ? ( OncRpcAcceptStatus ) value
            : throw new ArgumentException( $"{typeof( int )} value of {value} cannot be cast to {nameof( OncRpcAcceptStatus )}" );
    }

    /// <summary>   An int extension method that converts a value to a <see cref="OncRpcExceptionReason"/>. </summary>
    /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
    ///                                         illegal values. </exception>
    /// <param name="value">    An enum constant representing the enum value. </param>
    /// <returns>   Value as the OncRpcExceptionReason. </returns>
    public static OncRpcExceptionReason ToExceptionReason( this int value )
    {
        return Enum.IsDefined( typeof( OncRpcExceptionReason ), value )
            ? ( OncRpcExceptionReason ) value
            : throw new ArgumentException( $"{typeof( int )} value of {value} cannot be cast to {nameof( OncRpcExceptionReason )}" );
    }

    /// <summary>   An int extension method that converts a value to a <see cref="OncRpcProtocol"/>. </summary>
    /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
    ///                                         illegal values. </exception>
    /// <param name="value">    An enum constant representing the enum value. </param>
    /// <returns>   Value as the OncRpcProtocols. </returns>
    public static OncRpcProtocol ToProtocols( this int value )
    {
        return Enum.IsDefined( typeof( OncRpcProtocol ), value )
            ? ( OncRpcProtocol ) value
            : throw new ArgumentException( $"{typeof( int )} value of {value} cannot be cast to {nameof( OncRpcProtocol )}" );
    }

    /// <summary>   An int extension method that converts a value to a <see cref="OncRpcRejectStatus"/>. </summary>
    /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
    ///                                         illegal values. </exception>
    /// <param name="value">    An enum constant representing the enum value. </param>
    /// <returns>   Value as the OncRpcRejectStatus. </returns>
    public static OncRpcRejectStatus ToRejectStatus( this int value )
    {
        return Enum.IsDefined( typeof( OncRpcRejectStatus ), value )
            ? ( OncRpcRejectStatus ) value
            : throw new ArgumentException( $"{typeof( int )} value of {value} cannot be cast to {nameof( OncRpcRejectStatus )}" );
    }

    /// <summary>   An int extension method that converts a value to a <see cref="OncRpcReplyStatus"/>. </summary>
    /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
    ///                                         illegal values. </exception>
    /// <param name="value">    An enum constant representing the enum value. </param>
    /// <returns>   Value as the OncRpcReplyStatus. </returns>
    public static OncRpcReplyStatus ToReplyStatus( this int value )
    {
        return Enum.IsDefined( typeof( OncRpcReplyStatus ), value )
            ? ( OncRpcReplyStatus ) value
            : throw new ArgumentException( $"{typeof( int )} value of {value} cannot be cast to {nameof( OncRpcReplyStatus )}" );
    }

    /// <summary>   An int extension method that converts a value to a <see cref="OncRpcRetransmitMode"/>. </summary>
    /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
    ///                                         illegal values. </exception>
    /// <param name="value">    An enum constant representing the enum value. </param>
    /// <returns>   Value as the OncRpcRetransmitMode. </returns>
    public static OncRpcRetransmitMode ToRetransmitMode( this int value )
    {
        return Enum.IsDefined( typeof( OncRpcRetransmitMode ), value )
            ? ( OncRpcRetransmitMode ) value
            : throw new ArgumentException( $"{typeof( int )} value of {value} cannot be cast to {nameof( OncRpcRetransmitMode )}" );
    }

    /// <summary>   An int extension method that converts a value to a <see cref="OncRpcPortmapServiceProcedure"/>. </summary>
    /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
    ///                                         illegal values. </exception>
    /// <param name="value">    An enum constant representing the enum value. </param>
    /// <returns>   Value as the OncRpcPortmapServiceProcedure. </returns>
    public static OncRpcPortmapServiceProcedure ToPortmapServiceProcedure( this int value )
    {
        return Enum.IsDefined( typeof( OncRpcPortmapServiceProcedure ), value )
            ? ( OncRpcPortmapServiceProcedure ) value
            : throw new ArgumentException( $"{typeof( int )} value of {value} cannot be cast to {nameof( OncRpcPortmapServiceProcedure )}" );
    }

}
