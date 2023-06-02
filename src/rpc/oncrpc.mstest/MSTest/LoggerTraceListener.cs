namespace cc.isr.MSTest;

/// <summary>   A logger trace listener. </summary>
/// <remarks>
/// 2023-06-01.
/// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.tracelistener">trace listener</see>
/// <see href="https://learn.microsoft.com/en-us/dotnet/framework/debug-trace-profile/how-to-create-and-initialize-trace-listeners#to-create-and-use-a-trace-listener-in-code">Create and use a trace listener in code</see>
/// </remarks>
/// <typeparam name="TCategory">    Type of the category. </typeparam>
public class LoggerTraceListener<TCategory> : TraceMessageListener
{

    private readonly ILogger<TCategory> _logger;

    /// <summary>   Constructor. </summary>
    /// <remarks>   2023-06-01. </remarks>
    /// <param name="logger">   The logger. </param>
    public LoggerTraceListener( ILogger<TCategory> logger ) : base()
    {
        this._logger = logger;
    }

    /// <summary>
    /// When overridden in a derived class, writes the specified message to the listener you create
    /// in the derived class.
    /// </summary>
    /// <remarks>   2023-06-01. </remarks>
    /// <param name="message">  A message to write. </param>
    public override void Write( string? message )
    {
        if ( message is not null )
            this._logger?.LogInformation( message );
    }

    /// <summary>
    /// When overridden in a derived class, writes a message to the listener you create in the
    /// derived class, followed by a line terminator.
    /// </summary>
    /// <remarks>   2023-06-01. </remarks>
    /// <param name="message">  A message to write. </param>
    public override void WriteLine( string? message )
    {
        if ( message is not null )
            this._logger?.LogInformation( message );
    }
}
