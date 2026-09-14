using ValheimSharedWorldManager.Models;
using ValheimSharedWorldManager.Services;
using ValheimSharedWorldManager.Utilities;

namespace ValheimSharedWorldManager;

public sealed class SetupWizardForm : Form
{
    private readonly TextBox _txtLocal = new();
    private readonly TextBox _txtShared = new();
    private readonly Label _lblLocalStatus = new();
    private readonly Label _lblSharedStatus = new();

    private readonly OneDriveService _oneDriveService = new();

    private SetupMode? _setupMode;

    private readonly Panel _modePanel = new();
    private readonly Panel _folderPanel = new();

    private readonly ComboBox _cmbDetectedShared = new();

    public AppSettings Result { get; private set; }

    public SetupWizardForm(AppSettings current, bool showModeSelector = false)
    {
        Result = current;

        Text = "Setup - Valheim Shared World Manager";
        Width = 760;
        Height = 720;
        MinimumSize = new Size(680, 550);

        StartPosition = FormStartPosition.CenterParent;
        BackColor = AppTheme.Background;
        ForeColor = AppTheme.Text;
        Font = new Font("Segoe UI", 10F);

        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        BuildUi(current, showModeSelector);
    }

    private void BuildUi(AppSettings current, bool showModeSelector = false)
    {
        Controls.Clear();

        if (showModeSelector || !current.SetupCompleted)
        {
            ShowModeSelector();
            return;
        }

        _setupMode = SetupMode.CreateOrShare;
        ShowFolderSetup(current);
    }
    private void ShowFolderSetup(AppSettings current)
    {
        Controls.Clear();

        var scrollContainer = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = AppTheme.Background
        };

        Controls.Add(scrollContainer);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Padding = new Padding(24),
            ColumnCount = 1,
            RowCount = 7,
            BackColor = AppTheme.Background,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };

        for (var i = 0; i < 7; i++)
        {
            root.RowStyles.Add(
                new RowStyle(SizeType.AutoSize));
        }

        scrollContainer.Controls.Add(root);

        var title =
            _setupMode == SetupMode.JoinSharedWorld
                ? "Join a shared Valheim world"
                : "Share a Valheim world";

        root.Controls.Add(
            AppTheme.Heading(title, 20F),
            0,
            0);

        var subtitleText =
            _setupMode == SetupMode.JoinSharedWorld
                ? "Add your friend's shared folder to OneDrive, then select it below."
                : "Choose your local Valheim folder and the OneDrive folder you want to share.";

        var subtitle = AppTheme.Muted(subtitleText);
        subtitle.MaximumSize = new Size(680, 0);
        subtitle.Margin = new Padding(0, 6, 0, 18);

        root.Controls.Add(subtitle, 0, 1);

        // Local Valheim folder
        root.Controls.Add(
            BuildFolderBlock(
                "1. Local Valheim worlds",
                @"Usually: AppData\LocalLow\IronGate\Valheim\worlds_local",
                _txtLocal,
                _lblLocalStatus,
                string.IsNullOrWhiteSpace(current.LocalWorldRoot)
                    ? AppPaths.GetDefaultLocalWorldRoot()
                    : current.LocalWorldRoot,
                "Choose your local Valheim worlds_local folder"),
            0,
            2);

        // Join mode gets an extra OneDrive helper.
        if (_setupMode == SetupMode.JoinSharedWorld)
        {
            root.Controls.Add(
                BuildOneDriveJoinGuide(),
                0,
                3);
        }

        // Shared folder
        root.Controls.Add(
            BuildFolderBlock(
                "2. Shared OneDrive worlds",
                @"Select the shared OneDrive worlds_local folder",
                _txtShared,
                _lblSharedStatus,
                string.IsNullOrWhiteSpace(current.SharedWorldRoot)
                    ? AppPaths.GetSuggestedSharedWorldRoot()
                    : current.SharedWorldRoot,
                "Choose the shared OneDrive worlds_local folder"),
            0,
            4);

        var noteText =
            _setupMode == SetupMode.JoinSharedWorld
                ? "Tip: Make sure the shared OneDrive folder has finished syncing and is available in File Explorer."
                : "Tip: Share the parent folder with edit permission. Your friend should choose 'Add shortcut to My files'.";

        var note = new Label
        {
            Text = noteText,
            AutoSize = true,
            MaximumSize = new Size(680, 0),
            ForeColor = AppTheme.MutedText,
            Margin = new Padding(0, 18, 0, 0)
        };

        root.Controls.Add(note, 0, 5);

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

        var back = AppTheme.SecondaryButton("Back");
        back.Width = 100;

        back.Click += (_, _) =>
        {
            ShowModeSelector();
        };

        buttons.Controls.Add(back);

        var cancel = AppTheme.SecondaryButton("Cancel");
        cancel.Width = 100;
        cancel.Click += (_, _) =>
            DialogResult = DialogResult.Cancel;

        buttons.Controls.Add(cancel);

        root.Controls.Add(buttons, 0, 6);

        _txtLocal.TextChanged += (_, _) =>
            UpdateStatus(
                _txtLocal,
                _lblLocalStatus,
                expectValheim: true);

        _txtShared.TextChanged += (_, _) =>
            UpdateStatus(
                _txtShared,
                _lblSharedStatus,
                expectValheim: false);

        UpdateStatus(
            _txtLocal,
            _lblLocalStatus,
            true);

        UpdateStatus(
            _txtShared,
            _lblSharedStatus,
            false);
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
    
    private void ShowModeSelector()
    {
        Controls.Clear();

        _setupMode = null;

        _modePanel.Controls.Clear();
        _modePanel.Dock = DockStyle.Fill;

        _modePanel.Controls.Add(BuildModeSelector());

        Controls.Add(_modePanel);
    }

    private Control BuildModeSelector()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = AppTheme.Background
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(24)
        };

        panel.Controls.Add(layout);

        var heading = AppTheme.Heading(
            "Set up Valheim sharing",
            20F);

        layout.Controls.Add(heading, 0, 0);

        var description = AppTheme.Muted(
            "Choose what you want to do on this computer.");

        description.Margin = new Padding(0, 6, 0, 20);

        layout.Controls.Add(description, 0, 1);

        var create = CreateSetupChoiceButton(
            "CREATE / SHARE A WORLD",
            "I already have the Valheim world and want to share it with friends.");

        create.Click += (_, _) =>
            SelectSetupMode(SetupMode.CreateOrShare);

        layout.Controls.Add(create, 0, 2);

        var join = CreateSetupChoiceButton(
            "JOIN A SHARED WORLD",
            "A friend has already shared the Valheim folder with me.");

        join.Margin = new Padding(0, 12, 0, 0);

        join.Click += (_, _) =>
            SelectSetupMode(SetupMode.JoinSharedWorld);

        layout.Controls.Add(join, 0, 3);

        return panel;
    }

    private Control BuildOneDriveJoinGuide()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            BackColor = AppTheme.Surface,
            Padding = new Padding(16),
            Margin = new Padding(0, 0, 0, 12)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 4
        };

        panel.Controls.Add(layout);

        var heading = AppTheme.Heading(
            "Connect shared OneDrive folder",
            11.5F);

        layout.Controls.Add(heading, 0, 0);

        var instructions = AppTheme.Muted(
            "1. Open the OneDrive link your friend sent you.\r\n" +
            "2. Choose 'Add shortcut to My files'.\r\n" +
            "3. Wait until the folder appears in File Explorer.\r\n" +
            "4. Click 'Find shared folder' below.");

        instructions.Margin =
            new Padding(0, 6, 0, 12);

        layout.Controls.Add(
            instructions,
            0,
            1);

        var find = AppTheme.SecondaryButton(
            "Find shared folder");

        find.AutoSize = true;

        find.Click += (_, _) => DetectSharedFolders();

        layout.Controls.Add(find, 0,2);

        _cmbDetectedShared.Dock = DockStyle.Top;
        _cmbDetectedShared.DropDownStyle =
            ComboBoxStyle.DropDownList;

        _cmbDetectedShared.Margin =
            new Padding(0, 10, 0, 0);

        _cmbDetectedShared.SelectedIndexChanged +=
            (_, _) =>
            {
                if (_cmbDetectedShared.SelectedItem
                    is string path)
                {
                    _txtShared.Text = path;
                }
            };

        layout.Controls.Add(
            _cmbDetectedShared,
            0,
            3);

        return panel;
    }

    private void DetectSharedFolders()
    {
        try
        {
            var oneDriveFolders = _oneDriveService.FindOneDriveFolders();

            if (oneDriveFolders.Count == 0)
            {
                MessageBox.Show(
                    this,
                    "No OneDrive installation could be found on this computer.",
                    "OneDrive not found",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            var folders =
                _oneDriveService.FindSharedValheimFolders();

            _cmbDetectedShared.Items.Clear();

            foreach (var folder in folders)
            {
                _cmbDetectedShared.Items.Add(folder);
            }

            if (folders.Count == 0)
            {
                MessageBox.Show(
                    this,
                    "OneDrive was found, but no shared Valheim folder was found.\n\n" +
                    "Make sure you:\n" +
                    "1. Opened your friend's OneDrive link.\n" +
                    "2. Chose 'Add shortcut to My files'.\n" +
                    "3. Waited until the folder appeared in File Explorer.\n\n" +
                    $"OneDrive found at:\n{string.Join("\n", oneDriveFolders)}",
                    "Shared folder not found",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            _cmbDetectedShared.SelectedIndex = 0;
            _txtShared.Text = folders[0];

            MessageBox.Show(
                this,
                $"Found {folders.Count} shared Valheim folder(s).\n\n" +
                $"Selected:\n{folders[0]}",
                "Shared folder found",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                $"Could not search OneDrive folders.\n\n{ex.Message}",
                "Search failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void SelectSetupMode(SetupMode mode)
    {
        _setupMode = mode;

        ShowFolderSetup(Result);
    }

    private Button CreateSetupChoiceButton(
    string title,
    string description)
    {
        var button = new Button
        {
            Text = $"{title}\r\n{description}",
            Height = 85,
            Dock = DockStyle.Top,
            TextAlign = ContentAlignment.MiddleLeft,

            Padding = new Padding(18, 8, 18, 8),

            BackColor = AppTheme.Surface,
            ForeColor = AppTheme.Text,

            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,

            Font = new Font(
                "Segoe UI Semibold",
                10F)
        };

        button.FlatAppearance.BorderSize = 1;

        return button;
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
