using System.Text.Json;
using ValheimSharedWorldManager.Models;
using ValheimSharedWorldManager.Utilities;

namespace ValheimSharedWorldManager.Services;

public sealed class SettingsService
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    public AppSettings Load()
    {
        try
        {
            if (File.Exists(AppPaths.SettingsFile))
            {
                var json = File.ReadAllText(AppPaths.SettingsFile);
                return JsonSerializer.Deserialize<AppSettings>(json, Options) ?? CreateDefault();
            }
        }
        catch
        {
            // Fall back to defaults. The UI will still be usable.
        }

        return CreateDefault();
    }

    public void Save(AppSettings settings)
    {
        Directory.CreateDirectory(AppPaths.DataRoot);
        File.WriteAllText(AppPaths.SettingsFile, JsonSerializer.Serialize(settings, Options));
    }

    private static AppSettings CreateDefault() => new()
    {
        LocalWorldRoot = AppPaths.GetDefaultLocalWorldRoot(),
        SharedWorldRoot = AppPaths.GetSuggestedSharedWorldRoot(),
        CreateBackups = true
    };
}
