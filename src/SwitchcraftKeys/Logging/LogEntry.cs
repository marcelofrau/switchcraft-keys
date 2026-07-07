using Serilog.Events;

namespace SwitchcraftKeys.Logging;

public sealed class LogEntry
{
    public LogEntry(DateTimeOffset timestamp, LogEventLevel level, string sourceContext, string message, string? exception)
    {
        Timestamp = timestamp;
        Level = level;
        SourceContext = sourceContext;
        Message = message;
        Exception = exception;
    }

    public DateTimeOffset Timestamp { get; }

    public LogEventLevel Level { get; }

    public string SourceContext { get; }

    public string Message { get; }

    public string? Exception { get; }

    public string LevelText => Level switch
    {
        LogEventLevel.Verbose => "VRB",
        LogEventLevel.Debug => "DBG",
        LogEventLevel.Information => "INF",
        LogEventLevel.Warning => "WRN",
        LogEventLevel.Error => "ERR",
        LogEventLevel.Fatal => "FTL",
        _ => Level.ToString().ToUpperInvariant(),
    };

    public string SourceName
    {
        get
        {
            var lastDot = SourceContext.LastIndexOf('.');
            return lastDot >= 0 && lastDot < SourceContext.Length - 1
                ? SourceContext[(lastDot + 1)..]
                : SourceContext;
        }
    }

    public string Foreground => Level switch
    {
        LogEventLevel.Verbose => "#8AE234",
        LogEventLevel.Debug => "#729FCF",
        LogEventLevel.Information => "#EEEEEC",
        LogEventLevel.Warning => "#FCE94F",
        LogEventLevel.Error => "#EF2929",
        LogEventLevel.Fatal => "#F57900",
        _ => "#EEEEEC",
    };

    public string Text => $"[{LevelText}] {SourceName}: {Message}" +
        (string.IsNullOrWhiteSpace(Exception) ? string.Empty : Environment.NewLine + Exception);
}
