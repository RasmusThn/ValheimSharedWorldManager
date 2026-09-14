namespace ValheimSharedWorldManager.Services;

public sealed class OneDriveService
{
    public IReadOnlyList<string> FindOneDriveFolders()
    {
        var results = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);

        AddIfValid(
            results,
            Environment.GetEnvironmentVariable("OneDrive"));

        AddIfValid(
            results,
            Environment.GetEnvironmentVariable("OneDriveConsumer"));

        AddIfValid(
            results,
            Environment.GetEnvironmentVariable("OneDriveCommercial"));

        var userProfile = Environment.GetFolderPath(
            Environment.SpecialFolder.UserProfile);

        if (Directory.Exists(userProfile))
        {
            foreach (var directory in Directory.GetDirectories(
                         userProfile,
                         "OneDrive*",
                         SearchOption.TopDirectoryOnly))
            {
                AddIfValid(results, directory);
            }
        }

        return results
            .OrderBy(x => x)
            .ToList();
    }

    public IReadOnlyList<string> FindSharedValheimFolders()
    {
        var results = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var oneDrive in FindOneDriveFolders())
        {
            SearchForValheimFolders(oneDrive, results);
        }

        return results
            .OrderBy(x => x)
            .ToList();
    }

    private static void SearchForValheimFolders(
        string oneDriveRoot,
        HashSet<string> results)
    {
        try
        {
            foreach (var directory in Directory.GetDirectories(
                         oneDriveRoot,
                         "*",
                         SearchOption.TopDirectoryOnly))
            {
                var worldsLocal = Path.Combine(
                    directory,
                    "worlds_local");

                if (Directory.Exists(worldsLocal))
                {
                    results.Add(worldsLocal);
                }
            }
        }
        catch
        {
            // OneDrive may contain folders we cannot currently access.
        }
    }

    private static void AddIfValid(
        HashSet<string> results,
        string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        try
        {
            if (Directory.Exists(path))
                results.Add(Path.GetFullPath(path));
        }
        catch
        {
            // Ignore invalid paths.
        }
    }
}