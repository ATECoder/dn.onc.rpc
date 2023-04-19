namespace cc.isr.ONC.RPC.Logging;

public class ConsoleLogWriter : ILogWriter
{

    /// <summary>   Constructor. </summary>
    /// <param name="minimumLogLevel">  The minimum log level. </param>
    public ConsoleLogWriter( LogWriterLevel minimumLogLevel )
    {
        this.MinimumLogLevel = minimumLogLevel;
    }

    /// <summary>   Gets or sets the minimum log level. </summary>
    /// <value> The minimum log level. </value>
    private LogWriterLevel MinimumLogLevel { get; set; }

    /// <summary>   Query if 'level' is enabled. </summary>
    /// <remarks> 
    /// <see href="https://learn.Microsoft.com/en-us/dotnet/core/extensions/custom-logging-provider"/>
    /// public bool IsEnabled(LogLevel logLevel) = _getCurrentConfig().LogLevelToColorMap.ContainsKey(logLevel);
    /// </remarks>
    /// <param name="level">    The level. </param>
    /// 
    /// <returns>   True if enabled, false if not. </returns>
    public bool IsEnabled( LogWriterLevel level )
    {
        return level <= this.MinimumLogLevel;
    }

    /// <summary>   Writes. </summary>
    /// <param name="level">    The level. </param>
    /// <param name="message">  The message. </param>
    public void Write( LogWriterLevel level, string message )
    {
        Console.WriteLine( message );
    }

}

/// <summary>   A logger. </summary>
public static class Logger
{
    /// <summary>   Gets or sets the log writer. </summary>
    /// <value> The writer. </value>
    public static ILogWriter Writer { get; set; } = new ConsoleLogWriter( LogWriterLevel.Verbose );
}



