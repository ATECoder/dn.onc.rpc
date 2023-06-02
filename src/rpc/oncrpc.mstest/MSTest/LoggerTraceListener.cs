using System.Diagnostics;

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
    /// Writes trace information, a message, and event information to the listener specific output.
    /// </summary>
    /// <remarks>   2023-06-02. </remarks>
    /// <param name="eventCache">   A <see cref="T:System.Diagnostics.TraceEventCache" /> object that
    ///                             contains the current process ID, thread ID, and stack trace
    ///                             information. </param>
    /// <param name="source">       A name used to identify the output, typically the name of the
    ///                             application that generated the trace event. </param>
    /// <param name="eventType">    One of the <see cref="T:System.Diagnostics.TraceEventType" />
    ///                             values specifying the type of event that has caused the trace. </param>
    /// <param name="id">           A numeric identifier for the event. </param>
    /// <param name="message">      A message to write. </param>
    public override void TraceEvent( TraceEventCache? eventCache, string source, TraceEventType eventType, int id, string? message )
    {
        if ( !string.IsNullOrEmpty( message ) )
        {
            if ( TraceEventType.Critical == ( TraceEventType.Critical & eventType ) )
                this._logger?.LogCritical( message );
            else if ( TraceEventType.Error == (TraceEventType.Error & eventType) )
                this._logger?.LogError( message );
            else if ( TraceEventType.Information == (TraceEventType.Information & eventType) )
                this._logger?.LogInformation( message );
            else if ( TraceEventType.Verbose == (TraceEventType.Verbose & eventType) )
                this._logger?.LogVerbose( message );
            else if ( TraceEventType.Warning == (TraceEventType.Warning & eventType) )
                this._logger?.LogWarning( message );
            else
                this._logger?.LogInformation( message );
        }
        base.TraceEvent( eventCache, source, eventType, id, message );
    }

    /// <summary>
    /// When overridden in a derived class, writes the specified message to the listener you create
    /// in the derived class.
    /// </summary>
    /// <remarks>   2023-06-01. </remarks>
    /// <param name="message">  A message to write. </param>
    public override void Write( string? message )
    {
        // if ( message is not null ) this._logger?.LogInformation( message );
    }

    /// <summary>
    /// When overridden in a derived class, writes a message to the listener you create in the
    /// derived class, followed by a line terminator.
    /// </summary>
    /// <remarks>   2023-06-01. </remarks>
    /// <param name="message">  A message to write. </param>
    public override void WriteLine( string? message )
    {
        // if ( message is not null ) this._logger?.LogInformation( message );
    }
}
