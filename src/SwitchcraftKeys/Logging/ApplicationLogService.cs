using System.Collections.ObjectModel;
using System.Text;
using Avalonia.Threading;

namespace SwitchcraftKeys.Logging;

public sealed class ApplicationLogService
{
    private const int MaxEntries = 2_000;

    public ObservableCollection<LogEntry> Entries { get; } = [];

    public void Add(LogEntry entry)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            AddCore(entry);
            return;
        }

        Dispatcher.UIThread.Post(() => AddCore(entry));
    }

    public void Clear()
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            Entries.Clear();
            return;
        }

        Dispatcher.UIThread.Post(Entries.Clear);
    }

    public string GetAllText()
    {
        var builder = new StringBuilder();
        foreach (var entry in Entries)
        {
            builder.AppendLine(entry.Text);
        }

        return builder.ToString();
    }

    private void AddCore(LogEntry entry)
    {
        Entries.Add(entry);
        while (Entries.Count > MaxEntries)
        {
            Entries.RemoveAt(0);
        }
    }
}
