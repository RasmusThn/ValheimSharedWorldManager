using ValheimSharedWorldManager.Utilities;

namespace ValheimSharedWorldManager.Services;

public sealed class LogService
{
    private readonly string _filePath;

    public LogService()
    {
        Directory.CreateDirectory(AppPaths.LogRoot);
        _filePath = Path.Combine(AppPaths.LogRoot, $"log-{DateTime.Now:yyyy-MM-dd}.txt");
    }

    public event Action<string>? MessageLogged;

    public void Info(string message) => Write("INFO", message);
    public void Error(string message) => Write("ERROR", message);

    private void Write(string level, string message)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] {level}: {message}";
        try { File.AppendAllText(_filePath, line + Environment.NewLine); } catch { }
        MessageLogged?.Invoke(line);
    }
}
