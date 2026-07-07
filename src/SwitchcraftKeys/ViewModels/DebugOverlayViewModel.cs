using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace SwitchcraftKeys.ViewModels;

/// <summary>
/// ViewModel for the always-on-top debug overlay window.
/// Shows live device detection events, active device ID, and current layout KLID.
/// Used during Phase 1 manual testing before the full dashboard UI exists.
/// </summary>
public partial class DebugOverlayViewModel : ObservableObject
{
    private readonly ILogger<DebugOverlayViewModel> _logger;
    private const int MaxLogEntries = 100;

    // -------------------------------------------------------------------------
    // Observable state
    // -------------------------------------------------------------------------

    /// <summary>Currently active device ID (e.g. "VID_046D&PID_C31C" or "BUILTIN").</summary>
    [ObservableProperty]
    private string _activeDeviceId = "(none)";

    /// <summary>Currently active layout KLID (e.g. "00000409").</summary>
    [ObservableProperty]
    private string _activeLayoutKlid = "(none)";

    /// <summary>Display name of the current layout (e.g. "US").</summary>
    [ObservableProperty]
    private string _activeLayoutName = "(none)";

    /// <summary>Number of WM_INPUT messages received this session.</summary>
    [ObservableProperty]
    private int _inputEventCount;

    /// <summary>Status line shown at the top of the overlay.</summary>
    [ObservableProperty]
    private string _statusText = "Waiting for input...";

    /// <summary>Scrolling event log — newest entries at the top.</summary>
    public ObservableCollection<string> EventLog { get; } = [];

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public DebugOverlayViewModel(ILogger<DebugOverlayViewModel> logger)
    {
        _logger = logger;
        _logger.LogInformation("View activated view={View}", nameof(DebugOverlayViewModel));
    }

    // -------------------------------------------------------------------------
    // Public update methods (called by DeviceService event handlers)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Records a raw WM_INPUT event. Call from the UI thread.
    /// </summary>
    public void RecordInputEvent(string deviceId, string rawPath)
    {
        InputEventCount++;
        ActiveDeviceId = deviceId;
        StatusText = $"Input #{InputEventCount} from {deviceId}";

        var entry = $"[{DateTime.Now:HH:mm:ss.fff}] INPUT  device={deviceId}";
        AddLogEntry(entry);

        _logger.LogTrace("WM_INPUT recorded deviceId={DeviceId} rawPath={RawPath}", deviceId, rawPath);
    }

    /// <summary>
    /// Records a layout switch event. Call from the UI thread.
    /// </summary>
    public void RecordLayoutSwitch(string klid, string displayName)
    {
        ActiveLayoutKlid = klid;
        ActiveLayoutName = displayName;
        StatusText = $"Layout → {displayName} ({klid})";

        var entry = $"[{DateTime.Now:HH:mm:ss.fff}] LAYOUT klid={klid} name={displayName}";
        AddLogEntry(entry);

        _logger.LogInformation("Layout switch recorded klid={Klid} name={Name}", klid, displayName);
    }

    /// <summary>
    /// Appends a generic message to the event log.
    /// </summary>
    public void AppendLog(string message)
    {
        AddLogEntry($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
    }

    // -------------------------------------------------------------------------
    // Commands
    // -------------------------------------------------------------------------

    [RelayCommand]
    private void ClearLog()
    {
        _logger.LogTrace("Command dispatched command={Command}", nameof(ClearLogCommand));
        EventLog.Clear();
        InputEventCount = 0;
        StatusText = "Log cleared.";
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private void AddLogEntry(string entry)
    {
        EventLog.Insert(0, entry);

        // Cap log size to avoid unbounded growth
        while (EventLog.Count > MaxLogEntries)
            EventLog.RemoveAt(EventLog.Count - 1);
    }
}
