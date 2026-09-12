namespace ValheimSharedWorldManager.Models;

public sealed class HostLock
{
    public string World { get; set; } = "";
    public string Token { get; set; } = "";
    public string Machine { get; set; } = "";
    public string User { get; set; } = "";
    public DateTime StartedUtc { get; set; }
}
