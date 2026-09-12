namespace ValheimSharedWorldManager.Utilities;

public static class AppPaths
{
    public static string DataRoot => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ValheimSharedWorldManager");

    public static string SettingsFile => Path.Combine(DataRoot, "settings.json");
    public static string LogRoot => Path.Combine(DataRoot, "logs");

    public static string GetDefaultLocalWorldRoot()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "AppData", "LocalLow", "IronGate", "Valheim", "worlds_local");
    }

    public static string GetSuggestedSharedWorldRoot()
    {
        var oneDrive = Environment.GetEnvironmentVariable("OneDrive");
        if (string.IsNullOrWhiteSpace(oneDrive))
            oneDrive = Environment.GetEnvironmentVariable("OneDriveConsumer");
        if (string.IsNullOrWhiteSpace(oneDrive))
            oneDrive = Environment.GetEnvironmentVariable("OneDriveCommercial");

        return string.IsNullOrWhiteSpace(oneDrive)
            ? ""
            : Path.Combine(oneDrive, "ValheimShared", "worlds_local");
    }
}
