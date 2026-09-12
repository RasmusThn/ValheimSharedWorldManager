namespace ValheimSharedWorldManager;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        Application.ThreadException += (_, e) =>
            MessageBox.Show($"Unexpected error:\n\n{e.Exception.Message}", "Valheim Shared World Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            var message = e.ExceptionObject is Exception ex ? ex.Message : e.ExceptionObject?.ToString() ?? "Unknown error";
            MessageBox.Show($"Unexpected error:\n\n{message}", "Valheim Shared World Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);
        };

        Application.Run(new MainForm());
    }
}
