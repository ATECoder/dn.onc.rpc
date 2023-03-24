using System.Runtime.CompilerServices;

namespace cc.isr.ONC.RPC.Logging;

public interface ILogWriter
{

    public abstract void Write( LogWriterLevel level, string message );

    public abstract bool IsEnabled( LogWriterLevel level );

}

/// <summary>   Values that represent log writer levels. </summary>
/// <remarks>   2023-03-23. </remarks>
public enum LogWriterLevel
{ None, Error, Warning, Information, Verbose }

/// <summary>   A log writer extensions. </summary>
/// <remarks>   2023-03-23. </remarks>
public static class LogWriterExtensions
{

    /// <summary>   An <see cref="ILogWriter"/> extension method that logs a verbose. </summary>
    /// <remarks>   2023-03-23. </remarks>
    /// <param name="logWriter">    The <see cref="ILogWriter"/> to act on. </param>
    /// <param name="message">      The message. </param>
    public static void LogVerbose( this ILogWriter logWriter, string message )
    {
        if ( logWriter.IsEnabled( LogWriterLevel.Verbose ) )
            logWriter.Write( LogWriterLevel.Verbose, BuildMessage( message ) );
    }

    /// <summary>   An <see cref="ILogWriter"/> extension method that logs member verbose. </summary>
    /// <remarks>   2023-03-23. </remarks>
    /// <param name="logWriter">        The <see cref="ILogWriter"/> to act on. </param>
    /// <param name="message">          The message. </param>
    /// <param name="memberName">       (Optional) Name of the member. </param>
    /// <param name="sourceFilePath">   (Optional) Full pathname of the source file. </param>
    /// <param name="sourceLineNumber"> (Optional) Source line number. </param>
    public static void LogMemberVerbose( this ILogWriter logWriter, string message, [CallerMemberName] string memberName = "",
                                                                           [CallerFilePath] string sourceFilePath = "",
                                                                           [CallerLineNumber] int sourceLineNumber = 0 )
    {
        if ( logWriter.IsEnabled( LogWriterLevel.Verbose ) )
            logWriter.Write( LogWriterLevel.Error, BuildMessage( message, memberName, sourceFilePath, sourceLineNumber ) );
    }

    /// <summary>   An <see cref="ILogWriter"/> extension method that logs an information. </summary>
    /// <remarks>   2023-03-23. </remarks>
    /// <param name="logWriter">    The <see cref="ILogWriter"/> to act on. </param>
    /// <param name="message">      The message. </param>
    public static void LogInformation( this ILogWriter logWriter, string message )
    {
        if ( logWriter.IsEnabled( LogWriterLevel.Information ) )
            logWriter.Write( LogWriterLevel.Information, BuildMessage( message ) );
    }

    /// <summary>   An <see cref="ILogWriter"/> extension method that logs member information. </summary>
    /// <remarks>   2023-03-23. </remarks>
    /// <param name="logWriter">        The <see cref="ILogWriter"/> to act on. </param>
    /// <param name="message">          The message. </param>
    /// <param name="memberName">       (Optional) Name of the member. </param>
    /// <param name="sourceFilePath">   (Optional) Full pathname of the source file. </param>
    /// <param name="sourceLineNumber"> (Optional) Source line number. </param>
    public static void LogMemberInfo( this ILogWriter logWriter, string message, [CallerMemberName] string memberName = "",
                                                                           [CallerFilePath] string sourceFilePath = "",
                                                                           [CallerLineNumber] int sourceLineNumber = 0 )
    {
        if ( logWriter.IsEnabled( LogWriterLevel.Information ) )
            logWriter.Write( LogWriterLevel.Error, BuildMessage( message, memberName, sourceFilePath, sourceLineNumber ) );
    }

    /// <summary>   An <see cref="ILogWriter"/> extension method that logs a warning. </summary>
    /// <remarks>   2023-03-23. </remarks>
    /// <param name="logWriter">    The <see cref="ILogWriter"/> to act on. </param>
    /// <param name="message">      The message. </param>
    public static void LogWarning( this ILogWriter logWriter, string message )
    {
        if ( logWriter.IsEnabled( LogWriterLevel.Warning ) )
            logWriter.Write( LogWriterLevel.Warning, BuildMessage( message ) );
    }

    /// <summary>   An <see cref="ILogWriter"/> extension method that logs member warning. </summary>
    /// <remarks>   2023-03-23. </remarks>
    /// <param name="logWriter">        The <see cref="ILogWriter"/> to act on. </param>
    /// <param name="message">          The message. </param>
    /// <param name="memberName">       (Optional) Name of the member. </param>
    /// <param name="sourceFilePath">   (Optional) Full pathname of the source file. </param>
    /// <param name="sourceLineNumber"> (Optional) Source line number. </param>
    public static void LogMemberWarning( this ILogWriter logWriter, string message, [CallerMemberName] string memberName = "",
                                                                           [CallerFilePath] string sourceFilePath = "",
                                                                           [CallerLineNumber] int sourceLineNumber = 0 )
    {
        if ( logWriter.IsEnabled( LogWriterLevel.Warning ) )
            logWriter.Write( LogWriterLevel.Error, BuildMessage( message, memberName, sourceFilePath, sourceLineNumber ) );
    }

    /// <summary>   An <see cref="ILogWriter"/> extension method that writes an error. </summary>
    /// <remarks>   2023-03-23. </remarks>
    /// <param name="logWriter">    The <see cref="ILogWriter"/> to act on. </param>
    /// <param name="message">      The message. </param>
    /// <param name="ex">           The exception. </param>
    public static void LogError( this ILogWriter logWriter, string message, Exception ex )
    {
        if ( logWriter.IsEnabled( LogWriterLevel.Error ) )
            logWriter.Write( LogWriterLevel.Error, BuildMessage( message, ex ) );
    }

    /// <summary>   Console write exception. </summary>
    /// <remarks>   2023-03-23. </remarks>
    /// <param name="logWriter">        The <see cref="ILogWriter"/> to act on. </param>
    /// <param name="message">          The message. </param>
    /// <param name="ex">               The exception. </param>
    /// <param name="memberName">       (Optional) Name of the member. </param>
    /// <param name="sourceFilePath">   (Optional) Full pathname of the source file. </param>
    /// <param name="sourceLineNumber"> (Optional) Source line number. </param>
    public static void LogMemberError( this ILogWriter logWriter, string message, Exception ex, [CallerMemberName] string memberName = "",
                                                                                            [CallerFilePath] string sourceFilePath = "",
                                                                                            [CallerLineNumber] int sourceLineNumber = 0 )
    {
        if ( logWriter.IsEnabled( LogWriterLevel.Error ) )
            logWriter.Write( LogWriterLevel.Error, BuildMessage( message, ex, memberName, sourceFilePath, sourceLineNumber ) );
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
