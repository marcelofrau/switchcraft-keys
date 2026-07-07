using System;
using System.Runtime.InteropServices;

namespace SwitchcraftKeys.Interop;

/// <summary>
/// Console window detection. Never allocates or attaches — GUI app stays silent.
/// Only used to decide whether to log to console sink.
/// </summary>
public static class ConsoleApi
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetConsoleWindow();

    /// <summary>
    /// Check if console is already attached (from parent shell, --console flag, etc).
    /// </summary>
    public static bool HasConsole => GetConsoleWindow() != IntPtr.Zero;
}
