using Avalonia.Controls;
using Avalonia.Input.Platform;
using Microsoft.Extensions.Logging;
using SwitchcraftKeys.Interop;
using SwitchcraftKeys.Services.Interfaces;
using SwitchcraftKeys.ViewModels;

namespace SwitchcraftKeys.Views;

public partial class MainWindow : Window
{
    private readonly IDeviceService _deviceService;
    private readonly ILogger<MainWindow> _logger;
    private readonly Win32Properties.CustomWndProcHookCallback _wndProcHook;
    private MainViewModel? _viewModel;
    private bool _isCloseConfirmed;

    public MainWindow()
        : this(
            App.DeviceService ?? throw new InvalidOperationException("Device service is not configured."),
            App.LoggerFactory.CreateLogger<MainWindow>())
    {
    }

    public MainWindow(IDeviceService deviceService, ILogger<MainWindow> logger)
    {
        _deviceService = deviceService;
        _logger = logger;
        _wndProcHook = HandleWndProc;

        InitializeComponent();

        DataContextChanged += OnDataContextChanged;
        Opened += OnOpened;
        Closing += OnClosing;
        Closed += OnClosed;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel is not null)
        {
            _viewModel.CopyAllLogsRequested -= OnCopyAllLogsRequested;
            _viewModel.LogScrollLockChanged -= OnLogScrollLockChanged;
            _viewModel.LogEntries.CollectionChanged -= OnLogEntriesChanged;
        }

        if (DataContext is MainViewModel viewModel)
        {
            _viewModel = viewModel;
            viewModel.CopyAllLogsRequested += OnCopyAllLogsRequested;
            viewModel.LogScrollLockChanged += OnLogScrollLockChanged;
            viewModel.LogEntries.CollectionChanged += OnLogEntriesChanged;
        }
    }

    private void OnLogEntriesChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (_viewModel?.IsLogScrollLocked == true)
        {
            return;
        }

        LogsScrollViewer?.ScrollToEnd();
    }

    private void OnLogScrollLockChanged(object? sender, EventArgs e)
    {
        if (_viewModel?.IsLogScrollLocked == false)
        {
            LogsScrollViewer?.ScrollToEnd();
        }
    }

    private async void OnCopyAllLogsRequested(object? sender, string text)
    {
        _logger.LogInformation("Copy all logs requested length={Length}", text.Length);
        if (Clipboard is not null)
        {
            await Clipboard.SetTextAsync(text);
        }
    }

    private async void OnOpened(object? sender, EventArgs e)
    {
        _logger.LogInformation("View activated view={View}", nameof(MainWindow));
        Win32Properties.AddWndProcHookCallback(this, _wndProcHook);

        var handle = TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (handle == IntPtr.Zero)
        {
            _logger.LogError("Window platform handle unavailable view={View}", nameof(MainWindow));
            return;
        }

        await _deviceService.StartMonitoringAsync(handle);
    }

    private async void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_isCloseConfirmed)
        {
            return;
        }

        e.Cancel = true;
        _logger.LogInformation("Exit confirmation requested view={View}", nameof(MainWindow));
        var dialog = new ConfirmExitDialog();
        var shouldExit = await dialog.ShowDialog<bool>(this);
        _logger.LogInformation("Exit confirmation completed shouldExit={ShouldExit}", shouldExit);
        if (!shouldExit)
        {
            return;
        }

        _isCloseConfirmed = true;
        Close();
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _deviceService.StopMonitoring();
        Win32Properties.RemoveWndProcHookCallback(this, _wndProcHook);
        _logger.LogInformation("View closed view={View}", nameof(MainWindow));
    }

    private IntPtr HandleWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeConstants.WM_INPUT)
        {
            _logger.LogTrace("Window message received msg={Message} hwnd={Hwnd} lParam={LParam}", msg, hWnd, lParam);
            _deviceService.ProcessRawInput(lParam);
        }

        return IntPtr.Zero;
    }
}
