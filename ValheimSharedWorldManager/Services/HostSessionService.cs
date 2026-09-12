using System.Text.Json;
using ValheimSharedWorldManager.Models;

namespace ValheimSharedWorldManager.Services;

public sealed class HostSessionService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public HostSessionInfo? Read(
        string sharedWorldRoot,
        string world)
    {
        var path = GetSessionPath(sharedWorldRoot, world);

        if (!File.Exists(path))
            return null;

        try
        {
            var json = File.ReadAllText(path);

            return JsonSerializer.Deserialize<HostSessionInfo>(
                json,
                JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public void Save(
        string sharedWorldRoot,
        HostSessionInfo info)
    {
        var path = GetSessionPath(
            sharedWorldRoot,
            info.World);

        var directory = Path.GetDirectoryName(path)
            ?? throw new InvalidOperationException(
                "Could not determine session folder.");

        Directory.CreateDirectory(directory);

        var tempPath = path + ".tmp";

        var json = JsonSerializer.Serialize(
            info,
            JsonOptions);

        File.WriteAllText(tempPath, json);

        File.Move(
            tempPath,
            path,
            overwrite: true);
    }

    public string GetSessionPath(
        string sharedWorldRoot,
        string world)
    {
        var root = GetSharedRoot(sharedWorldRoot);

        return Path.Combine(
            root,
            "Sessions",
            $"{world}.json");
    }

    private static string GetSharedRoot(
        string sharedWorldRoot)
    {
        var parent =
            Directory.GetParent(sharedWorldRoot)?.FullName;

        if (string.IsNullOrWhiteSpace(parent))
        {
            throw new InvalidOperationException(
                "Could not determine shared Valheim root.");
        }

        return parent;
    }
}