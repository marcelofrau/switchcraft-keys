using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using SwitchcraftKeys.ViewModels;

namespace SwitchcraftKeys.Views;

public partial class AppToastWindow : Window
{
    private readonly DispatcherTimer _closeTimer;
    private readonly Window? _owner;

    public AppToastWindow()
        : this(new AppToastEventArgs("SwitchcraftKeys", "Ready"))
    {
    }

    public AppToastWindow(AppToastEventArgs toast, Window? owner = null)
    {
        InitializeComponent();

        _owner = owner;

        TitleText.Text = toast.Title;
        MessageText.Text = toast.Message;
        DetailText.Text = toast.Detail;
        DetailText.IsVisible = !string.IsNullOrWhiteSpace(toast.Detail);
        ApplyKind(toast.Kind);

        _closeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        _closeTimer.Tick += (_, _) => Close();

        Opened += OnOpened;
        Closed += (_, _) => _closeTimer.Stop();
    }

    private void ApplyKind(AppToastKind kind)
    {
        var (background, border, muted) = kind switch
        {
            AppToastKind.Success => ("#1C58B9", "#BFD7F4", "#D5E9FF"),
            AppToastKind.Warning => ("#B55A00", "#FFE0A3", "#FFF1D6"),
            AppToastKind.Error => ("#8E2A2A", "#F0B8B8", "#FFE0E0"),
            _ => ("#1C58B9", "#BFD7F4", "#D5E9FF"),
        };

        Background = SolidColorBrush.Parse(background);
        RootBorder.Background = SolidColorBrush.Parse(background);
        RootBorder.BorderBrush = SolidColorBrush.Parse(border);
        TitleText.Foreground = SolidColorBrush.Parse(muted);
        DetailText.Foreground = SolidColorBrush.Parse(muted);
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        var screen = _owner is not null
            ? Screens.ScreenFromWindow(_owner)
            : Screens.ScreenFromWindow(this);
        screen ??= Screens.Primary;
        if (screen is not null)
        {
            const int margin = 24;
            const int taskbarPadding = 44;
            var area = screen.WorkingArea;
            var pixelWidth = (int)Math.Ceiling(Width * screen.Scaling);
            var pixelHeight = (int)Math.Ceiling(Height * screen.Scaling);
            Position = new PixelPoint(
                area.Right - pixelWidth - margin,
                area.Bottom - pixelHeight - margin - taskbarPadding);
        }

        _closeTimer.Start();
    }
}
