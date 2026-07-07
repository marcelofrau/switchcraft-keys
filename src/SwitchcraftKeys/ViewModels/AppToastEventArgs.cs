namespace SwitchcraftKeys.ViewModels;

public enum AppToastKind
{
    Info,
    Success,
    Warning,
    Error,
}

public sealed class AppToastEventArgs : EventArgs
{
    public AppToastEventArgs(string title, string message, string detail = "", AppToastKind kind = AppToastKind.Info)
    {
        Title = title;
        Message = message;
        Detail = detail;
        Kind = kind;
    }

    public string Title { get; }

    public string Message { get; }

    public string Detail { get; }

    public AppToastKind Kind { get; }
}
