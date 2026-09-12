using System.Text.Json;
using ValheimSharedWorldManager.Models;

namespace ValheimSharedWorldManager.Services;

public sealed class LockService
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public string GetMetadataRoot(string sharedWorldRoot) => Path.Combine(sharedWorldRoot, ".valheim-sync");
    public string GetLockRoot(string sharedWorldRoot) => Path.Combine(GetMetadataRoot(sharedWorldRoot), "locks");
    public string GetLockPath(string sharedWorldRoot, string world) =>
        Path.Combine(GetLockRoot(sharedWorldRoot), SafeName(world) + ".lock.json");

    public HostLock? ReadLock(string sharedWorldRoot, string world)
    {
        var path = GetLockPath(sharedWorldRoot, world);
        if (!File.Exists(path)) return null;

        try
        {
            return JsonSerializer.Deserialize<HostLock>(File.ReadAllText(path), Options);
        }
        catch
        {
            return new HostLock
            {
                World = world,
                Machine = "Unknown",
                User = "Unknown",
                Token = "UNREADABLE",
                StartedUtc = File.GetLastWriteTimeUtc(path)
            };
        }
    }

    public HostLock CreateLock(string sharedWorldRoot, string world)
    {
        Directory.CreateDirectory(GetLockRoot(sharedWorldRoot));
        var path = GetLockPath(sharedWorldRoot, world);

        var hostLock = new HostLock
        {
            World = world,
            Token = Guid.NewGuid().ToString("N"),
            Machine = Environment.MachineName,
            User = Environment.UserName,
            StartedUtc = DateTime.UtcNow
        };

        var bytes = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(hostLock, Options));

        using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        stream.Write(bytes, 0, bytes.Length);
        stream.Flush(true);

        return hostLock;
    }

    public bool IsOurLock(string sharedWorldRoot, string world, string token)
    {
        var current = ReadLock(sharedWorldRoot, world);
        return current != null && string.Equals(current.Token, token, StringComparison.Ordinal);
    }

    public void ReleaseLock(string sharedWorldRoot, string world, string token, bool force = false)
    {
        var path = GetLockPath(sharedWorldRoot, world);
        if (!File.Exists(path)) return;

        if (!force && !IsOurLock(sharedWorldRoot, world, token))
            throw new InvalidOperationException("The host lock no longer belongs to this session. It was not removed.");

        File.Delete(path);
    }

    private static string SafeName(string value)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            value = value.Replace(c, '_');
        return value;
    }
}
