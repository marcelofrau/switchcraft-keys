namespace SwitchcraftKeys.Services.Interfaces;

public interface IApplicationControlService
{
    string SettingsDirectory { get; }

    string CacheDirectory { get; }

    void OpenSettingsDirectory();

    void OpenCacheDirectory();

    void OpenWindowsKeyboardSettings();

    bool? GetPerAppInputMethodEnabled();

    void SetPerAppInputMethodEnabled(bool enabled);

    void RestartApplication();
}
