namespace ValheimSharedWorldManager.Models;

public sealed class AppSettings
{
    public string LocalWorldRoot { get; set; } = "";
    public string SharedWorldRoot { get; set; } = "";
    public string SelectedWorld { get; set; } = "";
    public bool CreateBackups { get; set; } = true;
    public bool SetupCompleted { get; set; }
    public bool LaunchValheimAutomatically { get; set; } = true;
}
