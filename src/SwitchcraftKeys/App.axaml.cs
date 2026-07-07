using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SwitchcraftKeys.Logging;
using SwitchcraftKeys.Services;
using SwitchcraftKeys.Services.Interfaces;
using SwitchcraftKeys.ViewModels;
using SwitchcraftKeys.Views;

namespace SwitchcraftKeys;

public partial class App : Application
{
    // Set by Program.cs before StartWithClassicDesktopLifetime. Fallbacks
    // below keep designer/test contexts (which never call Main) working.
    public static ILoggerFactory LoggerFactory { get; set; } = NullLoggerFactory.Instance;

    public static IConfigService ConfigService { get; set; } = new ConfigService();

    public static IDeviceService? DeviceService { get; set; }

    public static ILayoutService? LayoutService { get; set; }

    public static IApplicationControlService? ApplicationControlService { get; set; }

    public static ApplicationLogService ApplicationLogService { get; set; } = new();

    public static Action<LogLevel>? UpdateMinimumLogLevel { get; set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var deviceService = DeviceService ?? new DeviceService(LoggerFactory.CreateLogger<DeviceService>());
            var layoutService = LayoutService ?? new LayoutService(LoggerFactory.CreateLogger<LayoutService>());
            var applicationControlService = ApplicationControlService ?? new ApplicationControlService(LoggerFactory.CreateLogger<ApplicationControlService>());

            desktop.MainWindow = new MainWindow(deviceService, LoggerFactory.CreateLogger<MainWindow>())
            {
                DataContext = new MainViewModel(
                    LoggerFactory.CreateLogger<MainViewModel>(),
                    ConfigService,
                    deviceService,
                    layoutService,
                    applicationControlService,
                    ApplicationLogService,
                    UpdateMinimumLogLevel),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
