using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace cc.isr.MSTest;

/// <summary>   An abstract trace message listener with message cache. </summary>
/// <remarks>
/// 2023-06-01.
/// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.tracelistener">trace listener</see>
/// <see href="https://learn.microsoft.com/en-us/dotnet/framework/debug-trace-profile/how-to-create-and-initialize-trace-listeners#to-create-and-use-a-trace-listener-in-code">Create and use a trace listener in code</see>
/// </remarks>
public abstract class TraceMessageListener : TraceListener
{

    /// <summary>   Constructor. </summary>
    /// <remarks>   2023-06-01. </remarks>
    /// <param name="logger">   The logger. </param>
    protected TraceMessageListener()
    {
        this.Clear();
    }

    /// <summary>   Gets or sets the error messages. </summary>
    /// <value> The error messages. </value>
    public Dictionary<TraceEventType, List<string>> Messages { get; private set; }

    /// <summary>   Clears this object to its blank/initial state. </summary>
    /// <remarks>   2023-06-01. </remarks>
    [MemberNotNull( nameof( Messages ) )]
    public void Clear()
    {
        this.Messages = new Dictionary<TraceEventType, List<string>> {
            { TraceEventType.Critical, new List<string>() },
            { TraceEventType.Error, new List<string>() },
            { TraceEventType.Information, new List<string>() },
            { TraceEventType.Verbose, new List<string>() },
            { TraceEventType.Warning, new List<string>() }
        };
    }

    /// <summary>   Check if the listener receive an <see cref="TraceEventType"/> messages. </summary>
    /// <remarks>   2023-06-01. </remarks>
    /// <param name="eventType">    One of the <see cref="T:System.Diagnostics.TraceEventType" />
    ///                             values specifying the type of event that has caused the trace. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    public bool Any( TraceEventType eventType )
    {
        return this.Messages.ContainsKey( eventType ) && this.Messages[eventType].Any();
    }

    /// <summary>   Parses the given event type. </summary>
    /// <remarks>   2023-06-01. </remarks>
    /// <param name="eventType">    One of the <see cref="T:System.Diagnostics.TraceEventType" />
    ///                             values specifying the type of event that has caused the trace. </param>
    /// <returns>   A TraceEventType. </returns>
    public static TraceEventType Parse( TraceEventType eventType )
    {
        return TraceEventType.Critical == (eventType & TraceEventType.Critical)
                ? TraceEventType.Critical
                : TraceEventType.Error == (eventType & TraceEventType.Error)
                ? TraceEventType.Error
                : TraceEventType.Information == (eventType & TraceEventType.Information)
                ? TraceEventType.Information
                : TraceEventType.Verbose == (eventType & TraceEventType.Verbose)
                ? TraceEventType.Verbose
                : TraceEventType.Warning == (eventType & TraceEventType.Warning)
                ? TraceEventType.Warning
                : TraceEventType.Stop;
    }

    /// <summary>   Adds a message to <see cref="Messages"/>. </summary>
    /// <remarks>   2023-06-01. </remarks>
    /// <param name="eventType">    One of the <see cref="T:System.Diagnostics.TraceEventType" />
    ///                             values specifying the type of event that has caused the trace. </param>
    /// <param name="message">      A message to write. </param>
    /// <returns>   An int. </returns>
    public int AddMessage( TraceEventType eventType, string message )
    {
        // remove irrelevant event type info.

        eventType = Parse( eventType );

        if ( eventType <= TraceEventType.Verbose )
        {
            this.Messages[eventType].Add( message );
            return this.Messages[eventType].Count;
        }
        else
            return 0;
    }

    /// <summary>  
    /// Writes trace information, a message, and event information to the listener specific output.
    /// </summary>
    /// <remarks>   2023-06-01. </remarks>
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
            _ = this.AddMessage( eventType, message );
        base.TraceEvent( eventCache, source, eventType, id, message );
    }

}
