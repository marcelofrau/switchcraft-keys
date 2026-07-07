using Avalonia.Controls;
using Avalonia.Interactivity;

namespace SwitchcraftKeys.Views;

public partial class ConfirmExitDialog : Window
{
    public ConfirmExitDialog()
    {
        InitializeComponent();
    }

    private void OnCancelClicked(object? sender, RoutedEventArgs e)
    {
        Close(ConfirmExitResult.Cancel);
    }

    private void OnMinimizeToTrayClicked(object? sender, RoutedEventArgs e)
    {
        Close(ConfirmExitResult.MinimizeToTray);
    }

    private void OnExitClicked(object? sender, RoutedEventArgs e)
    {
        Close(ConfirmExitResult.Exit);
    }
}
