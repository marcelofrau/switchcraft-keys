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
        Close(false);
    }

    private void OnExitClicked(object? sender, RoutedEventArgs e)
    {
        Close(true);
    }
}
