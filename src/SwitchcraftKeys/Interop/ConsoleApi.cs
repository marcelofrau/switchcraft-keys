using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace SwitchcraftKeys.Interop;

/// <summary>
/// Console window helpers. Default GUI startup stays silent; <c>--console</c>
/// can allocate one explicitly for diagnostics.
/// </summary>
public static class ConsoleApi
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AllocConsole();

    /// <summary>
    /// Check if console is already attached (from parent shell, --console flag, etc).
    /// </summary>
    public static bool HasConsole => GetConsoleWindow() != IntPtr.Zero;

    public static bool EnsureConsole()
    {
        if (HasConsole)
        {
            return true;
        }

        if (!AllocConsole())
        {
            return false;
        }

        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        Console.SetError(new StreamWriter(Console.OpenStandardError()) { AutoFlush = true });
        Console.SetIn(new StreamReader(Console.OpenStandardInput()));

        return true;
    }
}
