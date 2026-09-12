using System.Diagnostics;

namespace ValheimSharedWorldManager.Services;

public sealed class ValheimService
{
    public bool IsRunning() => Process.GetProcessesByName("valheim").Length > 0;

    public void Start()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "steam://rungameid/892970",
            UseShellExecute = true
        });
    }

    public async Task WaitForStartAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (IsRunning()) return;
            await Task.Delay(1000, cancellationToken);
        }

        throw new TimeoutException("Valheim did not start within the expected time.");
    }

    public async Task WaitForExitAsync(CancellationToken cancellationToken)
    {
        while (IsRunning())
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(2000, cancellationToken);
        }
    }
}
