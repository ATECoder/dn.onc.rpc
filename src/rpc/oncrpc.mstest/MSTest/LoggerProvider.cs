using System.Runtime.CompilerServices;

namespace cc.isr.MSTest;

/// <summary>   An application logger provider. </summary>
/// <remarks>   2023-05-09. </remarks>
internal static class LoggerProvider
{

    /// <summary>   Initializes the logger. </summary>
    /// <remarks>   2023-04-24. </remarks>
    /// <typeparam name="TCategory">    Type of the category. </typeparam>
    /// <param name="includeScopes">    (Optional) True to include, false to exclude the scopes. </param>
    /// <param name="singleLine">       (Optional) True to log a single line. </param>
    /// <param name="utcTime">          (Optional) True to use UTC time. </param>
    /// <param name="timeStampFormat">  (Optional) The time stamp format. </param>
    /// <param name="minimumLevel">     (Optional) The minimum level. </param>
    /// <returns>   An ILogger&lt;TCategory&gt; </returns>
    public static ILogger<TCategory> InitLogger<TCategory>( bool includeScopes = true, bool singleLine = false,
                                                            bool utcTime = true, string timeStampFormat = "yyyyMMdd HH:mm:ss.fff ",
                                                            LogLevel minimumLevel = LogLevel.Information )
    {
        LogWriterExtensions.MemberMessageFormat = singleLine
            ? LogWriterExtensions.SingleLineMemberMessageFormat
            : LogWriterExtensions.MultiLineMemberMessageFormat;
        LogWriterExtensions.MemberExceptionMessageFormat = singleLine
            ? LogWriterExtensions.SingleLineMemberExceptionMessageFormat
            : LogWriterExtensions.MultiLineMemberExceptionMessageFormat;
        using ILoggerFactory loggerFactory = LoggerFactory.Create( builder =>
            builder.AddSimpleConsole( options => {
                options.IncludeScopes = includeScopes;
                options.SingleLine = singleLine;
                options.UseUtcTimestamp = utcTime;
                options.TimestampFormat = timeStampFormat;
            } ).SetMinimumLevel( minimumLevel )
        );
        return loggerFactory.CreateLogger<TCategory>();
    }

}

/// <summary>   A logging extensions. </summary>
public static class LogWriterExtensions
{

    /// <summary>   (Immutable) the multi line member message format. </summary>
    public const string MultiLineMemberMessageFormat = "{message}\n  at '{sourceFilePath}' {memberName} line {sourceLineNumber})";

    /// <summary>   (Immutable) the single line member message format. </summary>
    public const string SingleLineMemberMessageFormat = "{message} at '{sourceFilePath}' {memberName} line {sourceLineNumber})";

    /// <summary>   Gets or sets the member message format. </summary>
    /// <value> The member message format. </value>
    public static string MemberMessageFormat { get; set; } = MultiLineMemberMessageFormat;

    /// <summary>   (Immutable) the multi line member exception message format. </summary>
    public const string MultiLineMemberExceptionMessageFormat = "{message}\n  {ex}\n  at '{sourceFilePath}' {memberName} line {sourceLineNumber})";

    /// <summary>   (Immutable) the single line member exception message format. </summary>
    public const string SingleLineMemberExceptionMessageFormat = "{message} {ex} at '{sourceFilePath}' {memberName} line {sourceLineNumber})";

    /// <summary>   Gets or sets the member exception message format. </summary>
    /// <value> The member exception message format. </value>
    public static string MemberExceptionMessageFormat { get; set; } = MultiLineMemberExceptionMessageFormat;

    /// <summary>   An <see cref="ILogger"/> extension method that logs a verbose. </summary>
    /// <param name="logWriter">    The <see cref="ILogger"/> to act on. </param>
    /// <param name="message">      The message. </param>
    public static void LogVerbose<TCategory>( this ILogger<TCategory> logWriter, string message )
    {
        if ( logWriter.IsEnabled( LogLevel.Trace ) )
            logWriter.LogTrace( message );
    }

    /// <summary>   An <see cref="ILogger"/> extension method that logs member verbose. </summary>
    /// <remarks>   2023-03-23. </remarks>
    /// <param name="logWriter">        The <see cref="ILogger"/> to act on. </param>
    /// <param name="message">          The message. </param>
    /// <param name="memberName">       (Optional) Name of the member. </param>
    /// <param name="sourceFilePath">   (Optional) Full pathname of the source file. </param>
    /// <param name="sourceLineNumber"> (Optional) Source line number. </param>
    public static void LogMemberVerbose<TCategory>( this ILogger<TCategory> logWriter, string message,
                                                        [CallerMemberName] string memberName = "",
                                                        [CallerFilePath] string sourceFilePath = "",
                                                        [CallerLineNumber] int sourceLineNumber = 0 )
    {
        if ( logWriter.IsEnabled( LogLevel.Trace ) )
            logWriter.Log( LogLevel.Trace, MemberMessageFormat, message, sourceFilePath, memberName, sourceLineNumber );
    }

    /// <summary>   An <see cref="ILogger"/> extension method that logs member information. </summary>
    /// <remarks>   2023-03-23. </remarks>
    /// <param name="logWriter">        The <see cref="ILogger"/> to act on. </param>
    /// <param name="message">          The message. </param>
    /// <param name="memberName">       (Optional) Name of the member. </param>
    /// <param name="sourceFilePath">   (Optional) Full pathname of the source file. </param>
    /// <param name="sourceLineNumber"> (Optional) Source line number. </param>
    public static void LogMemberInfo<TCategory>( this ILogger<TCategory> logWriter, string message,
                                                    [CallerMemberName] string memberName = "",
                                                    [CallerFilePath] string sourceFilePath = "",
                                                    [CallerLineNumber] int sourceLineNumber = 0 )
    {
        if ( logWriter.IsEnabled( LogLevel.Information ) )
            logWriter.Log( LogLevel.Information, MemberMessageFormat, message, sourceFilePath, memberName, sourceLineNumber );
    }

    /// <summary>   An <see cref="ILogger"/> extension method that logs member warning. </summary>
    /// <remarks>   2023-03-23. </remarks>
    /// <param name="logWriter">        The <see cref="ILogger"/> to act on. </param>
    /// <param name="message">          The message. </param>
    /// <param name="memberName">       (Optional) Name of the member. </param>
    /// <param name="sourceFilePath">   (Optional) Full pathname of the source file. </param>
    /// <param name="sourceLineNumber"> (Optional) Source line number. </param>
    public static void LogMemberWarning<TCategory>( this ILogger<TCategory> logWriter, string message,
                                                    [CallerMemberName] string memberName = "",
                                                    [CallerFilePath] string sourceFilePath = "",
                                                    [CallerLineNumber] int sourceLineNumber = 0 )
    {
        if ( logWriter.IsEnabled( LogLevel.Warning ) )
            logWriter.Log( LogLevel.Warning, MemberMessageFormat, message, sourceFilePath, memberName, sourceLineNumber );
    }

    /// <summary>   Console write exception. </summary>
    /// <remarks>   2023-03-23. </remarks>
    /// <param name="logWriter">        The <see cref="ILogger"/> to act on. </param>
    /// <param name="message">          The message. </param>
    /// <param name="ex">               The exception. </param>
    /// <param name="memberName">       (Optional) Name of the member. </param>
    /// <param name="sourceFilePath">   (Optional) Full pathname of the source file. </param>
    /// <param name="sourceLineNumber"> (Optional) Source line number. </param>
    public static void LogMemberError<TCategory>( this ILogger<TCategory> logWriter, string message, System.Exception ex,
                                                    [CallerMemberName] string memberName = "",
                                                    [CallerFilePath] string sourceFilePath = "",
                                                    [CallerLineNumber] int sourceLineNumber = 0 )
    {
        if ( logWriter.IsEnabled( LogLevel.Error ) )
            logWriter.Log( LogLevel.Error, MemberExceptionMessageFormat, message, ex, sourceFilePath, memberName, sourceLineNumber );
    }

}

