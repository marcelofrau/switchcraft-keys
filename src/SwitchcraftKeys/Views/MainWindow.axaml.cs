using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Platform;
using Avalonia.Threading;
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
    private AppToastWindow? _activeToast;
    private TrayIcon? _trayIcon;
    private bool _isRestoringFromTray;
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

        InitializeTrayIcon();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == WindowStateProperty
            && WindowState == WindowState.Minimized
            && !_isRestoringFromTray)
        {
            _logger.LogInformation("Window minimized; hiding to tray view={View}", nameof(MainWindow));
            HideToTray();
        }
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel is not null)
        {
            _viewModel.CopyAllLogsRequested -= OnCopyAllLogsRequested;
            _viewModel.LogScrollLockChanged -= OnLogScrollLockChanged;
            _viewModel.LogEntries.CollectionChanged -= OnLogEntriesChanged;
            _viewModel.AppToastRequested -= OnAppToastRequested;
        }

        if (DataContext is MainViewModel viewModel)
        {
            _viewModel = viewModel;
            viewModel.CopyAllLogsRequested += OnCopyAllLogsRequested;
            viewModel.LogScrollLockChanged += OnLogScrollLockChanged;
            viewModel.LogEntries.CollectionChanged += OnLogEntriesChanged;
            viewModel.AppToastRequested += OnAppToastRequested;
        }
    }

    private void OnAppToastRequested(object? sender, AppToastEventArgs e)
    {
        _logger.LogInformation("App toast requested title={Title} message={Message} detail={Detail} kind={Kind}", e.Title, e.Message, e.Detail, e.Kind);
        _activeToast?.Close();
        _activeToast = new AppToastWindow(e, this);
        _activeToast.Closed += (_, _) => _activeToast = null;
        _activeToast.Show();
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
        var result = await dialog.ShowDialog<ConfirmExitResult>(this);
        _logger.LogInformation("Exit confirmation completed result={Result}", result);
        if (result == ConfirmExitResult.Cancel)
        {
            return;
        }

        if (result == ConfirmExitResult.MinimizeToTray)
        {
            HideToTray();
            return;
        }

        _isCloseConfirmed = true;
        Close();
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _deviceService.StopMonitoring();
        _activeToast?.Close();
        _trayIcon?.Dispose();
        Win32Properties.RemoveWndProcHookCallback(this, _wndProcHook);
        _logger.LogInformation("View closed view={View}", nameof(MainWindow));
    }

    private void InitializeTrayIcon()
    {
        var openItem = new NativeMenuItem("Open SwitchcraftKeys");
        openItem.Click += (_, _) => Dispatcher.UIThread.Post(ShowFromTray);

        var hideItem = new NativeMenuItem("Minimize to tray");
        hideItem.Click += (_, _) => Dispatcher.UIThread.Post(HideToTray);

        var exitItem = new NativeMenuItem("Exit");
        exitItem.Click += (_, _) => Dispatcher.UIThread.Post(ExitFromTray);

        var menu = new NativeMenu();
        menu.Add(openItem);
        menu.Add(hideItem);
        menu.Add(new NativeMenuItemSeparator());
        menu.Add(exitItem);

        _trayIcon = new TrayIcon
        {
            Icon = new WindowIcon(AssetLoader.Open(new Uri("avares://SwitchcraftKeys/Assets/icon.ico"))),
            ToolTipText = "SwitchcraftKeys - keyboard layouts by device",
            Menu = menu,
        };
        _trayIcon.Clicked += (_, _) => Dispatcher.UIThread.Post(ShowFromTray);

        var icons = TrayIcon.GetIcons(App.Current!) ?? new TrayIcons();
        icons.Add(_trayIcon);
        TrayIcon.SetIcons(App.Current!, icons);
    }

    private void HideToTray()
    {
        _logger.LogInformation("Window hidden to tray view={View}", nameof(MainWindow));
        WindowState = WindowState.Normal;
        Hide();
    }

    private void ShowFromTray()
    {
        _logger.LogInformation("Window restored from tray view={View}", nameof(MainWindow));
        _isRestoringFromTray = true;
        Show();
        WindowState = WindowState.Normal;
        Activate();
        _isRestoringFromTray = false;
    }

    private void ExitFromTray()
    {
        _logger.LogInformation("Exit requested from tray view={View}", nameof(MainWindow));
        _isCloseConfirmed = true;
        Close();
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
