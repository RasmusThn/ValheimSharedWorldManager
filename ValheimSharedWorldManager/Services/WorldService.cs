using System.Security.Cryptography;
using System.Text;

namespace ValheimSharedWorldManager.Services;

public sealed class WorldService
{
    public IReadOnlyList<string> DiscoverWorlds(string root)
    {
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
            return Array.Empty<string>();

        return Directory.EnumerateDirectories(root)
            .Where(IsValidWorldFolder)
            .Select(Path.GetFileName)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)
            .OrderBy(x => x, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    public bool IsValidWorldFolder(string folder)
    {
        if (!Directory.Exists(folder))
            return false;

        try
        {
            return Directory.EnumerateFiles(folder, "*.fwl2", SearchOption.TopDirectoryOnly).Any()
                   || Directory.EnumerateFiles(folder, "*.db2", SearchOption.TopDirectoryOnly).Any()
                   || Directory.EnumerateFiles(folder, "*.fwl", SearchOption.TopDirectoryOnly).Any()
                   || Directory.EnumerateFiles(folder, "*.db", SearchOption.TopDirectoryOnly).Any();
        }
        catch
        {
            return false;
        }
    }

    public void MirrorDirectory(string source, string destination)
    {
        if (!Directory.Exists(source))
            throw new DirectoryNotFoundException($"Source folder does not exist: {source}");

        Directory.CreateDirectory(destination);

        var sourceRoot = new DirectoryInfo(source);
        var destinationRoot = new DirectoryInfo(destination);

        CopyRecursive(sourceRoot, destinationRoot);
        DeleteExtras(sourceRoot, destinationRoot);
    }

    public void CopyDirectory(string source, string destination)
    {
        if (!Directory.Exists(source))
            throw new DirectoryNotFoundException($"Source folder does not exist: {source}");

        Directory.CreateDirectory(destination);
        CopyRecursive(new DirectoryInfo(source), new DirectoryInfo(destination));
    }

    public string BuildFingerprint(string folder)
    {
        if (!Directory.Exists(folder))
            return "MISSING";

        var root = Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var sb = new StringBuilder();

        foreach (var file in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories)
                     .OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            var info = new FileInfo(file);
            var relative = Path.GetFullPath(file)[root.Length..].Replace('\\', '/');
            sb.Append(relative).Append('|')
                .Append(info.Length).Append('|')
                .Append(info.LastWriteTimeUtc.Ticks).Append('\n');
        }

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString())));
    }

    private static void CopyRecursive(DirectoryInfo source, DirectoryInfo destination)
    {
        foreach (var file in source.EnumerateFiles())
        {
            file.CopyTo(Path.Combine(destination.FullName, file.Name), true);
        }

        foreach (var directory in source.EnumerateDirectories())
        {
            if ((directory.Attributes & FileAttributes.ReparsePoint) != 0)
                continue;

            var target = destination.CreateSubdirectory(directory.Name);
            CopyRecursive(directory, target);
        }
    }

    private static void DeleteExtras(DirectoryInfo source, DirectoryInfo destination)
    {
        var sourceFiles = source.EnumerateFiles().Select(f => f.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var file in destination.EnumerateFiles())
        {
            if (!sourceFiles.Contains(file.Name))
                file.Delete();
        }

        var sourceDirectories = source.EnumerateDirectories()
            .Where(d => (d.Attributes & FileAttributes.ReparsePoint) == 0)
            .Select(d => d.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var directory in destination.EnumerateDirectories())
        {
            if ((directory.Attributes & FileAttributes.ReparsePoint) != 0)
                continue;

            if (!sourceDirectories.Contains(directory.Name))
            {
                directory.Delete(true);
                continue;
            }

            DeleteExtras(
                new DirectoryInfo(Path.Combine(source.FullName, directory.Name)),
                directory);
        }
    }
}
