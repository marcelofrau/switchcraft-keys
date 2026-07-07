using CommunityToolkit.Mvvm.ComponentModel;

namespace SwitchcraftKeys.ViewModels;

public sealed partial class KeyboardDeviceViewModel : ObservableObject
{
    public KeyboardDeviceViewModel(string deviceId, string alias, string? assignedLayoutKlid)
    {
        DeviceId = deviceId;
        Alias = alias;
        AssignedLayoutKlid = assignedLayoutKlid ?? string.Empty;
    }

    public string DeviceId { get; }

    public bool IsBuiltin => string.Equals(DeviceId, "BUILTIN", StringComparison.OrdinalIgnoreCase);

    public bool IsBluetooth => DeviceId.Contains("PID_B380", StringComparison.OrdinalIgnoreCase);

    public bool IsUsb => !IsBuiltin && !IsBluetooth;

    [ObservableProperty]
    private string _alias;

    [ObservableProperty]
    private string _assignedLayoutKlid;

    public string DeviceType => DeviceId == "BUILTIN"
        ? "Built-in keyboard"
        : DeviceId.Contains("PID_B380", StringComparison.OrdinalIgnoreCase)
            ? "Bluetooth keyboard"
            : "External keyboard";

    public string DisplayName => string.IsNullOrWhiteSpace(Alias) ? DeviceId : Alias;

    partial void OnAliasChanged(string value)
    {
        OnPropertyChanged(nameof(DisplayName));
    }
}
