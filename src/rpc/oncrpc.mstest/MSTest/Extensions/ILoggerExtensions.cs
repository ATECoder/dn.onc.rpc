namespace cc.isr.MSTest.Extensions;

/// <summary>
/// <see cref="Microsoft"/>.<see cref="Microsoft.Extensions"/>.<see cref="Microsoft.Extensions.Logging"/>.<see cref="Microsoft.Extensions.Logging.ILogger"/>
/// extension methods.
/// </summary>
/// <remarks>   2024-07-25. </remarks>
public static partial class ILoggerExtensions
{
    #region " Define Logger Message Delegates "

    /// <summary>
    /// Writes a Trace message.
    /// </summary>
    /// <remarks>   David, 2021-07-02. </remarks>
    /// <param name="logger">           The logger. </param>
    /// <param name="message">          The message. </param>
    [LoggerMessage( 0, LogLevel.Trace, "{message}", EventName = "LogTraceMessage" )]
    public static partial void LogTraceMessage( this Microsoft.Extensions.Logging.ILogger logger, string message );

    /// <summary>
    /// Writes a single line Trace message with caller information. Contains the most detailed messages. These
    /// messages may contain sensitive application data. These messages are disabled by default and
    /// should never be enabled in a production environment.
    /// </summary>
    /// <remarks>   David, 2021-07-02. </remarks>
    /// <param name="logger">           The logger. </param>
    /// <param name="message">          The message. </param>
    /// <param name="memberName">       (Optional) Name of the caller member. </param>
    /// <param name="sourcePath">       (Optional) Full pathname of the caller source file. </param>
    /// <param name="sourceLineNumber"> (Optional) Line number in the caller source file. </param>
    [LoggerMessage( 1, LogLevel.Trace, "{message} at [{sourcePath}].{memberName}.Line#{sourceLineNumber}", EventName = "LogTraceSingleLineMessage" )]
    public static partial void LogTraceSingleLineMessage( this Microsoft.Extensions.Logging.ILogger logger,
        string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0 );

    /// <summary>
    /// Writes a multi-line Trace message with caller information. Contains the most detailed messages. These
    /// messages may contain sensitive application data. These messages are disabled by default and
    /// should never be enabled in a production environment.
    /// </summary>
    /// <remarks>   David, 2021-07-02. </remarks>
    /// <param name="logger">           The logger. </param>
    /// <param name="message">          The message. </param>
    /// <param name="memberName">       (Optional) Name of the caller member. </param>
    /// <param name="sourcePath">       (Optional) Full pathname of the caller source file. </param>
    /// <param name="sourceLineNumber"> (Optional) Line number in the caller source file. </param>
    [LoggerMessage( 2, LogLevel.Trace, "{message}\n at [{sourcePath}].{memberName}.Line#{sourceLineNumber}", EventName = "LogTraceMultiLineMessage" )]
    public static partial void LogTraceMultiLineMessage( this Microsoft.Extensions.Logging.ILogger logger,
        string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0 );

    /// <summary>
    /// Writes a Verbose (same as trace) message.
    /// </summary>
    /// <remarks>   David, 2021-07-02. </remarks>
    /// <param name="logger">           The logger. </param>
    /// <param name="message">          The message. </param>
    [LoggerMessage( 3, LogLevel.Trace, "{message}", EventName = "LogVerboseMessage" )]
    public static partial void LogVerboseMessage( this Microsoft.Extensions.Logging.ILogger logger, string message );

    /// <summary>
    /// Writes a single line 'Verbose' (same as Trace for the Microsoft ILogger) message with caller information.
    /// Contains the most detailed messages. These messages may contain sensitive application data.
    /// These messages are disabled by default and should never be enabled in a production
    /// environment.
    /// </summary>
    /// <remarks>   David, 2021-07-02. <para>
    /// This is a left over from using trace event types to manage logging.
    /// </para></remarks>
    /// <param name="logger">           The logger. </param>
    /// <param name="message">          The message. </param>
    /// <param name="memberName">       (Optional) Name of the caller member. </param>
    /// <param name="sourcePath">       (Optional) Full pathname of the caller source file. </param>
    /// <param name="sourceLineNumber"> (Optional) Line number in the caller source file. </param>
    [LoggerMessage( 4, LogLevel.Trace, "[{sourcePath}].{memberName}.Line#{sourceLineNumber}, {message}", EventName = "LogVerboseSingleLineMessage" )]
    public static partial void LogVerboseSingleLineMessage( this Microsoft.Extensions.Logging.ILogger logger,
        string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0 );

    /// <summary>
    /// Writes a multi-Line 'Verbose' (same as Trace for the Microsoft ILogger) message with caller information.
    /// Contains the most detailed messages. These messages may contain sensitive application data.
    /// These messages are disabled by default and should never be enabled in a production
    /// environment.
    /// </summary>
    /// <remarks>   David, 2021-07-02. <para>
    /// This is a left over from using trace event types to manage logging.
    /// </para></remarks>
    /// <param name="logger">           The logger. </param>
    /// <param name="message">          The message. </param>
    /// <param name="memberName">       (Optional) Name of the caller member. </param>
    /// <param name="sourcePath">       (Optional) Full pathname of the caller source file. </param>
    /// <param name="sourceLineNumber"> (Optional) Line number in the caller source file. </param>
    [LoggerMessage( 5, LogLevel.Trace, "[{sourcePath}].{memberName}.Line#{sourceLineNumber}, {message}", EventName = "LogVerboseMultiLineMessage" )]
    public static partial void LogVerboseMultiLineMessage( this Microsoft.Extensions.Logging.ILogger logger,
        string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0 );

    /// <summary>
    /// Writes a Debug message.
    /// </summary>
    /// <remarks>   David, 2021-07-02. </remarks>
    /// <param name="logger">           The logger. </param>
    /// <param name="message">          The message. </param>
    [LoggerMessage( 10, LogLevel.Debug, "{message}", EventName = "LogDebugMessage" )]
    public static partial void LogDebugMessage( this Microsoft.Extensions.Logging.ILogger logger, string message );

    /// <summary>
    /// Writes a single line Debug message with caller information. Contains the most detailed messages. These
    /// messages may contain sensitive application data. These messages are disabled by default and
    /// should never be enabled in a production environment.
    /// </summary>
    /// <remarks>   David, 2021-07-02. </remarks>
    /// <param name="logger">           The logger. </param>
    /// <param name="message">          The message. </param>
    /// <param name="memberName">       (Optional) Name of the caller member. </param>
    /// <param name="sourcePath">       (Optional) Full pathname of the caller source file. </param>
    /// <param name="sourceLineNumber"> (Optional) Line number in the caller source file. </param>
    [LoggerMessage( 11, LogLevel.Debug, "{message} at [{sourcePath}].{memberName}.Line#{sourceLineNumber}", EventName = "LogDebugSingleLineMessage" )]
    public static partial void LogDebugSingleLineMessage( this Microsoft.Extensions.Logging.ILogger logger,
        string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0 );

    /// <summary>
    /// Writes a multi-line Debug message with caller information. Contains the most detailed messages. These
    /// messages may contain sensitive application data. These messages are disabled by default and
    /// should never be enabled in a production environment.
    /// </summary>
    /// <remarks>   David, 2021-07-02. </remarks>
    /// <param name="logger">           The logger. </param>
    /// <param name="message">          The message. </param>
    /// <param name="memberName">       (Optional) Name of the caller member. </param>
    /// <param name="sourcePath">       (Optional) Full pathname of the caller source file. </param>
    /// <param name="sourceLineNumber"> (Optional) Line number in the caller source file. </param>
    [LoggerMessage( 12, LogLevel.Debug, "{message}\n at [{sourcePath}].{memberName}.Line#{sourceLineNumber}", EventName = "LogDebugMultiLineMessage" )]
    public static partial void LogDebugMultiLineMessage( this Microsoft.Extensions.Logging.ILogger logger,
        string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0 );

    /// <summary>
    /// Writes a Information message.
    /// </summary>
    /// <remarks>   David, 2021-07-02. </remarks>
    /// <param name="logger">           The logger. </param>
    /// <param name="message">          The message. </param>
    [LoggerMessage( 20, LogLevel.Information, "{message}", EventName = "LogInformationMessage" )]
    public static partial void LogInformationMessage( this Microsoft.Extensions.Logging.ILogger logger, string message );

    /// <summary>
    /// Writes a single line Information message with caller information. Contains the most detailed messages. These
    /// messages may contain sensitive application data. These messages are disabled by default and
    /// should never be enabled in a production environment.
    /// </summary>
    /// <remarks>   David, 2021-07-02. </remarks>
    /// <param name="logger">           The logger. </param>
    /// <param name="message">          The message. </param>
    /// <param name="memberName">       (Optional) Name of the caller member. </param>
    /// <param name="sourcePath">       (Optional) Full pathname of the caller source file. </param>
    /// <param name="sourceLineNumber"> (Optional) Line number in the caller source file. </param>
    [LoggerMessage( 21, LogLevel.Information, "{message} at [{sourcePath}].{memberName}.Line#{sourceLineNumber}", EventName = "LogInformationSingleLineMessage" )]
    public static partial void LogInformationSingleLineMessage( this Microsoft.Extensions.Logging.ILogger logger,
        string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0 );

    /// <summary>
    /// Writes a multi-line Information message with caller information. Contains the most detailed messages. These
    /// messages may contain sensitive application data. These messages are disabled by default and
    /// should never be enabled in a production environment.
    /// </summary>
    /// <remarks>   David, 2021-07-02. </remarks>
    /// <param name="logger">           The logger. </param>
    /// <param name="message">          The message. </param>
    /// <param name="memberName">       (Optional) Name of the caller member. </param>
    /// <param name="sourcePath">       (Optional) Full pathname of the caller source file. </param>
    /// <param name="sourceLineNumber"> (Optional) Line number in the caller source file. </param>
    [LoggerMessage( 22, LogLevel.Information, "{message}\n at [{sourcePath}].{memberName}.Line#{sourceLineNumber}", EventName = "LogInformationMultiLineMessage" )]
    public static partial void LogInformationMultiLineMessage( this Microsoft.Extensions.Logging.ILogger logger,
        string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0 );

    /// <summary>
    /// Writes a Warning message.
    /// </summary>
    /// <remarks>   David, 2021-07-02. </remarks>
    /// <param name="logger">           The logger. </param>
    /// <param name="message">          The message. </param>
    [LoggerMessage( 30, LogLevel.Warning, "{message}", EventName = "LogWarningMessage" )]
    public static partial void LogWarningMessage( this Microsoft.Extensions.Logging.ILogger logger, string message );

    /// <summary>
    /// Writes a single line Warning message with caller information. Contains the most detailed messages. These
    /// messages may contain sensitive application data. These messages are disabled by default and
    /// should never be enabled in a production environment.
    /// </summary>
    /// <remarks>   David, 2021-07-02. </remarks>
    /// <param name="logger">           The logger. </param>
    /// <param name="message">          The message. </param>
    /// <param name="memberName">       (Optional) Name of the caller member. </param>
    /// <param name="sourcePath">       (Optional) Full pathname of the caller source file. </param>
    /// <param name="sourceLineNumber"> (Optional) Line number in the caller source file. </param>
    [LoggerMessage( 31, LogLevel.Warning, "{message} at [{sourcePath}].{memberName}.Line#{sourceLineNumber}", EventName = "LogWarningSingleLineMessage" )]
    public static partial void LogWarningSingleLineMessage( this Microsoft.Extensions.Logging.ILogger logger,
        string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0 );

    /// <summary>
    /// Writes a multi-line Warning message with caller information. Contains the most detailed messages. These
    /// messages may contain sensitive application data. These messages are disabled by default and
    /// should never be enabled in a production environment.
    /// </summary>
    /// <remarks>   David, 2021-07-02. </remarks>
    /// <param name="logger">           The logger. </param>
    /// <param name="message">          The message. </param>
    /// <param name="memberName">       (Optional) Name of the caller member. </param>
    /// <param name="sourcePath">       (Optional) Full pathname of the caller source file. </param>
    /// <param name="sourceLineNumber"> (Optional) Line number in the caller source file. </param>
    [LoggerMessage( 32, LogLevel.Warning, "{message}\n at [{sourcePath}].{memberName}.Line#{sourceLineNumber}", EventName = "LogWarningMultiLineMessage" )]
    public static partial void LogWarningMultiLineMessage( this Microsoft.Extensions.Logging.ILogger logger,
        string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0 );

    /// <summary>
    /// Writes a Error message.
    /// </summary>
    /// <remarks>   David, 2021-07-02. </remarks>
    /// <param name="logger">           The logger. </param>
    /// <param name="message">          The message. </param>
    [LoggerMessage( 40, LogLevel.Error, "{message}", EventName = "LogErrorMessage" )]
    public static partial void LogErrorMessage( this Microsoft.Extensions.Logging.ILogger logger, string message );

    /// <summary>
    /// Writes a single line Error message with caller information. Contains the most detailed
    /// messages. These messages may contain sensitive application data. These messages are disabled
    /// by default and should never be enabled in a production environment.
    /// </summary>
    /// <remarks>   David, 2021-07-02. </remarks>
    /// <param name="logger">           The logger. </param>
    /// <param name="message">          The message. </param>
    /// <param name="memberName">       (Optional) Name of the caller member. </param>
    /// <param name="sourcePath">       (Optional) Full pathname of the caller source file. </param>
    /// <param name="sourceLineNumber"> (Optional) Line number in the caller source file. </param>
    [LoggerMessage( 41, LogLevel.Error, "{message} at [{sourcePath}].{memberName}.Line#{sourceLineNumber}", EventName = "LogErrorSingleLineMessage" )]
    public static partial void LogErrorSingleLineMessage( this Microsoft.Extensions.Logging.ILogger logger,
        string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0 );

    /// <summary>
    /// Writes a multi-line Error message with caller information. Contains the most detailed
    /// messages. These messages may contain sensitive application data. These messages are disabled
    /// by default and should never be enabled in a production environment.
    /// </summary>
    /// <remarks>   David, 2021-07-02. </remarks>
    /// <param name="logger">           The logger. </param>
    /// <param name="message">          The message. </param>
    /// <param name="memberName">       (Optional) Name of the caller member. </param>
    /// <param name="sourcePath">       (Optional) Full pathname of the caller source file. </param>
    /// <param name="sourceLineNumber"> (Optional) Line number in the caller source file. </param>
    [LoggerMessage( 42, LogLevel.Error, "{message}\n at [{sourcePath}].{memberName}.Line#{sourceLineNumber}", EventName = "LogErrorMultiLineMessage" )]
    public static partial void LogErrorMultiLineMessage( this Microsoft.Extensions.Logging.ILogger logger,
        string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0 );

    /// <summary>   Writes a Exception message. </summary>
    /// <remarks>   David, 2021-07-02. </remarks>
    /// <param name="logger">       The logger. </param>
    /// <param name="message">      The message. </param>
    /// <param name="exception">    The exception. </param>
    [LoggerMessage( 43, LogLevel.Error, "{message}", EventName = "LogExceptionMessage" )]
    public static partial void LogExceptionMessage( this Microsoft.Extensions.Logging.ILogger logger, string message, Exception exception );

    /// <summary>
    /// Writes a single line Exception message with caller information. Contains the most detailed
    /// messages. These messages may contain sensitive application data. These messages are disabled
    /// by default and should never be enabled in a production environment.
    /// </summary>
    /// <remarks>   David, 2021-07-02. </remarks>
    /// <param name="logger">           The logger. </param>
    /// <param name="message">          The message. </param>
    /// <param name="exception">        The exception. </param>
    /// <param name="memberName">       (Optional) Name of the caller member. </param>
    /// <param name="sourcePath">       (Optional) Full pathname of the caller source file. </param>
    /// <param name="sourceLineNumber"> (Optional) Line number in the caller source file. </param>
    [LoggerMessage( 44, LogLevel.Error, "{message} at [{sourcePath}].{memberName}.Line#{sourceLineNumber}", EventName = "LogExceptionSingleLineMessage" )]
    public static partial void LogExceptionSingleLineMessage( this Microsoft.Extensions.Logging.ILogger logger,
        string message, Exception exception, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0 );

    /// <summary>
    /// Writes a multi-line Exception message with caller information. Contains the most detailed
    /// messages. These messages may contain sensitive application data. These messages are disabled
    /// by default and should never be enabled in a production environment.
    /// </summary>
    /// <remarks>   David, 2021-07-02. </remarks>
    /// <param name="logger">           The logger. </param>
    /// <param name="message">          The message. </param>
    /// <param name="exception">        The exception. </param>
    /// <param name="memberName">       (Optional) Name of the caller member. </param>
    /// <param name="sourcePath">       (Optional) Full pathname of the caller source file. </param>
    /// <param name="sourceLineNumber"> (Optional) Line number in the caller source file. </param>
    [LoggerMessage( 45, LogLevel.Error, "{message}\n at [{sourcePath}].{memberName}.Line#{sourceLineNumber}", EventName = "LogExceptionMultiLineMessage" )]
    public static partial void LogExceptionMultiLineMessage( this Microsoft.Extensions.Logging.ILogger logger,
        string message, Exception exception, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0 );

    /// <summary>
    /// Writes a Critical message.
    /// </summary>
    /// <remarks>   David, 2021-07-02. </remarks>
    /// <param name="logger">           The logger. </param>
    /// <param name="message">          The message. </param>
    [LoggerMessage( 50, LogLevel.Critical, "{message}", EventName = "LogCriticalMessage" )]
    public static partial void LogCriticalMessage( this Microsoft.Extensions.Logging.ILogger logger, string message );

    /// <summary>
    /// Writes a single line Critical message with caller information. Contains the most detailed messages. These
    /// messages may contain sensitive application data. These messages are disabled by default and
    /// should never be enabled in a production environment.
    /// </summary>
    /// <remarks>   David, 2021-07-02. </remarks>
    /// <param name="logger">           The logger. </param>
    /// <param name="message">          The message. </param>
    /// <param name="memberName">       (Optional) Name of the caller member. </param>
    /// <param name="sourcePath">       (Optional) Full pathname of the caller source file. </param>
    /// <param name="sourceLineNumber"> (Optional) Line number in the caller source file. </param>
    [LoggerMessage( 51, LogLevel.Critical, "{message} at [{sourcePath}].{memberName}.Line#{sourceLineNumber}", EventName = "LogCriticalSingleLineMessage" )]
    public static partial void LogCriticalSingleLineMessage( this Microsoft.Extensions.Logging.ILogger logger,
        string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0 );

    /// <summary>
    /// Writes a multi-line Critical message with caller information. Contains the most detailed messages. These
    /// messages may contain sensitive application data. These messages are disabled by default and
    /// should never be enabled in a production environment.
    /// </summary>
    /// <remarks>   David, 2021-07-02. </remarks>
    /// <param name="logger">           The logger. </param>
    /// <param name="message">          The message. </param>
    /// <param name="memberName">       (Optional) Name of the caller member. </param>
    /// <param name="sourcePath">       (Optional) Full pathname of the caller source file. </param>
    /// <param name="sourceLineNumber"> (Optional) Line number in the caller source file. </param>
    [LoggerMessage( 52, LogLevel.Critical, "{message}\n at [{sourcePath}].{memberName}.Line#{sourceLineNumber}", EventName = "LogCriticalMultiLineMessage" )]
    public static partial void LogCriticalMultiLineMessage( this Microsoft.Extensions.Logging.ILogger logger,
        string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0 );

    /// <summary>
    /// Writes a Fatal (same as Critical) message.
    /// </summary>
    /// <remarks>   David, 2021-07-02. </remarks>
    /// <param name="logger">           The logger. </param>
    /// <param name="message">          The message. </param>
    [LoggerMessage( 53, LogLevel.Critical, "{message}", EventName = "LogFatalMessage" )]
    public static partial void LogFatalMessage( this Microsoft.Extensions.Logging.ILogger logger, string message );

    /// <summary>
    /// Writes a single line Fatal (same as Critical) message with caller information. Contains the most detailed messages. These
    /// messages may contain sensitive application data. These messages are disabled by default and
    /// should never be enabled in a production environment.
    /// </summary>
    /// <remarks>   David, 2021-07-02. </remarks>
    /// <param name="logger">           The logger. </param>
    /// <param name="message">          The message. </param>
    /// <param name="memberName">       (Optional) Name of the caller member. </param>
    /// <param name="sourcePath">       (Optional) Full pathname of the caller source file. </param>
    /// <param name="sourceLineNumber"> (Optional) Line number in the caller source file. </param>
    [LoggerMessage( 54, LogLevel.Critical, "{message} at [{sourcePath}].{memberName}.Line#{sourceLineNumber}", EventName = "LogFatalSingleLineMessage" )]
    public static partial void LogFatalSingleLineMessage( this Microsoft.Extensions.Logging.ILogger logger,
        string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0 );

    /// <summary>
    /// Writes a multi-line Fatal (same as Critical) message with caller information. Contains the most detailed messages. These
    /// messages may contain sensitive application data. These messages are disabled by default and
    /// should never be enabled in a production environment.
    /// </summary>
    /// <remarks>   David, 2021-07-02. </remarks>
    /// <param name="logger">           The logger. </param>
    /// <param name="message">          The message. </param>
    /// <param name="memberName">       (Optional) Name of the caller member. </param>
    /// <param name="sourcePath">       (Optional) Full pathname of the caller source file. </param>
    /// <param name="sourceLineNumber"> (Optional) Line number in the caller source file. </param>
    [LoggerMessage( 55, LogLevel.Critical, "{message}\n at [{sourcePath}].{memberName}.Line#{sourceLineNumber}", EventName = "LogFatalMultiLineMessage" )]
    public static partial void LogFatalMultiLineMessage( this Microsoft.Extensions.Logging.ILogger logger,
        string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourcePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0 );

    #endregion

    #region " Consolidated writing "

    /// <summary>   An ILogger extension method that writes a log entry. </summary>
    /// <remarks>   David, 2021-03-13. </remarks>
    /// <param name="logger">   The <see cref="Microsoft.Extensions.Logging.ILogger"/> to act on. </param>
    /// <param name="level">    The <see cref="Microsoft.Extensions.Logging.LogLevel"/>. </param>
    /// <param name="message">  The message. </param>
    public static void WriteLogEntry( this Microsoft.Extensions.Logging.ILogger logger, LogLevel level, string message )
    {
        switch ( level )
        {
            case LogLevel.Trace:
                logger.LogTraceMessage( message );
                break;
            case LogLevel.Debug:
                logger.LogDebugMessage( message );
                break;
            case LogLevel.Information:
                logger.LogInformationMessage( message );
                break;
            case LogLevel.Warning:
                logger.LogWarningMessage( message );
                break;
            case LogLevel.Error:
                logger.LogErrorMessage( message );
                break;
            case LogLevel.Critical:
                logger.LogCriticalMessage( message );
                break;
            case LogLevel.None:
                break;
            default:
                logger.LogInformationMessage( message );
                break;
        }
    }

    #endregion
}

