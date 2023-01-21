namespace cc.isr.ONC.RPC.Logging;

public class ConsoleLogger : ILogger
{

    /// <summary>   Constructor. </summary>
    /// <param name="minimumLogLevel">  The minimum log level. </param>
    public ConsoleLogger( LogLevel minimumLogLevel )
    {
        this.MinimumLogLevel = minimumLogLevel;
    }

    /// <summary>   Gets or sets the minimum log level. </summary>
    /// <value> The minimum log level. </value>
    private LogLevel MinimumLogLevel { get; set; }

    /// <summary>   Query if 'level' is enabled. </summary>
    /// <remarks> 
    /// <see href="https://learn.microsoft.com/en-us/dotnet/core/extensions/custom-logging-provider"/>
    /// public bool IsEnabled(LogLevel logLevel) = _getCurrentConfig().LogLevelToColorMap.ContainsKey(logLevel);
    /// </remarks>
    /// <param name="level">    The level. </param>
    /// 
    /// <returns>   True if enabled, false if not. </returns>
    public bool IsEnabled( LogLevel level )
    {
        return level <= this.MinimumLogLevel;
    }

    /// <summary>   Writes. </summary>
    /// <param name="level">    The level. </param>
    /// <param name="message">  The message. </param>
    public void Write( LogLevel level, string message )
    {
        Console.WriteLine( message );
    }

}

/// <summary>   A logger. </summary>
public static class Logger
{
    /// <summary>   Gets or sets the log writer. </summary>
    /// <value> The writer. </value>
    public static ILogger Writer { get; set; } = new ConsoleLogger( LogLevel.Verbose );
}



