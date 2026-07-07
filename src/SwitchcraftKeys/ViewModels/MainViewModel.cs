using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SwitchcraftKeys.Logging;
using Microsoft.Extensions.Logging;
using SwitchcraftKeys.Models;
using SwitchcraftKeys.Services.Interfaces;

namespace SwitchcraftKeys.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private const string AllLanguages = "All languages";

    private readonly ILogger<MainViewModel> _logger;
    private readonly IConfigService _configService;
    private readonly IDeviceService _deviceService;
    private readonly ILayoutService _layoutService;
    private readonly IApplicationControlService _applicationControlService;
    private readonly ApplicationLogService _applicationLogService;
    private readonly Action<LogLevel>? _updateMinimumLogLevel;
    private bool _isLoadingSelectedKeyboardFields;

    [ObservableProperty]
    private string _title = "SwitchcraftKeys";

    [ObservableProperty]
    private string _currentDevice = "No device active";

    [ObservableProperty]
    private string _currentDeviceDisplayName = "No device active";

    [ObservableProperty]
    private string _currentLayout = "No layout active";

    [ObservableProperty]
    private KeyboardDeviceViewModel? _selectedKeyboard;

    [ObservableProperty]
    private string _selectedKeyboardAlias = string.Empty;

    [ObservableProperty]
    private string _selectedKeyboardLayoutKlid = string.Empty;

    [ObservableProperty]
    private string _selectedKeyboardLanguage = AllLanguages;

    [ObservableProperty]
    private LayoutInfo? _selectedKeyboardLayout;

    [ObservableProperty]
    private string _selectedKeyboardStatusMessage = string.Empty;

    [ObservableProperty]
    private bool _hasSelectedKeyboardStatusMessage;

    [ObservableProperty]
    private string _selectedSection = "Dashboard";

    [ObservableProperty]
    private string _selectedLogLevel = "Trace";

    [ObservableProperty]
    private bool _isLogScrollLocked;

    [ObservableProperty]
    private string _windowsInputMethodStatus = "Not checked";

    public ObservableCollection<KeyboardDeviceViewModel> Keyboards { get; } = [];

    public ObservableCollection<string> AvailableLanguages { get; } = [];

    public ObservableCollection<LayoutInfo> AvailableLayouts { get; } = [];

    public ObservableCollection<LayoutInfo> FilteredLayouts { get; } = [];

    public ObservableCollection<LogEntry> LogEntries => _applicationLogService.Entries;

    public IReadOnlyList<string> LogLevels { get; } = ["Trace", "Debug", "Information", "Warning", "Error", "Critical"];

    public bool IsDashboardSelected => SelectedSection == "Dashboard";

    public bool IsLogsSelected => SelectedSection == "Logs";

    public bool IsSettingsSelected => SelectedSection == "Settings";

    public bool IsAboutSelected => SelectedSection == "About";

    public bool IsLogScrollUnlocked => !IsLogScrollLocked;

    public bool HasSelectedKeyboard => SelectedKeyboard is not null;

    public bool HasNoSelectedKeyboard => SelectedKeyboard is null;

    public bool CurrentDeviceIsBuiltin => string.Equals(CurrentDevice, "BUILTIN", StringComparison.OrdinalIgnoreCase);

    public bool CurrentDeviceIsBluetooth => CurrentDevice.Contains("PID_B380", StringComparison.OrdinalIgnoreCase);

    public bool CurrentDeviceIsUsb => CurrentDevice.StartsWith("VID_", StringComparison.OrdinalIgnoreCase) && !CurrentDeviceIsBluetooth;

    public event EventHandler<string>? CopyAllLogsRequested;

    public event EventHandler? LogScrollLockChanged;

    public event EventHandler<AppToastEventArgs>? AppToastRequested;

    public MainViewModel(
        ILogger<MainViewModel> logger,
        IConfigService configService,
        IDeviceService deviceService,
        ILayoutService layoutService,
        IApplicationControlService applicationControlService,
        ApplicationLogService applicationLogService,
        Action<LogLevel>? updateMinimumLogLevel)
    {
        _logger = logger;
        _configService = configService;
        _deviceService = deviceService;
        _layoutService = layoutService;
        _applicationControlService = applicationControlService;
        _applicationLogService = applicationLogService;
        _updateMinimumLogLevel = updateMinimumLogLevel;

        _deviceService.DeviceActivated += OnDeviceActivated;
        SelectedLogLevel = _configService.Load().Logging.MinimumLevel;
        RefreshWindowsInputMethodStatus();
        LoadAvailableLayouts();
        LoadConfiguredDevices();

        _logger.LogDebug("MainViewModel initialized");
    }

    private async void OnDeviceActivated(object? sender, DeviceActivatedEventArgs e)
    {
        _logger.LogInformation("Device activation handled deviceId={DeviceId}", e.Device.DeviceId);
        CurrentDevice = e.Device.DeviceId;

        _configService.EnsureDeviceExists(e.Device.DeviceId);
        var keyboard = EnsureKeyboardViewModel(e.Device);
        CurrentDeviceDisplayName = keyboard.DisplayName;

        var mappedKlid = _configService.GetMappedLayoutKlid(e.Device.DeviceId);
        _logger.LogDebug("Mapped layout lookup deviceId={DeviceId} klid={Klid}", e.Device.DeviceId, mappedKlid);

        if (string.IsNullOrWhiteSpace(mappedKlid))
        {
            _logger.LogInformation("No mapped layout for deviceId={DeviceId}", e.Device.DeviceId);
            AppToastRequested?.Invoke(this, new AppToastEventArgs("Active keyboard", CurrentDeviceDisplayName, "No layout assigned", AppToastKind.Warning));
            return;
        }

        var switched = await _layoutService.SwitchLayoutAsync(mappedKlid);
        if (switched)
        {
            CurrentLayout = mappedKlid;
        }

        var toastKind = switched ? AppToastKind.Success : AppToastKind.Error;
        var toastDetail = switched ? $"Layout changed: {mappedKlid}" : $"Layout switch failed: {mappedKlid}";
        AppToastRequested?.Invoke(this, new AppToastEventArgs("Active keyboard", CurrentDeviceDisplayName, toastDetail, toastKind));

        _logger.LogInformation("Layout switch requested deviceId={DeviceId} klid={Klid} switched={Switched}", e.Device.DeviceId, mappedKlid, switched);
    }

    [RelayCommand]
    private void SelectSection(string section)
    {
        _logger.LogInformation("Command dispatched command={Command} section={Section}", nameof(SelectSectionCommand), section);
        SelectedSection = section;
    }

    [RelayCommand]
    private void ClearLogs()
    {
        _logger.LogInformation("Command dispatched command={Command}", nameof(ClearLogsCommand));
        _applicationLogService.Clear();
    }

    [RelayCommand]
    private void CopyAllLogs()
    {
        _logger.LogInformation("Command dispatched command={Command}", nameof(CopyAllLogsCommand));
        CopyAllLogsRequested?.Invoke(this, _applicationLogService.GetAllText());
    }

    [RelayCommand]
    private void SaveSelectedKeyboard()
    {
        if (SelectedKeyboard is null)
        {
            _logger.LogWarning("Save selected keyboard requested with no selection");
            return;
        }

        _logger.LogInformation("Command dispatched command={Command} deviceId={DeviceId}", nameof(SaveSelectedKeyboardCommand), SelectedKeyboard.DeviceId);
        SelectedKeyboard.Alias = SelectedKeyboardAlias;
        var selectedKlid = SelectedKeyboardLayout?.Klid ?? SelectedKeyboardLayoutKlid;
        SelectedKeyboard.AssignedLayoutKlid = selectedKlid;
        _configService.SetDeviceAlias(SelectedKeyboard.DeviceId, SelectedKeyboardAlias);

        if (!string.IsNullOrWhiteSpace(selectedKlid))
        {
            _configService.AssignLayout(SelectedKeyboard.DeviceId, selectedKlid);
        }

        SelectedKeyboardStatusMessage = $"Saved {SelectedKeyboard.DisplayName}";
        HasSelectedKeyboardStatusMessage = true;
    }

    [RelayCommand]
    private void DiscardSelectedKeyboard()
    {
        if (SelectedKeyboard is null)
        {
            return;
        }

        _logger.LogInformation("Command dispatched command={Command} deviceId={DeviceId}", nameof(DiscardSelectedKeyboardCommand), SelectedKeyboard.DeviceId);
        LoadSelectedKeyboardFields(SelectedKeyboard);
        SelectedKeyboard = null;
    }

    [RelayCommand]
    private void ToggleLogScrollLock()
    {
        _logger.LogInformation("Command dispatched command={Command} locked={Locked}", nameof(ToggleLogScrollLockCommand), !IsLogScrollLocked);
        IsLogScrollLocked = !IsLogScrollLocked;
    }

    [RelayCommand]
    private void SaveSettings()
    {
        _logger.LogInformation("Command dispatched command={Command} logLevel={LogLevel}", nameof(SaveSettingsCommand), SelectedLogLevel);
        if (!Enum.TryParse<LogLevel>(SelectedLogLevel, ignoreCase: true, out var level))
        {
            _logger.LogWarning("Invalid log level selected value={Value}", SelectedLogLevel);
            return;
        }

        _updateMinimumLogLevel?.Invoke(level);
    }

    [RelayCommand]
    private void DiscardSettings()
    {
        _logger.LogInformation("Command dispatched command={Command}", nameof(DiscardSettingsCommand));
        SelectedLogLevel = _configService.Load().Logging.MinimumLevel;
    }

    [RelayCommand]
    private void ResetCache()
    {
        _logger.LogInformation("Command dispatched command={Command}", nameof(ResetCacheCommand));
        
        var localAppData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SwitchcraftKeys");
        var cacheDir = Path.Combine(localAppData, "cache");

        if (!Directory.Exists(cacheDir))
        {
            _logger.LogWarning("Cache directory not found");
            return;
        }

        try
        {
            Directory.Delete(cacheDir, true);
            Directory.CreateDirectory(cacheDir);
            _logger.LogInformation("Cache cleared path={Path}", cacheDir);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to clear cache error={Error}", ex.Message);
        }
    }

    [RelayCommand]
    private void ResetData()
    {
        _logger.LogInformation("Command dispatched command={Command}", nameof(ResetDataCommand));
        
        var appData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SwitchcraftKeys");
        var localAppData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SwitchcraftKeys");

        var errors = new List<string>();

        // Delete config + logs
        if (Directory.Exists(appData))
        {
            try
            {
                Directory.Delete(appData, true);
                _logger.LogInformation("Deleted config/logs path={Path}", appData);
            }
            catch (Exception ex)
            {
                errors.Add($"Config/logs: {ex.Message}");
                _logger.LogError("Failed to delete config/logs error={Error}", ex.Message);
            }
        }

        // Delete cache
        if (Directory.Exists(localAppData))
        {
            try
            {
                Directory.Delete(localAppData, true);
                _logger.LogInformation("Deleted cache path={Path}", localAppData);
            }
            catch (Exception ex)
            {
                errors.Add($"Cache: {ex.Message}");
                _logger.LogError("Failed to delete cache error={Error}", ex.Message);
            }
        }

        if (errors.Count == 0)
        {
            _logger.LogInformation("All application data reset");
        }
    }

    [RelayCommand]
    private void OpenSettingsDirectory()
    {
        _logger.LogInformation("Command dispatched command={Command} path={Path}", nameof(OpenSettingsDirectoryCommand), _applicationControlService.SettingsDirectory);
        _applicationControlService.OpenSettingsDirectory();
    }

    [RelayCommand]
    private void OpenCacheDirectory()
    {
        _logger.LogInformation("Command dispatched command={Command} path={Path}", nameof(OpenCacheDirectoryCommand), _applicationControlService.CacheDirectory);
        _applicationControlService.OpenCacheDirectory();
    }

    [RelayCommand]
    private void OpenWindowsKeyboardSettings()
    {
        _logger.LogInformation("Command dispatched command={Command}", nameof(OpenWindowsKeyboardSettingsCommand));
        _applicationControlService.OpenWindowsKeyboardSettings();
    }

    [RelayCommand]
    private void DisablePerAppInputMethod()
    {
        _logger.LogInformation("Command dispatched command={Command}", nameof(DisablePerAppInputMethodCommand));
        try
        {
            _applicationControlService.SetPerAppInputMethodEnabled(false);
            WindowsInputMethodStatus = "Per-app input method disabled. Sign out or restart Explorer if Windows does not apply it immediately.";
        }
        catch (Exception ex)
        {
            WindowsInputMethodStatus = $"Failed to update Windows setting: {ex.Message}";
            _logger.LogError(ex, "Failed to disable Windows per-app input method");
        }
    }

    [RelayCommand]
    private void EnablePerAppInputMethod()
    {
        _logger.LogInformation("Command dispatched command={Command}", nameof(EnablePerAppInputMethodCommand));
        try
        {
            _applicationControlService.SetPerAppInputMethodEnabled(true);
            WindowsInputMethodStatus = "Per-app input method enabled.";
        }
        catch (Exception ex)
        {
            WindowsInputMethodStatus = $"Failed to update Windows setting: {ex.Message}";
            _logger.LogError(ex, "Failed to enable Windows per-app input method");
        }
    }

    [RelayCommand]
    private void RestartApplication()
    {
        _logger.LogInformation("Command dispatched command={Command}", nameof(RestartApplicationCommand));
        _applicationControlService.RestartApplication();
    }

    partial void OnSelectedKeyboardChanged(KeyboardDeviceViewModel? value)
    {
        _logger.LogInformation("Selected keyboard changed deviceId={DeviceId}", value?.DeviceId);
        LoadSelectedKeyboardFields(value);
        SelectedKeyboardStatusMessage = string.Empty;
        HasSelectedKeyboardStatusMessage = false;
        OnPropertyChanged(nameof(HasSelectedKeyboard));
        OnPropertyChanged(nameof(HasNoSelectedKeyboard));
    }

    partial void OnSelectedKeyboardLanguageChanged(string value)
    {
        _logger.LogInformation("Selected keyboard language changed language={Language}", value);
        if (!_isLoadingSelectedKeyboardFields)
        {
            ClearSelectedKeyboardStatus();
        }

        RefreshFilteredLayouts();

        if (!_isLoadingSelectedKeyboardFields && SelectedKeyboardLayout is not null && !FilteredLayouts.Contains(SelectedKeyboardLayout))
        {
            SelectedKeyboardLayout = null;
            SelectedKeyboardLayoutKlid = string.Empty;
        }
    }

    partial void OnSelectedKeyboardLayoutChanged(LayoutInfo? value)
    {
        _logger.LogInformation("Selected keyboard layout changed klid={Klid}", value?.Klid);
        if (!_isLoadingSelectedKeyboardFields)
        {
            ClearSelectedKeyboardStatus();
        }

        SelectedKeyboardLayoutKlid = value?.Klid ?? string.Empty;
    }

    partial void OnSelectedKeyboardAliasChanged(string value)
    {
        _logger.LogTrace("Selected keyboard alias changed value={Value}", value);
        if (_isLoadingSelectedKeyboardFields)
        {
            return;
        }

        ClearSelectedKeyboardStatus();
    }

    partial void OnSelectedSectionChanged(string value)
    {
        _logger.LogInformation("Selected section changed section={Section}", value);
        OnPropertyChanged(nameof(IsDashboardSelected));
        OnPropertyChanged(nameof(IsLogsSelected));
        OnPropertyChanged(nameof(IsSettingsSelected));
        OnPropertyChanged(nameof(IsAboutSelected));
    }

    partial void OnIsLogScrollLockedChanged(bool value)
    {
        _logger.LogInformation("Log scroll lock changed locked={Locked}", value);
        OnPropertyChanged(nameof(IsLogScrollUnlocked));
        LogScrollLockChanged?.Invoke(this, EventArgs.Empty);
    }

    partial void OnCurrentDeviceChanged(string value)
    {
        _logger.LogTrace("Current device property changed value={Value}", value);
        OnPropertyChanged(nameof(CurrentDeviceIsBuiltin));
        OnPropertyChanged(nameof(CurrentDeviceIsBluetooth));
        OnPropertyChanged(nameof(CurrentDeviceIsUsb));
    }

    partial void OnCurrentDeviceDisplayNameChanged(string value)
    {
        _logger.LogTrace("Current device display name changed value={Value}", value);
    }

    partial void OnCurrentLayoutChanged(string value)
    {
        _logger.LogTrace("Current layout property changed value={Value}", value);
    }

    private void LoadConfiguredDevices()
    {
        var config = _configService.Load();
        foreach (var device in config.Devices.Values)
        {
            AddKeyboardViewModel(device);
        }

        _logger.LogDebug("Configured devices loaded count={Count}", Keyboards.Count);
    }

    private void LoadAvailableLayouts()
    {
        AvailableLayouts.Clear();
        AvailableLanguages.Clear();

        AvailableLanguages.Add(AllLanguages);
        foreach (var layout in _layoutService.GetAvailableLayouts())
        {
            AvailableLayouts.Add(layout);
        }

        foreach (var language in AvailableLayouts
            .Select(layout => string.IsNullOrWhiteSpace(layout.LanguageTag) ? "Unknown" : layout.LanguageTag)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(language => language, StringComparer.CurrentCultureIgnoreCase))
        {
            AvailableLanguages.Add(language);
        }

        RefreshFilteredLayouts();
        _logger.LogDebug("Available layouts loaded count={LayoutCount} languages={LanguageCount}", AvailableLayouts.Count, AvailableLanguages.Count);
    }

    private void RefreshFilteredLayouts()
    {
        FilteredLayouts.Clear();
        var selectedLanguage = SelectedKeyboardLanguage;

        var layouts = string.Equals(selectedLanguage, AllLanguages, StringComparison.OrdinalIgnoreCase)
            ? AvailableLayouts
            : AvailableLayouts.Where(layout => string.Equals(layout.LanguageTag ?? "Unknown", selectedLanguage, StringComparison.OrdinalIgnoreCase));

        foreach (var layout in layouts)
        {
            FilteredLayouts.Add(layout);
        }
    }

    private KeyboardDeviceViewModel EnsureKeyboardViewModel(DeviceInfo device)
    {
        var existing = Keyboards.FirstOrDefault(k => string.Equals(k.DeviceId, device.DeviceId, StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
        {
            return existing;
        }

        var config = _configService.Load();
        if (!config.Devices.TryGetValue(device.DeviceId, out var configuredDevice))
        {
            configuredDevice = device;
        }

        var keyboard = AddKeyboardViewModel(configuredDevice);
        _logger.LogInformation("Keyboard added to view model deviceId={DeviceId}", device.DeviceId);
        return keyboard;
    }

    private KeyboardDeviceViewModel AddKeyboardViewModel(DeviceInfo device)
    {
        var keyboard = new KeyboardDeviceViewModel(
            device.DeviceId,
            string.IsNullOrWhiteSpace(device.Alias) ? device.DeviceId : device.Alias,
            device.AssignedLayoutKlid);
        keyboard.PropertyChanged += (_, args) => OnKeyboardPropertyChanged(keyboard, args.PropertyName);
        Keyboards.Add(keyboard);
        return keyboard;
    }

    private void OnKeyboardPropertyChanged(KeyboardDeviceViewModel keyboard, string? propertyName)
    {
        if (propertyName == nameof(KeyboardDeviceViewModel.Alias)
            && string.Equals(CurrentDevice, keyboard.DeviceId, StringComparison.OrdinalIgnoreCase))
        {
            CurrentDeviceDisplayName = keyboard.DisplayName;
        }

        _logger.LogTrace("Keyboard property changed deviceId={DeviceId} property={Property}", keyboard.DeviceId, propertyName);
    }

    private void LoadSelectedKeyboardFields(KeyboardDeviceViewModel? keyboard)
    {
        _isLoadingSelectedKeyboardFields = true;
        try
        {
            SelectedKeyboardAlias = keyboard?.Alias ?? string.Empty;
            SelectedKeyboardLayoutKlid = keyboard?.AssignedLayoutKlid ?? string.Empty;

            var layout = ResolveConfiguredLayout(SelectedKeyboardLayoutKlid);
            SelectedKeyboardLanguage = layout?.LanguageTag ?? AllLanguages;
            RefreshFilteredLayouts();
            SelectedKeyboardLayout = layout is not null && FilteredLayouts.Contains(layout) ? layout : null;

            _logger.LogDebug("Selected keyboard fields loaded deviceId={DeviceId} configuredKlid={ConfiguredKlid} resolvedKlid={ResolvedKlid} language={Language}",
                keyboard?.DeviceId,
                SelectedKeyboardLayoutKlid,
                SelectedKeyboardLayout?.Klid,
                SelectedKeyboardLanguage);
        }
        finally
        {
            _isLoadingSelectedKeyboardFields = false;
        }
    }

    private LayoutInfo? ResolveConfiguredLayout(string configuredKlid)
    {
        if (string.IsNullOrWhiteSpace(configuredKlid))
        {
            return null;
        }

        var exact = AvailableLayouts.FirstOrDefault(layout => string.Equals(layout.Klid, configuredKlid, StringComparison.OrdinalIgnoreCase));
        if (exact is not null)
        {
            return exact;
        }

        // Older config may contain base KLID (0000xxxx), while Windows exposes
        // loaded HKLs as variants (hhhhxxxx). Match by LANGID as fallback.
        if (configuredKlid.Length == 8)
        {
            var languageId = configuredKlid[4..];
            return AvailableLayouts.FirstOrDefault(layout => layout.Klid.Length == 8
                && string.Equals(layout.Klid[4..], languageId, StringComparison.OrdinalIgnoreCase));
        }

        return null;
    }

    private void ClearSelectedKeyboardStatus()
    {
        if (!HasSelectedKeyboardStatusMessage)
        {
            return;
        }

        SelectedKeyboardStatusMessage = string.Empty;
        HasSelectedKeyboardStatusMessage = false;
    }

    private void RefreshWindowsInputMethodStatus()
    {
        try
        {
            WindowsInputMethodStatus = _applicationControlService.GetPerAppInputMethodEnabled() switch
            {
                true => "Windows is configured to allow different input methods per app window.",
                false => "Windows is configured to share input method across app windows.",
                null => "Windows setting not found yet. Use a button below to create/update it.",
            };
        }
        catch (Exception ex)
        {
            WindowsInputMethodStatus = $"Unable to read Windows setting: {ex.Message}";
            _logger.LogError(ex, "Failed to read Windows per-app input method setting");
        }
    }
}
