using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using SwitchcraftKeys.Interop;
using SwitchcraftKeys.Models;
using SwitchcraftKeys.Services.Interfaces;

namespace SwitchcraftKeys.Services;

public sealed class DeviceService : IDeviceService
{
    private readonly ILogger<DeviceService> _logger;
    private IntPtr _windowHandle;
    private bool _isMonitoring;
    private string? _currentDeviceId;

    public DeviceService(ILogger<DeviceService> logger)
    {
        _logger = logger;
    }

    public event EventHandler<DeviceActivatedEventArgs>? DeviceActivated;

    public IReadOnlyList<DeviceInfo> GetConnectedDevices()
    {
        _logger.LogInformation("Calling GetRawInputDeviceList for keyboard devices");
        var devices = RawInputApi.GetKeyboardDeviceList();
        _logger.LogInformation("GetRawInputDeviceList returned count={Count}", devices.Length);

        var result = new Dictionary<string, DeviceInfo>(StringComparer.OrdinalIgnoreCase);
        foreach (var rawDevice in devices)
        {
            _logger.LogInformation("Calling GetRawInputDeviceInfo hDevice={HDevice}", rawDevice.hDevice);
            var rawPath = RawInputApi.GetDeviceName(rawDevice.hDevice);
            _logger.LogInformation("GetRawInputDeviceInfo returned hDevice={HDevice} rawPath={RawPath}", rawDevice.hDevice, rawPath);

            var deviceId = DeviceIdNormalizer.Normalize(rawPath);
            if (deviceId is null || result.ContainsKey(deviceId))
            {
                continue;
            }

            result[deviceId] = new DeviceInfo
            {
                DeviceId = deviceId,
                Alias = deviceId,
                RawPath = rawPath,
            };
        }

        _logger.LogDebug("Normalized keyboard device count={Count}", result.Count);
        return result.Values.ToArray();
    }

    public Task StartMonitoringAsync(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
        {
            throw new ArgumentException("Window handle is required.", nameof(windowHandle));
        }

        var devices = new[]
        {
            new RAWINPUTDEVICE
            {
                usUsagePage = NativeConstants.HID_USAGE_PAGE_GENERIC,
                usUsage = NativeConstants.HID_USAGE_GENERIC_KEYBOARD,
                dwFlags = NativeConstants.RIDEV_INPUTSINK,
                hwndTarget = windowHandle,
            },
        };

        _logger.LogInformation("Calling RegisterRawInputDevices hwnd={Hwnd} count={Count}", windowHandle, devices.Length);
        var success = RawInputApi.RegisterRawInputDevices(
            devices,
            (uint)devices.Length,
            (uint)Marshal.SizeOf<RAWINPUTDEVICE>());
        var error = Marshal.GetLastWin32Error();
        _logger.LogInformation("RegisterRawInputDevices returned success={Success} error={Error}", success, error);

        if (!success)
        {
            _logger.LogError("RegisterRawInputDevices failed hwnd={Hwnd} error={Error}", windowHandle, error);
            throw new InvalidOperationException($"RegisterRawInputDevices failed with Win32 error {error}.");
        }

        _windowHandle = windowHandle;
        _isMonitoring = true;
        _logger.LogInformation("Raw Input monitoring started hwnd={Hwnd}", windowHandle);
        return Task.CompletedTask;
    }

    public void StopMonitoring()
    {
        if (!_isMonitoring)
        {
            return;
        }

        var devices = new[]
        {
            new RAWINPUTDEVICE
            {
                usUsagePage = NativeConstants.HID_USAGE_PAGE_GENERIC,
                usUsage = NativeConstants.HID_USAGE_GENERIC_KEYBOARD,
                dwFlags = NativeConstants.RIDEV_REMOVE,
                hwndTarget = IntPtr.Zero,
            },
        };

        _logger.LogInformation("Calling RegisterRawInputDevices remove hwnd={Hwnd} count={Count}", _windowHandle, devices.Length);
        var success = RawInputApi.RegisterRawInputDevices(
            devices,
            (uint)devices.Length,
            (uint)Marshal.SizeOf<RAWINPUTDEVICE>());
        var error = Marshal.GetLastWin32Error();
        _logger.LogInformation("RegisterRawInputDevices remove returned success={Success} error={Error}", success, error);

        if (!success)
        {
            _logger.LogWarning("Raw Input unregister failed hwnd={Hwnd} error={Error}", _windowHandle, error);
        }

        _isMonitoring = false;
        _windowHandle = IntPtr.Zero;
        _logger.LogInformation("Raw Input monitoring stopped");
    }

    public void ProcessRawInput(IntPtr lParam)
    {
        _logger.LogTrace("WM_INPUT received lParam={LParam}", lParam);

        uint size = 0;
        var headerSize = (uint)Marshal.SizeOf<RAWINPUTHEADER>();
        _logger.LogInformation("Calling GetRawInputData query lParam={LParam}", lParam);
        RawInputApi.GetRawInputData(lParam, NativeConstants.RID_INPUT, IntPtr.Zero, ref size, headerSize);
        var queryError = Marshal.GetLastWin32Error();
        _logger.LogInformation("GetRawInputData query returned size={Size} error={Error}", size, queryError);

        if (size == 0)
        {
            _logger.LogWarning("Raw input query returned empty size lParam={LParam} error={Error}", lParam, queryError);
            return;
        }

        var buffer = Marshal.AllocHGlobal((int)size);
        try
        {
            _logger.LogInformation("Calling GetRawInputData read lParam={LParam} size={Size}", lParam, size);
            var bytesRead = RawInputApi.GetRawInputData(lParam, NativeConstants.RID_INPUT, buffer, ref size, headerSize);
            var readError = Marshal.GetLastWin32Error();
            _logger.LogInformation("GetRawInputData read returned bytes={Bytes} error={Error}", bytesRead, readError);

            if (bytesRead == uint.MaxValue)
            {
                _logger.LogError("GetRawInputData failed lParam={LParam} error={Error}", lParam, readError);
                return;
            }

            var rawInput = Marshal.PtrToStructure<RAWINPUT>(buffer);
            if (rawInput.header.dwType != NativeConstants.RIM_TYPEKEYBOARD)
            {
                _logger.LogTrace("Ignoring non-keyboard raw input type={Type}", rawInput.header.dwType);
                return;
            }

            var hDevice = rawInput.header.hDevice;
            _logger.LogDebug("Raw input keyboard hDevice={HDevice} vkey={VKey}", hDevice, rawInput.data.keyboard.VKey);
            _logger.LogInformation("Calling GetRawInputDeviceInfo hDevice={HDevice}", hDevice);
            var rawPath = RawInputApi.GetDeviceName(hDevice);
            _logger.LogInformation("GetRawInputDeviceInfo returned hDevice={HDevice} rawPath={RawPath}", hDevice, rawPath);

            var deviceId = DeviceIdNormalizer.Normalize(rawPath);
            if (deviceId is null)
            {
                _logger.LogWarning("Raw input device path could not be normalized rawPath={RawPath}", rawPath);
                return;
            }

            if (string.Equals(_currentDeviceId, deviceId, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogTrace("Device already active deviceId={DeviceId}", deviceId);
                return;
            }

            var previousDevice = _currentDeviceId;
            _currentDeviceId = deviceId;
            _logger.LogInformation("Device activated deviceId={DeviceId} previousDevice={PreviousDevice}", deviceId, previousDevice);

            DeviceActivated?.Invoke(this, new DeviceActivatedEventArgs(new DeviceInfo
            {
                DeviceId = deviceId,
                Alias = deviceId,
                RawPath = rawPath,
            }));
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
}
