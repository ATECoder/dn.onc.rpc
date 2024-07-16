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
    /// <returns>   An <see cref="ILogger{TCategory}" /> </returns>
    public static ILogger<TCategory> InitLogger<TCategory>( bool includeScopes = true, bool singleLine = false,
                                                            bool utcTime = true, string timeStampFormat = "yyyyMMdd HH:mm:ss.fff ",
                                                            LogLevel minimumLevel = LogLevel.Information )
    {
        LogWriterExtensions.MemberMessageFormat = singleLine
            ? LogWriterExtensions.SINGLE_LINE_MEMBER_MESSAGE_FORMAT
            : LogWriterExtensions.MULTI_LINE_MEMBER_MESSAGE_FORMAT;
        LogWriterExtensions.MemberExceptionMessageFormat = singleLine
            ? LogWriterExtensions.SINGLE_LINE_MEMBER_EXCEPTION_MESSAGE_FORMAT
            : LogWriterExtensions.MULTI_LINE_MEMBER_EXCEPTION_MESSAGE_FORMAT;
        using ILoggerFactory loggerFactory = LoggerFactory.Create( builder =>
            builder.AddSimpleConsole( options =>
            {
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>" )]
    public const string MULTI_LINE_MEMBER_MESSAGE_FORMAT = "{message}\n  at '{sourceFilePath}' {memberName} line {sourceLineNumber})";

    /// <summary>   (Immutable) the single line member message format. </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>" )]
    public const string SINGLE_LINE_MEMBER_MESSAGE_FORMAT = "{message} at '{sourceFilePath}' {memberName} line {sourceLineNumber})";

    /// <summary>   Gets or sets the member message format. </summary>
    /// <value> The member message format. </value>
    public static string MemberMessageFormat { get; set; } = MULTI_LINE_MEMBER_MESSAGE_FORMAT;

    /// <summary>   (Immutable) the multi line member exception message format. </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>" )]
    public const string MULTI_LINE_MEMBER_EXCEPTION_MESSAGE_FORMAT = "{message}\n  {ex}\n  at '{sourceFilePath}' {memberName} line {sourceLineNumber})";

    /// <summary>   (Immutable) the single line member exception message format. </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>" )]
    public const string SINGLE_LINE_MEMBER_EXCEPTION_MESSAGE_FORMAT = "{message} {ex} at '{sourceFilePath}' {memberName} line {sourceLineNumber})";

    /// <summary>   Gets or sets the member exception message format. </summary>
    /// <value> The member exception message format. </value>
    public static string MemberExceptionMessageFormat { get; set; } = MULTI_LINE_MEMBER_EXCEPTION_MESSAGE_FORMAT;

    /// <summary>   An <see cref="ILogger"/> extension method that logs a verbose. </summary>
    /// <param name="logWriter">    The <see cref="ILogger"/> to act on. </param>
    /// <param name="message">      The message. </param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Performance", "CA1848:Use the LoggerMessage delegates", Justification = "<Pending>" )]
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Usage", "CA2254:Template should be a static expression", Justification = "<Pending>" )]
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Performance", "CA1848:Use the LoggerMessage delegates", Justification = "<Pending>" )]
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Usage", "CA2254:Template should be a static expression", Justification = "<Pending>" )]
    public static void LogMemberVerbose<TCategory>( this ILogger<TCategory> logWriter, string message,
                                                        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
                                                        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
                                                        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0 )
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Performance", "CA1848:Use the LoggerMessage delegates", Justification = "<Pending>" )]
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Usage", "CA2254:Template should be a static expression", Justification = "<Pending>" )]
    public static void LogMemberInfo<TCategory>( this ILogger<TCategory> logWriter, string message,
                                                    [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
                                                    [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
                                                    [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0 )
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Performance", "CA1848:Use the LoggerMessage delegates", Justification = "<Pending>" )]
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Usage", "CA2254:Template should be a static expression", Justification = "<Pending>" )]
    public static void LogMemberWarning<TCategory>( this ILogger<TCategory> logWriter, string message,
                                                    [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
                                                    [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
                                                    [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0 )
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Performance", "CA1848:Use the LoggerMessage delegates", Justification = "<Pending>" )]
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Usage", "CA2254:Template should be a static expression", Justification = "<Pending>" )]
    public static void LogMemberError<TCategory>( this ILogger<TCategory> logWriter, string message, Exception ex,
                                                    [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
                                                    [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
                                                    [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0 )
    {
        if ( logWriter.IsEnabled( LogLevel.Error ) )
            logWriter.Log( LogLevel.Error, MemberExceptionMessageFormat, message, ex, sourceFilePath, memberName, sourceLineNumber );
    }

}

