using SwitchcraftKeys.Models;

namespace SwitchcraftKeys.Services.Interfaces;

public interface IDeviceService
{
    event EventHandler<DeviceActivatedEventArgs>? DeviceActivated;

    IReadOnlyList<DeviceInfo> GetConnectedDevices();

    Task StartMonitoringAsync(IntPtr windowHandle);

    void StopMonitoring();

    void ProcessRawInput(IntPtr lParam);
}

public sealed class DeviceActivatedEventArgs : EventArgs
{
    public DeviceActivatedEventArgs(DeviceInfo device)
    {
        Device = device;
    }

    public DeviceInfo Device { get; }
}
