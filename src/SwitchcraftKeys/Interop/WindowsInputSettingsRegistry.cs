using Microsoft.Win32;

namespace SwitchcraftKeys.Interop;

internal static class WindowsInputSettingsRegistry
{
    private const string UserProfileKeyPath = @"Control Panel\International\User Profile";
    private const string EnablePerProcessInputMethodValueName = "EnablePerProcessInputMethod";

    public static bool? GetPerAppInputMethodEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(UserProfileKeyPath, writable: false);
        var value = key?.GetValue(EnablePerProcessInputMethodValueName);
        return value switch
        {
            int intValue => intValue != 0,
            _ => null,
        };
    }

    public static void SetPerAppInputMethodEnabled(bool enabled)
    {
        using var key = Registry.CurrentUser.CreateSubKey(UserProfileKeyPath, writable: true);
        key.SetValue(EnablePerProcessInputMethodValueName, enabled ? 1 : 0, RegistryValueKind.DWord);
    }
}
