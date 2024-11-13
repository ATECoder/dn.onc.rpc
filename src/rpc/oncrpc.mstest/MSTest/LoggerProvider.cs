using Microsoft.Extensions.Logging;

namespace cc.isr.Std.MSTest;

/// <summary>   Provides (Creates) an <see cref="Microsoft.Extensions.Logging.ILogger"/>. </summary>
/// <remarks>   2023-05-09. </remarks>
public static class LoggerProvider
{
    /// <summary>
    /// Creates an instance of the <see cref="Microsoft.Extensions.Logging.ILogger"/>
    /// Adds a configures a console log formatter named 'simple' to the <see cref="Microsoft.Extensions.Logging.LoggerFactory"/>.
    /// </summary>
    /// <remarks>   2023-04-24. </remarks>
    /// <typeparam name="TCategory">    Type of the category. </typeparam>
    /// <param name="includeScopes">    (Optional) True to include, false to exclude the scopes. </param>
    /// <param name="singleLine">       (Optional) True to log a single line. </param>
    /// <param name="utcTime">          (Optional) True to use UTC time. </param>
    /// <param name="timeStampFormat">  (Optional) The time stamp format. </param>
    /// <param name="minimumLevel">     (Optional) The minimum level. </param>
    /// <returns>   An <see cref="Microsoft.Extensions.Logging.ILogger{TCategory}" /> </returns>
    public static ILogger<TCategory> CreateLogger<TCategory>( bool includeScopes = true, bool singleLine = false,
        bool utcTime = true, string timeStampFormat = "yyyyMMdd HH:mm:ss.fff ",
        LogLevel minimumLevel = LogLevel.Information )
    {
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
