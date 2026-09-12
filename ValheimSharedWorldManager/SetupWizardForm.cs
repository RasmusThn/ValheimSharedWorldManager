using ValheimSharedWorldManager.Models;
using ValheimSharedWorldManager.Utilities;

namespace ValheimSharedWorldManager;

public sealed class SetupWizardForm : Form
{
    private readonly TextBox _txtLocal = new();
    private readonly TextBox _txtShared = new();
    private readonly Label _lblLocalStatus = new();
    private readonly Label _lblSharedStatus = new();

    public AppSettings Result { get; private set; }

    public SetupWizardForm(AppSettings current)
    {
        Result = current;
        Text = "First setup - Valheim Shared World Manager";
        Width = 760;
        Height = 600;
        MinimumSize = new Size(680, 550);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = AppTheme.Background;
        ForeColor = AppTheme.Text;
        Font = new Font("Segoe UI", 10F);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        BuildUi(current);
    }

    private void BuildUi(AppSettings current)
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24),
            ColumnCount = 1,
            RowCount = 6,
            BackColor = AppTheme.Background
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        root.AutoSize = true;
        root.AutoSizeMode = AutoSizeMode.GrowAndShrink;

        Controls.Add(root);

        root.Controls.Add(AppTheme.Heading("Set up shared Valheim worlds", 20F), 0, 0);
        var subtitle = AppTheme.Muted("Choose where Valheim stores local worlds and where your shared OneDrive folder lives. You can change both later.");
        subtitle.MaximumSize = new Size(680, 0);
        subtitle.Margin = new Padding(0, 6, 0, 18);
        root.Controls.Add(subtitle, 0, 1);

        root.Controls.Add(BuildFolderBlock(
            "1. Local Valheim worlds",
            "Usually: AppData\\LocalLow\\IronGate\\Valheim\\worlds_local",
            _txtLocal,
            _lblLocalStatus,
            string.IsNullOrWhiteSpace(current.LocalWorldRoot) ? AppPaths.GetDefaultLocalWorldRoot() : current.LocalWorldRoot,
            "Choose your local Valheim worlds_local folder"), 0, 2);

        root.Controls.Add(BuildFolderBlock(
            "2. Shared OneDrive worlds",
            "Recommended: OneDrive\\ValheimShared\\worlds_local",
            _txtShared,
            _lblSharedStatus,
            string.IsNullOrWhiteSpace(current.SharedWorldRoot) ? AppPaths.GetSuggestedSharedWorldRoot() : current.SharedWorldRoot,
            "Choose the shared OneDrive worlds_local folder"), 0, 3);

        var note = new Label
        {
            Text = "Tip: Share the parent ValheimShared folder with edit permission. On both PCs, use 'Always keep on this device' in OneDrive.",
            AutoSize = true,
            MaximumSize = new Size(680, 0),
            ForeColor = AppTheme.MutedText,
            Margin = new Padding(0, 18, 0, 0)
        };
        root.Controls.Add(note, 0, 4);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Margin = new Padding(0, 16, 0, 0),
            Padding = new Padding(0, 0, 8, 0)
        };

        var finish = AppTheme.PrimaryButton("Save & continue");
        finish.Width = 150;
        finish.Click += (_, _) => Finish();
        buttons.Controls.Add(finish);

        var cancel = AppTheme.SecondaryButton("Cancel");
        cancel.Width = 100;
        cancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        buttons.Controls.Add(cancel);
        root.Controls.Add(buttons, 0, 5);

        _txtLocal.TextChanged += (_, _) => UpdateStatus(_txtLocal, _lblLocalStatus, expectValheim: true);
        _txtShared.TextChanged += (_, _) => UpdateStatus(_txtShared, _lblSharedStatus, expectValheim: false);
        UpdateStatus(_txtLocal, _lblLocalStatus, true);
        UpdateStatus(_txtShared, _lblSharedStatus, false);
    }

    private Control BuildFolderBlock(
    string title,
    string hint,
    TextBox textBox,
    Label status,
    string initial,
    string dialogTitle)
    {
        var card = new Panel
        {
            Height = 136,
            Dock = DockStyle.Top,
            BackColor = AppTheme.Surface,
            Padding = new Padding(16),
            Margin = new Padding(0, 0, 0, 12)
        };

        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4
        };

        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));

        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));

        card.Controls.Add(table);

        var lblTitle = AppTheme.Heading(title, 11.5F);
        table.Controls.Add(lblTitle, 0, 0);
        table.SetColumnSpan(lblTitle, 2);

        var lblHint = AppTheme.Muted(hint);
        lblHint.Margin = new Padding(0, 2, 0, 8);
        table.Controls.Add(lblHint, 0, 1);
        table.SetColumnSpan(lblHint, 2);

        textBox.Text = initial;
        textBox.Dock = DockStyle.Fill;
        textBox.BackColor = AppTheme.SurfaceAlt;
        textBox.ForeColor = AppTheme.Text;
        textBox.BorderStyle = BorderStyle.FixedSingle;
        textBox.Margin = new Padding(0, 0, 0, 4);
        table.Controls.Add(textBox, 0, 2);

        var browse = AppTheme.SecondaryButton("Browse...");
        browse.Height = 30;
        browse.Dock = DockStyle.Fill;
        browse.Margin = new Padding(8, 0, 0, 4);
        browse.Click += (_, _) => Browse(textBox, dialogTitle);
        table.Controls.Add(browse, 1, 2);

        status.AutoSize = true;
        status.Font = new Font("Segoe UI", 8.5F);
        status.Margin = new Padding(0, 2, 0, 0);

        table.Controls.Add(status, 0, 3);
        table.SetColumnSpan(status, 2);

        return card;
    }

    private void Browse(TextBox target, string title)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = title,
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true,
            SelectedPath = Directory.Exists(target.Text) ? target.Text : ""
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
            target.Text = dialog.SelectedPath;
    }

    private static void UpdateStatus(TextBox box, Label label, bool expectValheim)
    {
        if (string.IsNullOrWhiteSpace(box.Text))
        {
            label.Text = "Not selected";
            label.ForeColor = AppTheme.Warning;
            return;
        }

        if (!Directory.Exists(box.Text))
        {
            label.Text = expectValheim ? "Folder not found yet" : "Folder will be created if possible";
            label.ForeColor = AppTheme.Warning;
            return;
        }

        label.Text = "Folder found";
        label.ForeColor = AppTheme.Success;
    }

    private void Finish()
    {
        var local = _txtLocal.Text.Trim();
        var shared = _txtShared.Text.Trim();

        if (string.IsNullOrWhiteSpace(local) || string.IsNullOrWhiteSpace(shared))
        {
            MessageBox.Show(this, "Choose both folders first.", "Missing folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            Directory.CreateDirectory(local);
            Directory.CreateDirectory(shared);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"One of the folders could not be accessed.\n\n{ex.Message}", "Folder error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        Result.LocalWorldRoot = local;
        Result.SharedWorldRoot = shared;
        Result.SetupCompleted = true;
        DialogResult = DialogResult.OK;
    }
}
