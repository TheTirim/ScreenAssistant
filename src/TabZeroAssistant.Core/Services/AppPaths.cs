namespace TabZeroAssistant.Core.Services;

public static class AppPaths
{
    public static string BaseDirectory
    {
        get
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "TabZeroAssistant");
        }
    }

    public static string DatabasePath => Path.Combine(BaseDirectory, "assistant.db");
    public static string MasterKeyPath => Path.Combine(BaseDirectory, "masterkey.protected");
    public static string SettingsPath => Path.Combine(BaseDirectory, "settings.json");
}
