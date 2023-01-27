using System.Runtime.CompilerServices;

namespace cc.isr.ONC.RPC.Logging;

public interface ILogger
{

    public abstract void Write( LogLevel level, string message );

    public abstract bool IsEnabled( LogLevel level );

}

public enum LogLevel
{ None, Error, Warning, Information, Verbose }

/// <summary>   A logging extensions. </summary>
public static class LoggingExtensions
{

    /// <summary>   An ILogger extension method that logs a verbose. </summary>
    /// <param name="logger">   The logger to act on. </param>
    /// <param name="message">  The message. </param>
    public static void LogVerbose( this ILogger logger, string message )
    {
        if ( logger.IsEnabled( LogLevel.Verbose ) )
            logger.Write( LogLevel.Verbose, BuildMessage( message ) );
    }

    /// <summary>   An ILogger extension method that logs member verbose. </summary>
    /// <param name="logger">           The logger to act on. </param>
    /// <param name="message">          The message. </param>
    /// <param name="memberName">       (Optional) Name of the member. </param>
    /// <param name="sourceFilePath">   (Optional) Full pathname of the source file. </param>
    /// <param name="sourceLineNumber"> (Optional) Source line number. </param>
    public static void LogMemberVerbose( this ILogger logger, string message, [CallerMemberName] string memberName = "",
                                                                           [CallerFilePath] string sourceFilePath = "",
                                                                           [CallerLineNumber] int sourceLineNumber = 0 )
    {
        if ( logger.IsEnabled( LogLevel.Verbose ) )
            logger.Write( LogLevel.Error, BuildMessage( message, memberName, sourceFilePath, sourceLineNumber ) );
    }

    /// <summary>   An ILogger extension method that logs an information. </summary>
    /// <param name="logger">   The logger to act on. </param>
    /// <param name="message">  The message. </param>
    public static void LogInformation( this ILogger logger, string message )
    {
        if ( logger.IsEnabled( LogLevel.Information ) )
            logger.Write( LogLevel.Information, BuildMessage( message ) );
    }

    /// <summary>   An ILogger extension method that logs member information. </summary>
    /// <param name="logger">           The logger to act on. </param>
    /// <param name="message">          The message. </param>
    /// <param name="memberName">       (Optional) Name of the member. </param>
    /// <param name="sourceFilePath">   (Optional) Full pathname of the source file. </param>
    /// <param name="sourceLineNumber"> (Optional) Source line number. </param>
    public static void LogMemberInfo( this ILogger logger, string message, [CallerMemberName] string memberName = "",
                                                                           [CallerFilePath] string sourceFilePath = "",
                                                                           [CallerLineNumber] int sourceLineNumber = 0 )
    {
        if ( logger.IsEnabled( LogLevel.Information ) )
            logger.Write( LogLevel.Error, BuildMessage( message, memberName, sourceFilePath, sourceLineNumber ) );
    }

    /// <summary>   An ILogger extension method that logs a warning. </summary>
    /// <param name="logger">   The logger to act on. </param>
    /// <param name="message">  The message. </param>
    public static void LogWarning( this ILogger logger, string message )
    {
        if ( logger.IsEnabled( LogLevel.Warning ) )
            logger.Write( LogLevel.Warning, BuildMessage( message ) );
    }

    /// <summary>   An ILogger extension method that logs member warning. </summary>
    /// <param name="logger">           The logger to act on. </param>
    /// <param name="message">          The message. </param>
    /// <param name="memberName">       (Optional) Name of the member. </param>
    /// <param name="sourceFilePath">   (Optional) Full pathname of the source file. </param>
    /// <param name="sourceLineNumber"> (Optional) Source line number. </param>
    public static void LogMemberWarning( this ILogger logger, string message, [CallerMemberName] string memberName = "",
                                                                           [CallerFilePath] string sourceFilePath = "",
                                                                           [CallerLineNumber] int sourceLineNumber = 0 )
    {
        if ( logger.IsEnabled( LogLevel.Warning ) )
            logger.Write( LogLevel.Error, BuildMessage( message, memberName, sourceFilePath, sourceLineNumber ) );
    }

    /// <summary>   An ILogger extension method that writes an error. </summary>
    /// <param name="logger">   The logger to act on. </param>
    /// <param name="message">  The message. </param>
    /// <param name="ex">       The exception. </param>
    public static void LogError( this ILogger logger, string message, Exception ex )
    {
        if ( logger.IsEnabled( LogLevel.Error ) )
            logger.Write( LogLevel.Error, BuildMessage( message, ex ) );
    }

    /// <summary>   Console write exception. </summary>
    /// <param name="logger">           The logger to act on. </param>
    /// <param name="message">          The message. </param>
    /// <param name="ex">               The exception. </param>
    /// <param name="memberName">       (Optional) Name of the member. </param>
    /// <param name="sourceFilePath">   (Optional) Full pathname of the source file. </param>
    /// <param name="sourceLineNumber"> (Optional) Source line number. </param>
    public static void LogMemberError( this ILogger logger, string message, Exception ex, [CallerMemberName] string memberName = "",
                                                                                            [CallerFilePath] string sourceFilePath = "",
                                                                                            [CallerLineNumber] int sourceLineNumber = 0 )
    {
        if ( logger.IsEnabled( LogLevel.Error ) )
            logger.Write( LogLevel.Error, BuildMessage( message, ex, memberName, sourceFilePath, sourceLineNumber ) );
    }

    /// <summary>   Builds exception message. </summary>
    /// <remarks>   2023-01-20. </remarks>
    /// <param name="message">  The message. </param>
    /// <returns>   A string. </returns>
    public static string BuildMessage( string message )
    {
        return $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff},{message}";
    }

    /// <summary>   Builds exception message. </summary>
    /// <remarks>   2023-01-20. </remarks>
    /// <param name="message">          The message. </param>
    /// <param name="memberName">       Name of the member. </param>
    /// <param name="sourceFilePath">   Full pathname of the source file. </param>
    /// <param name="sourceLineNumber"> Source line number. </param>
    /// <returns>   A string. </returns>
    public static string BuildMessage( string message, string memberName, string sourceFilePath, int sourceLineNumber )
    {
        int indent = 4;
        StringBuilder builder = new();
        _ = builder.Append( $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff},{memberName}({sourceLineNumber})" );
        if ( !string.IsNullOrEmpty( message ) ) _ = builder.AppendLine( $"{new string( ' ', indent )}{message}" );
        if ( !string.IsNullOrEmpty( sourceFilePath ) ) _ = builder.AppendLine( $"{new string( ' ', indent )}{sourceFilePath}" );
        return builder.ToString();
    }

    /// <summary>   Builds exception message. </summary>
    /// <param name="message">  The message. </param>
    /// <param name="ex">       The exception. </param>
    /// <returns>   A string. </returns>
    public static string BuildMessage( string message, Exception ex )
    {
        return BuildMessage( message, ex, string.Empty, string.Empty, 0 );
    }

    /// <summary>   Builds exception message. </summary>
    /// <param name="message">          The message. </param>
    /// <param name="ex">               The exception. </param>
    /// <param name="memberName">       Name of the member. </param>
    /// <param name="sourceFilePath">   Full pathname of the source file. </param>
    /// <param name="sourceLineNumber"> Source line number. </param>
    /// <returns>   A string. </returns>
    public static string BuildMessage( string message, Exception ex, string memberName, string sourceFilePath, int sourceLineNumber )
    {
        int indent = 4;
        StringBuilder builder = new();
        _ = builder.Append( $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff},{memberName}({sourceLineNumber})" );
        if ( !string.IsNullOrEmpty( message ) ) _ = builder.AppendLine( $"{new string( ' ', indent )}{message}" );
        _ = builder.AppendLine( $"{new string( ' ', indent )}{ex.Message}" );
        if ( !string.IsNullOrEmpty( ex.StackTrace ) ) _ = builder.AppendLine( $"{new string( ' ', indent )}{ex.StackTrace}" );
        if ( !string.IsNullOrEmpty( sourceFilePath ) ) _ = builder.AppendLine( $"{new string( ' ', indent )}{sourceFilePath}" );
        return builder.ToString();
    }


}
