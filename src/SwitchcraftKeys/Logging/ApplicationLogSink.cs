using Serilog.Core;
using Serilog.Events;

namespace SwitchcraftKeys.Logging;

public sealed class ApplicationLogSink : ILogEventSink
{
    private readonly ApplicationLogService _logService;

    public ApplicationLogSink(ApplicationLogService logService)
    {
        _logService = logService;
    }

    public void Emit(LogEvent logEvent)
    {
        var sourceContext = logEvent.Properties.TryGetValue("SourceContext", out var source)
            ? source.ToString().Trim('"')
            : "Application";

        _logService.Add(new LogEntry(
            logEvent.Timestamp,
            logEvent.Level,
            sourceContext,
            logEvent.RenderMessage(),
            logEvent.Exception?.ToString()));
    }
}
