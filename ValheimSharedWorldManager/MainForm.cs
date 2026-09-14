using System.Diagnostics;
using System.Text.Json;
using ValheimSharedWorldManager.Controls;
using ValheimSharedWorldManager.Models;
using ValheimSharedWorldManager.Services;
using ValheimSharedWorldManager.Utilities;

namespace ValheimSharedWorldManager;

public sealed class MainForm : Form
{
    private readonly HostSessionService _hostSessionService = new();
    private readonly SettingsService _settingsService = new();
    private readonly WorldService _worldService = new();
    private readonly LockService _lockService = new();
    private readonly ValheimService _valheimService = new();
    private readonly LogService _log = new();

    private readonly ComboBox _cmbWorld = new();
    private readonly Label _lblLocalPath = new();
    private readonly Label _lblSharedPath = new();
    private readonly Label _lblBanner = new();
    private readonly Label _lblSubBanner = new();
    private readonly Label _lblSession = new();
    private readonly TextBox _txtLog = new();
    private readonly ProgressBar _progress = new();
    private readonly CheckBox _chkBackups = new();
    private readonly CheckBox _chkAutoLaunch = new();

    private readonly StatusCard _cardLocal = new();
    private readonly StatusCard _cardShared = new();
    private readonly StatusCard _cardLock = new();
    private readonly StatusCard _cardLastPublish = new();

    private readonly Button _btnHost = AppTheme.PrimaryButton("HOST WORLD");
    private readonly Button _btnJoin = AppTheme.PrimaryButton("JOIN WORLD");
    private readonly Button _btnInitialize = AppTheme.SecondaryButton("Upload local world");
    private readonly Button _btnPull = AppTheme.SecondaryButton("Download shared world");
    private readonly Button _btnRelease = AppTheme.SecondaryButton("Release host lock");
    private readonly Button _btnRefresh = AppTheme.SecondaryButton("Refresh");
    private readonly Button _btnSettings = AppTheme.SecondaryButton("Setup / folders");
    private readonly Button _btnAdvanced = AppTheme.SecondaryButton("Advanced");
    private readonly Button _btnForceUnlock = AppTheme.SecondaryButton("Force unlock");
    private readonly Button _btnHelp = AppTheme.SecondaryButton("Help");

    private readonly Panel _advancedPanel = new();
    private readonly Panel _historyPanel = new();
    private readonly TableLayoutPanel _historyList = new();

    private readonly Panel _sessionPanel = new();

    private readonly TextBox _txtJoinCode = new();
    private readonly TextBox _txtServerPassword = new();

    private readonly Label _lblSessionHost = new();
    private readonly Label _lblSessionWarning = new();

    private readonly Button _btnSaveSession =
        AppTheme.SecondaryButton("Save server info");

    private readonly Button _btnCopyJoinCode =
        AppTheme.SecondaryButton("Copy code");

    private AppSettings _settings = new();
    private HostLock? _activeLock;
    private string? _sharedFingerprintAtHostStart;
    private bool _busy;
    private bool _loadingSettings;
    private bool _waitingForRelease;

    public MainForm()
    {
        Text = "Valheim Shared World Manager";
        var assembly = typeof(MainForm).Assembly;

        using var stream = assembly.GetManifestResourceStream(
            "ValheimSharedWorldManager.app.ico");

        if (stream != null)
        {
            Icon = new Icon(stream);
        }

        Width = 1040;
        Height = 790;
        MinimumSize = new Size(940, 700);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10F);
        BackColor = AppTheme.Background;
        ForeColor = AppTheme.Text;

        BuildUi();
        _log.MessageLogged += AppendLog;

        Shown += (_, _) => LoadSettingsAndRefresh();
        FormClosing += MainForm_FormClosing;
    }

    private void BuildUi()
    {
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
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(22),
            ColumnCount = 1,
            RowCount = 9,
            BackColor = AppTheme.Background
        };

        for (var i = 0; i < 9; i++)
        {
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }

        scrollContainer.Controls.Add(root);

        // 0
        root.Controls.Add(BuildHeader(), 0, 0);

        // 1
        root.Controls.Add(BuildWorldSelector(), 0, 1);

        // 2
        root.Controls.Add(BuildStatusCards(), 0, 2);

        // 3
        BuildSessionPanel();
        root.Controls.Add(_sessionPanel, 0, 3);

        // 4
        BuildHistoryPanel();
        root.Controls.Add(_historyPanel, 0, 4);

        // 5
        root.Controls.Add(BuildMainActions(), 0, 5);

        // 6
        BuildAdvancedPanel();
        root.Controls.Add(_advancedPanel, 0, 6);

        // 7
        root.Controls.Add(BuildLogPanel(), 0, 7);

        // 8
        _progress.Dock = DockStyle.Top;
        _progress.Style = ProgressBarStyle.Marquee;
        _progress.MarqueeAnimationSpeed = 25;
        _progress.Height = 5;
        _progress.Visible = false;

        root.Controls.Add(_progress, 0, 8);
    }

    private Control BuildHeader()
    {
        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 3,
            Margin = new Padding(0, 0, 0, 18)
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var text = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, RowCount = 2, ColumnCount = 1 };
        _lblBanner.Text = "Valheim Shared World Manager";
        _lblBanner.AutoSize = true;
        _lblBanner.Font = new Font("Segoe UI Semibold", 22F);
        _lblBanner.ForeColor = AppTheme.Text;
        text.Controls.Add(_lblBanner, 0, 0);

        _lblSubBanner.Text = "Share a world through OneDrive and safely take turns hosting.";
        _lblSubBanner.AutoSize = true;
        _lblSubBanner.ForeColor = AppTheme.MutedText;
        _lblSubBanner.Margin = new Padding(1, 3, 0, 0);
        text.Controls.Add(_lblSubBanner, 0, 1);
        header.Controls.Add(text, 0, 0);

        _btnHelp.Width = 90;
        _btnHelp.Height = 36;
        _btnHelp.Margin = new Padding(8, 0, 0, 0);
        _btnHelp.Click += (_, _) => OpenHelp();

        header.Controls.Add(_btnHelp, 1, 0);

        _btnSettings.Width = 145;
        _btnSettings.Height = 36;
        _btnSettings.Click += (_, _) => OpenSetupWizard();
        header.Controls.Add(_btnSettings, 2, 0);
        return header;
    }

    private void OpenHelp()
    {
        using var dialog = new HelpForm();
        dialog.ShowDialog(this);
    }

    private Control BuildWorldSelector()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 92,
            BackColor = AppTheme.Surface,
            Padding = new Padding(16),
            Margin = new Padding(0, 0, 0, 14)
        };

        var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 2 };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        panel.Controls.Add(table);

        var worldLabel = AppTheme.Muted("WORLD TO SYNC");
        worldLabel.Font = new Font("Segoe UI Semibold", 8.5F);
        table.Controls.Add(worldLabel, 0, 0);

        _cmbWorld.Dock = DockStyle.Fill;
        _cmbWorld.DropDownStyle = ComboBoxStyle.DropDown;
        _cmbWorld.BackColor = AppTheme.SurfaceAlt;
        _cmbWorld.ForeColor = AppTheme.Text;
        _cmbWorld.FlatStyle = FlatStyle.Flat;
        _cmbWorld.Font = new Font("Segoe UI Semibold", 12F);
        _cmbWorld.Margin = new Padding(0, 8, 10, 0);
        _cmbWorld.SelectedIndexChanged += (_, _) => WorldChanged();
        _cmbWorld.TextChanged += (_, _) => WorldChanged();
        table.Controls.Add(_cmbWorld, 0, 1);

        _btnRefresh.Dock = DockStyle.Fill;
        _btnRefresh.Height = 38;
        _btnRefresh.Margin = new Padding(0, 8, 10, 0);
        _btnRefresh.Click += (_, _) => RefreshWorlds();
        table.Controls.Add(_btnRefresh, 1, 1);

        _chkAutoLaunch.Text = "Start Valheim automatically";
        _chkAutoLaunch.AutoSize = true;
        _chkAutoLaunch.ForeColor = AppTheme.Text;
        _chkAutoLaunch.TextAlign = ContentAlignment.MiddleRight;
        _chkAutoLaunch.Anchor = AnchorStyles.Right;
        _chkAutoLaunch.Margin = new Padding(0, 0, 0, 0);
        _chkAutoLaunch.CheckedChanged += (_, _) => SaveSettings();

        table.Controls.Add(_chkAutoLaunch, 2, 0);

        _btnHost.Dock = DockStyle.Fill;
        _btnHost.Height = 38;
        _btnHost.Margin = new Padding(0, 8, 0, 0);
        _btnHost.Click += async (_, _) =>
        {
            if (_waitingForRelease)
            {
                ReleaseCurrentLock();
                return;
            }

            await StartHostingAsync();
        };

        table.Controls.Add(_btnHost, 2, 1);

        _btnJoin.Dock = DockStyle.Fill;
        _btnJoin.Height = 38;
        _btnJoin.Margin = new Padding(0, 8, 0, 0);
        _btnJoin.Visible = false;

        _btnJoin.Click += (_, _) => JoinWorld();

        table.Controls.Add(_btnJoin, 2, 1);

        return panel;
    }

    private Control BuildStatusCards()
    {
        var cards = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 114,
            ColumnCount = 4,
            Margin = new Padding(0, 0, 0, 14)
        };
        for (var i = 0; i < 4; i++) cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

        cards.Controls.Add(_cardLocal, 0, 0);
        cards.Controls.Add(_cardShared, 1, 0);
        cards.Controls.Add(_cardLock, 2, 0);
        cards.Controls.Add(_cardLastPublish, 3, 0);

        _cardLastPublish.SetClickable(true);
        _cardLastPublish.CardClick += (_, _) => ToggleHistory();

        return cards;
    }
    private void BuildSessionPanel()
    {
        _sessionPanel.Dock = DockStyle.Top;
        _sessionPanel.AutoSize = true;
        _sessionPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _sessionPanel.BackColor = AppTheme.Surface;
        _sessionPanel.Padding = new Padding(14);
        _sessionPanel.Margin = new Padding(0, 0, 0, 12);
        _sessionPanel.Visible = false;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 4,
            RowCount = 4
        };

        layout.ColumnStyles.Add(
            new ColumnStyle(SizeType.Absolute, 120));

        layout.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100));

        layout.ColumnStyles.Add(
            new ColumnStyle(SizeType.AutoSize));

        layout.ColumnStyles.Add(
            new ColumnStyle(SizeType.AutoSize));

        var heading =
            AppTheme.Heading("Current server", 12F);

        layout.Controls.Add(heading, 0, 0);
        layout.SetColumnSpan(heading, 4);

        _lblSessionHost.AutoSize = true;
        _lblSessionHost.ForeColor = AppTheme.MutedText;
        _lblSessionHost.Margin = new Padding(0, 3, 0, 12);

        layout.Controls.Add(_lblSessionHost, 0, 1);
        layout.SetColumnSpan(_lblSessionHost, 4);

        // Join code
        layout.Controls.Add(
            AppTheme.Muted("Join code"),
            0,
            2);

        _txtJoinCode.Dock = DockStyle.Fill;
        _txtJoinCode.BackColor = AppTheme.SurfaceAlt;
        _txtJoinCode.ForeColor = AppTheme.Text;

        layout.Controls.Add(_txtJoinCode, 1, 2);

        _btnCopyJoinCode.AutoSize = true;
        _btnCopyJoinCode.Height = 30;

        _btnCopyJoinCode.Click += (_, _) =>
        {
            if (!string.IsNullOrWhiteSpace(
                    _txtJoinCode.Text))
            {
                Clipboard.SetText(
                    _txtJoinCode.Text.Trim());
            }
        };

        layout.Controls.Add(
            _btnCopyJoinCode,
            2,
            2);

        // Password
        layout.Controls.Add(
            AppTheme.Muted("Password"),
            0,
            3);

        _txtServerPassword.Dock = DockStyle.Fill;
        _txtServerPassword.BackColor = AppTheme.SurfaceAlt;
        _txtServerPassword.ForeColor = AppTheme.Text;

        layout.Controls.Add(
            _txtServerPassword,
            1,
            3);

        _btnSaveSession.AutoSize = true;
        _btnSaveSession.Height = 30;
        _btnSaveSession.Click += (_, _) =>
            SaveCurrentHostSession();

        layout.Controls.Add(
            _btnSaveSession,
            3,
            3);

        _lblSessionWarning.AutoSize = true;
        _lblSessionWarning.ForeColor = AppTheme.Warning;
        _lblSessionWarning.Margin =
            new Padding(10, 5, 0, 0);

        layout.Controls.Add(
            _lblSessionWarning,
            2,
            3);

        _sessionPanel.Controls.Add(layout);
    }
    private Control BuildMainActions()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            Margin = new Padding(0, 0, 0, 12)
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        _lblSession.Text = "Ready";
        _lblSession.AutoSize = true;
        _lblSession.ForeColor = AppTheme.MutedText;
        _lblSession.Font = new Font("Segoe UI Semibold", 10F);
        _lblSession.Anchor = AnchorStyles.Left;
        panel.Controls.Add(_lblSession, 0, 0);

        _btnAdvanced.Width = 105;
        _btnAdvanced.Height = 34;
        _btnAdvanced.Click += (_, _) =>
        {
            _advancedPanel.Visible = !_advancedPanel.Visible;
            _btnAdvanced.Text = _advancedPanel.Visible ? "Hide advanced" : "Advanced";
        };
        panel.Controls.Add(_btnAdvanced, 1, 0);
        return panel;
    }

    private void BuildAdvancedPanel()
    {
        _advancedPanel.Dock = DockStyle.Top;
        _advancedPanel.Height = 180;
        _advancedPanel.BackColor = AppTheme.Surface;
        _advancedPanel.Padding = new Padding(14);
        _advancedPanel.Margin = new Padding(0, 0, 0, 12);
        _advancedPanel.Visible = false;

        var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _advancedPanel.Controls.Add(table);

        var paths = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 3,
            RowCount = 2
        };

        paths.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105));
        paths.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 32));
        paths.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        // Local row
        var localLabel = AppTheme.Muted("Local folder");
        localLabel.Anchor = AnchorStyles.Left;
        localLabel.Margin = new Padding(0, 4, 0, 4);
        paths.Controls.Add(localLabel, 0, 0);

        var btnOpenLocalFolder = CreateFolderButton();
        btnOpenLocalFolder.Click += (_, _) => OpenFolder(_settings.LocalWorldRoot);
        paths.Controls.Add(btnOpenLocalFolder, 1, 0);

        _lblLocalPath.AutoSize = true;
        _lblLocalPath.ForeColor = AppTheme.Text;
        _lblLocalPath.Anchor = AnchorStyles.Left;
        _lblLocalPath.Margin = new Padding(8, 4, 0, 4);
        paths.Controls.Add(_lblLocalPath, 2, 0);

        // Shared row
        var sharedLabel = AppTheme.Muted("Shared folder");
        sharedLabel.Anchor = AnchorStyles.Left;
        sharedLabel.Margin = new Padding(0, 4, 0, 4);
        paths.Controls.Add(sharedLabel, 0, 1);

        var btnOpenSharedFolder = CreateFolderButton();
        btnOpenSharedFolder.Click += (_, _) => OpenFolder(_settings.SharedWorldRoot);
        paths.Controls.Add(btnOpenSharedFolder, 1, 1);

        _lblSharedPath.AutoSize = true;
        _lblSharedPath.ForeColor = AppTheme.Text;
        _lblSharedPath.Anchor = AnchorStyles.Left;
        _lblSharedPath.Margin = new Padding(8, 4, 0, 4);
        paths.Controls.Add(_lblSharedPath, 2, 1);

        table.Controls.Add(paths, 0, 0);

        var options = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Margin = new Padding(0, 10, 0, 4) };
        _chkBackups.Text = "Create backups";
        _chkBackups.AutoSize = true;
        _chkBackups.ForeColor = AppTheme.Text;
        _chkBackups.CheckedChanged += (_, _) => SaveSettings();
        options.Controls.Add(_chkBackups);

       
        table.Controls.Add(options, 0, 1);

        var actions = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, Margin = new Padding(0, 5, 0, 0) };
        foreach (var b in new[] { _btnInitialize, _btnPull, _btnForceUnlock, _btnRelease })
        {
            b.Height = 32;
            b.AutoSize = true;
            actions.Controls.Add(b);
        }
        _btnInitialize.Click += async (_, _) => await InitializeSharedAsync();
        _btnPull.Click += async (_, _) => await PullSharedToLocalAsync();
        _btnForceUnlock.Click += (_, _) => ForceUnlock();
        _btnRelease.Click += (_, _) => ReleaseCurrentLock();
        table.Controls.Add(actions, 0, 2);

    }

    private Button CreateFolderButton()
    {
        var button = new Button
        {
            Text = "📁",
            Width = 26,
            Height = 24,
            FlatStyle = FlatStyle.Flat,
            BackColor = AppTheme.Surface,
            ForeColor = AppTheme.Text,
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 1, 0, 1),
            TabStop = false
        };

        button.FlatAppearance.BorderSize = 0;

        return button;
    }

    private void BuildHistoryPanel()
    {
        _historyPanel.Dock = DockStyle.Top;
        _historyPanel.AutoSize = true;
        _historyPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _historyPanel.BackColor = AppTheme.Surface;
        _historyPanel.Padding = new Padding(14);
        _historyPanel.Margin = new Padding(0, 0, 0, 12);
        _historyPanel.Visible = false;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 2
        };

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2
        };

        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        header.Margin = new Padding(0, 0, 0, 4);

        var title = AppTheme.Heading("Recent backups", 12F);
        title.Margin = new Padding(0);
        header.Controls.Add(title, 0, 0);

        var close = AppTheme.SecondaryButton("Close");
        close.AutoSize = true;
        close.Height = 30;
        close.Click += (_, _) => _historyPanel.Visible = false;
        header.Controls.Add(close, 1, 0);

        layout.Controls.Add(header, 0, 0);

        _historyList.Dock = DockStyle.Top;
        _historyList.AutoSize = true;
        _historyList.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _historyList.ColumnCount = 1;
        _historyList.Margin = new Padding(0, 10, 0, 0);
        _historyList.Padding = new Padding(0);

        layout.Controls.Add(_historyList, 0, 1);

        _historyPanel.Controls.Add(layout);
    }

    private Control BuildLogPanel()
    {
        var group = new GroupBox
        {
            Text = "Activity",
            Dock = DockStyle.Top,
            Height = 220,
            ForeColor = AppTheme.MutedText,
            BackColor = AppTheme.Background,
            Padding = new Padding(10),
            Margin = new Padding(0)
        };

        _txtLog.Dock = DockStyle.Fill;
        _txtLog.Multiline = true;
        _txtLog.ReadOnly = true;
        _txtLog.ScrollBars = ScrollBars.Vertical;
        _txtLog.BackColor = Color.FromArgb(14, 17, 21);
        _txtLog.ForeColor = Color.FromArgb(195, 202, 214);
        _txtLog.BorderStyle = BorderStyle.FixedSingle;
        _txtLog.Font = new Font("Consolas", 9F);
        group.Controls.Add(_txtLog);
        return group;
    }

    private void LoadSettingsAndRefresh()
    {
        _loadingSettings = true;

        try
        {
            _settings = _settingsService.Load();

            if (string.IsNullOrWhiteSpace(_settings.LocalWorldRoot))
                _settings.LocalWorldRoot = AppPaths.GetDefaultLocalWorldRoot();

            if (string.IsNullOrWhiteSpace(_settings.SharedWorldRoot))
                _settings.SharedWorldRoot = AppPaths.GetSuggestedSharedWorldRoot();

            if (!_settings.SetupCompleted || string.IsNullOrWhiteSpace(_settings.SharedWorldRoot))
            {
                OpenSetupWizard(firstRun: true);
            }

            TryRestoreOwnedLock();

            _chkBackups.Checked = _settings.CreateBackups;
            _chkAutoLaunch.Checked = _settings.LaunchValheimAutomatically;

            UpdatePathLabels();
            RefreshWorlds();

            if (!string.IsNullOrWhiteSpace(_settings.SelectedWorld))
                _cmbWorld.Text = _settings.SelectedWorld;

            RefreshStatusCards();
            _log.Info("Application started.");
        }
        finally
        {
            _loadingSettings = false;
        }
    }

    private void TryRestoreOwnedLock()
    {
        if (string.IsNullOrWhiteSpace(_settings.ActiveHostWorld) ||
            string.IsNullOrWhiteSpace(_settings.ActiveHostToken))
        {
            return;
        }

        var hostLock = _lockService.ReadLock(
            _settings.SharedWorldRoot,
            _settings.ActiveHostWorld);

        // Lockfilen finns inte längre.
        if (hostLock == null)
        {
            ClearSavedHostState();
            return;
        }

        // Lockfilen finns, men token matchar inte.
        // Då ägs locken av någon annan session.
        if (!string.Equals(
                hostLock.Token,
                _settings.ActiveHostToken,
                StringComparison.Ordinal))
        {
            ClearSavedHostState();
            return;
        }

        // Samma token = detta är vårt gamla lock.
        _activeLock = hostLock;
        _waitingForRelease = _settings.WaitingForRelease;

        _log.Info(
            $"Restored owned host lock for '{hostLock.World}'. " +
            $"WaitingForRelease={_waitingForRelease}");
    }

    private void ClearSavedHostState()
    {
        _settings.ActiveHostWorld = null;
        _settings.ActiveHostToken = null;
        _settings.WaitingForRelease = false;

        _waitingForRelease = false;

        _settingsService.Save(_settings);
    }

    private void ToggleHistory()
    {
        _historyPanel.Visible = !_historyPanel.Visible;

        if (_historyPanel.Visible)
            RefreshHistory();
    }
    private void RefreshHistory()
    {
        _historyList.Controls.Clear();
        _historyList.RowStyles.Clear();
        _historyList.RowCount = 0;

        var world = _cmbWorld.Text.Trim();

        if (string.IsNullOrWhiteSpace(world))
        {
            _historyList.Controls.Add(
                AppTheme.Muted("Select a world first."));
            return;
        }

        var backupRoot = GetBackupRoot(world);

        if (!Directory.Exists(backupRoot))
        {
            _historyList.Controls.Add(
                AppTheme.Muted("No backups found for this world."));
            return;
        }

        var backups = Directory
            .GetDirectories(backupRoot)
            .OrderByDescending(GetBackupDate)
            .Take(8)
            .ToList();

        if (backups.Count == 0)
        {
            _historyList.Controls.Add(
                AppTheme.Muted("No backups found for this world."));
            return;
        }

        foreach (var backup in backups)
        {
            var rowIndex = _historyList.RowCount++;

            _historyList.RowStyles.Add(
                new RowStyle(SizeType.AutoSize));

            _historyList.Controls.Add(
                BuildHistoryRow(world, backup),
                0,
                rowIndex);
        }
    }
    private Control BuildHistoryRow(string world, string backupPath)
    {
        var folderName = Path.GetFileName(backupPath);
        var metadata = ReadBackupMetadata(backupPath);

        var row = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Height = 54,
            ColumnCount = 3,
            Padding = new Padding(14, 7, 14, 7),
            Margin = new Padding(0, 0, 0, 6),
            BackColor = AppTheme.SurfaceAlt
        };

        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var dateText = metadata != null
            ? metadata.CreatedUtc.ToLocalTime().ToString("dd MMM HH:mm")
            : GetBackupDateText(folderName);

        var typeText = metadata != null
            ? GetBackupTypeText(metadata.Type)
            : GetBackupTypeText(folderName);

        var date = new Label
        {
            Text = dateText,
            AutoSize = true,
            ForeColor = AppTheme.Text,
            Font = new Font("Segoe UI Semibold", 9.5F),
            Anchor = AnchorStyles.Left
        };

        row.Controls.Add(date, 0, 0);

        var info = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            RowCount = 2,
            ColumnCount = 1,
            Margin = new Padding(0)
        };

        var type = new Label
        {
            Text = typeText,
            AutoSize = true,
            ForeColor = AppTheme.MutedText,
            Margin = new Padding(0)
        };

        info.Controls.Add(type, 0, 0);

        var host = new Label
        {
            Text = metadata != null
                ? $"{metadata.Machine} / {metadata.User}"
                : "Legacy backup",
            AutoSize = true,
            ForeColor = AppTheme.MutedText,
            Font = new Font("Segoe UI", 8F),
            Margin = new Padding(0, 2, 0, 0)
        };

        info.Controls.Add(host, 0, 1);

        row.Controls.Add(info, 1, 0);

        var restore = AppTheme.SecondaryButton("Restore");
        restore.Width = 76;
        restore.Height = 30;
        restore.Anchor = AnchorStyles.Right;
        restore.Click += async (_, _) =>
            await RestoreBackupAsync(world, backupPath);

        row.Controls.Add(restore, 2, 0);

        return row;
    }

    private static string GetBackupDateText(string folderName)
    {
        if (folderName.Length >= 19)
        {
            var datePart = folderName[..19];

            if (DateTime.TryParseExact(
                datePart,
                "yyyy-MM-dd_HH-mm-ss",
                null,
                System.Globalization.DateTimeStyles.None,
                out var date))
            {
                return date.ToString("dd MMM HH:mm");
            }
        }

        return folderName;
    }
    private static string GetBackupTypeText(string value)
    {
        if (value.Contains(
            "before_hosting",
            StringComparison.OrdinalIgnoreCase))
        {
            return "Before hosting";
        }

        if (value.Contains(
            "after_hosting",
            StringComparison.OrdinalIgnoreCase))
        {
            return "After hosting";
        }

        if (value.Contains(
            "before_initialize",
            StringComparison.OrdinalIgnoreCase))
        {
            return "Before upload";
        }

        if (value.Contains(
            "local_before_pull",
            StringComparison.OrdinalIgnoreCase))
        {
            return "Before download";
        }

        if (value.Contains(
            "before_restore",
            StringComparison.OrdinalIgnoreCase))
        {
            return "Before restore";
        }

        return "Backup";
    }

    private string? GetLatestHostingBackup(string world)
    {
        var backupRoot = GetBackupRoot(world);

        if (!Directory.Exists(backupRoot))
            return null;

        return Directory
            .GetDirectories(backupRoot)
            .Where(x =>
            {
                var metadata = ReadBackupMetadata(x);

                if (metadata != null)
                {
                    return string.Equals(
                        metadata.Type,
                        "after_hosting",
                        StringComparison.OrdinalIgnoreCase);
                }

                return Path.GetFileName(x)
                    .Contains(
                        "after_hosting",
                        StringComparison.OrdinalIgnoreCase);
            })
            .OrderByDescending(GetBackupDate)
            .FirstOrDefault();
    }
    private DateTime GetBackupDate(string backupPath)
    {
        var metadata = ReadBackupMetadata(backupPath);

        if (metadata != null)
            return metadata.CreatedUtc;

        var folderName = Path.GetFileName(backupPath);

        if (folderName.Length >= 19)
        {
            var datePart = folderName[..19];

            if (DateTime.TryParseExact(
                datePart,
                "yyyy-MM-dd_HH-mm-ss",
                null,
                System.Globalization.DateTimeStyles.None,
                out var date))
            {
                return date.ToUniversalTime();
            }
        }

        // Sista fallback om namnet inte går att tolka
        return Directory.GetLastWriteTimeUtc(backupPath);
    }
    private async Task RestoreBackupAsync(
    string world,
    string backupPath)
    {
        if (_busy)
            return;

        if (_activeLock != null)
        {
            MessageBox.Show(
                this,
                "You cannot restore a backup while this app owns a host session.",
                "Hosting active",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        var existingLock =
            _lockService.ReadLock(_settings.SharedWorldRoot, world);

        if (existingLock != null)
        {
            MessageBox.Show(
                this,
                $"'{world}' is currently locked by:\n\n" +
                $"{existingLock.Machine} / {existingLock.User}\n\n" +
                "Do not restore while somebody is hosting.",
                "World is in use",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        var backupName = Path.GetFileName(backupPath);

        var result = MessageBox.Show(
            this,
            $"Restore this backup?\n\n" +
            $"{GetBackupDateText(backupName)}\n" +
            $"{GetBackupTypeText(backupName)}\n\n" +
            "This will replace your LOCAL Valheim world.\n\n" +
            "The shared OneDrive world will NOT be changed.",
            "Restore backup",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);

        if (result != DialogResult.Yes)
            return;

        var local = Path.Combine(
            _settings.LocalWorldRoot,
            world);

        await RunBusyAsync(
            $"Restoring backup for {world}...",
            async () =>
            {
                // Safety backup of current local world first.
                if (_worldService.IsValidWorldFolder(local))
                {
                    CreateBackup(
                        local,
                        world,
                        "before_restore");
                }

                await Task.Run(() =>
                    _worldService.MirrorDirectory(
                        backupPath,
                        local));

                _log.Info(
                    $"Restored backup '{backupName}' to local world '{world}'.");
            });

        MessageBox.Show(
            this,
            $"Backup restored successfully.\n\n" +
            $"'{world}' has been restored locally.\n\n" +
            "The shared OneDrive copy has not been changed.",
            "Backup restored",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);

        RefreshStatusCards();
        RefreshHistory();
    }
    
    private string GetBackupRoot(string world)
    {
        var sharedParent = Directory.GetParent(_settings.SharedWorldRoot)?.FullName;

        if (string.IsNullOrWhiteSpace(sharedParent))
            throw new InvalidOperationException("Could not determine shared Valheim folder.");

        return Path.Combine(sharedParent, "Backups", world);
    }

    private void OpenSetupWizard(bool firstRun = false)
    {
        using var dialog = new SetupWizardForm(_settings);
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _settings = dialog.Result;
            _settingsService.Save(_settings);
            UpdatePathLabels();
            RefreshWorlds();
            RefreshStatusCards();
            _log.Info("Folder setup updated.");
        }
        else if (firstRun)
        {
            _log.Info("First-run setup was skipped. Use 'Setup / folders' when ready.");
        }
    }

    private void UpdatePathLabels()
    {
        _lblLocalPath.Text = _settings.LocalWorldRoot;
        _lblSharedPath.Text = _settings.SharedWorldRoot;
    }

    private void WorldChanged()
    {
        SaveSettings();
        RefreshStatusCards();
    }

    private void RefreshWorlds()
    {
        try
        {
            var current = _cmbWorld.Text;
            var worlds = _worldService.DiscoverWorlds(_settings.LocalWorldRoot)
                .Concat(_worldService.DiscoverWorlds(_settings.SharedWorldRoot))
                .Distinct(StringComparer.CurrentCultureIgnoreCase)
                .OrderBy(x => x, StringComparer.CurrentCultureIgnoreCase)
                .ToArray();

            _cmbWorld.BeginUpdate();
            _cmbWorld.Items.Clear();
            _cmbWorld.Items.AddRange(worlds.Cast<object>().ToArray());
            _cmbWorld.EndUpdate();

            if (!string.IsNullOrWhiteSpace(current)) _cmbWorld.Text = current;
            else if (!string.IsNullOrWhiteSpace(_settings.SelectedWorld)) _cmbWorld.Text = _settings.SelectedWorld;
            else if (worlds.Length > 0) _cmbWorld.SelectedIndex = 0;

            RefreshStatusCards();
        }
        catch (Exception ex)
        {
            ShowError("Could not scan world folders.", ex);
        }
    }

    private void RefreshStatusCards()
    {
        var world = _cmbWorld.Text.Trim();
        if (string.IsNullOrWhiteSpace(world))
        {
            _cardLocal.Set("LOCAL WORLD", "No world selected", "Choose or type a world name", AppTheme.Warning);
            _cardShared.Set("SHARED COPY", "No world selected", "", AppTheme.Warning);
            _cardLock.Set("HOST LOCK", "-", "", AppTheme.MutedText);
            _cardLastPublish.Set("LAST PUBLISH", "-", "", AppTheme.MutedText);
            _btnHost.Enabled = !_busy;
            return;
        }

        if (string.IsNullOrWhiteSpace(_settings.LocalWorldRoot) || string.IsNullOrWhiteSpace(_settings.SharedWorldRoot))
        {
            _cardLocal.Set("LOCAL WORLD", "Setup required", "Choose folders first", AppTheme.Warning);
            _cardShared.Set("SHARED COPY", "Setup required", "Choose folders first", AppTheme.Warning);
            _cardLock.Set("HOST LOCK", "-", "", AppTheme.MutedText);
            _cardLastPublish.Set("LAST PUBLISH", "-", "", AppTheme.MutedText);
            _btnHost.Enabled = false;
            return;
        }

        var local = Path.Combine(_settings.LocalWorldRoot, world);
        var shared = Path.Combine(_settings.SharedWorldRoot, world);
        var hasLocal = _worldService.IsValidWorldFolder(local);
        var hasShared = _worldService.IsValidWorldFolder(shared);
        _cardLocal.Set("LOCAL WORLD", hasLocal ? "Found" : "Missing", hasLocal ? world : "Not in local Valheim folder", hasLocal ? AppTheme.Success : AppTheme.Warning);
        _cardShared.Set("SHARED COPY", hasShared ? "Found" : "Missing", hasShared ? "Ready in shared folder" : "Upload local world first", hasShared ? AppTheme.Success : AppTheme.Warning);

        HostLock? hostLock = null;

        try
        {
            hostLock = _lockService.ReadLock(
                _settings.SharedWorldRoot,
                world);
        }
        catch
        {
        }

        var isOurLock =
            hostLock != null &&
            _activeLock != null &&
            string.Equals(
                hostLock.Token,
                _activeLock.Token,
                StringComparison.Ordinal);

        var isOtherHost =
            hostLock != null &&
            !isOurLock;

        if (hostLock == null)
        {
            _cardLock.Set(
                "HOST LOCK",
                "Free",
                "Nobody is marked as hosting",
                AppTheme.Success);

            _btnHost.Visible = true;
            _btnHost.Text = "HOST WORLD";
            _btnHost.Enabled = !_busy;

            _btnJoin.Visible = false;

            _chkAutoLaunch.Visible = true;
        }
        else if (isOurLock)
        {
            _cardLock.Set(
               "HOST LOCK",
               _waitingForRelease
                   ? "Ready to release"
                   : "You are hosting",
               $"{hostLock.Machine} / {hostLock.User}",
               AppTheme.Accent);

            _btnHost.Visible = true;
            _btnJoin.Visible = false;
            _chkAutoLaunch.Visible = false;

            if (_waitingForRelease)
            {
                _btnHost.Text = "RELEASE HOST LOCK";
                _btnHost.Enabled = true;
            }
            else
            {
                _btnHost.Text = "HOSTING...";
                _btnHost.Enabled = false;
            }
        }
        else
        {
            _cardLock.Set(
                "HOST LOCK",
                "World is live",
                $"{hostLock.Machine} / {hostLock.User}",
                AppTheme.Danger);

            _btnHost.Visible = false;

            _btnJoin.Visible = true;
            _btnJoin.Enabled = !_busy;

            _chkAutoLaunch.Visible = false;
        }

        var latestBackup = GetLatestHostingBackup(world);

        if (latestBackup == null)
        {
            _cardLastPublish.Set(
                "LAST PUBLISH",
                "No history",
                "No hosting backups found",
                AppTheme.MutedText);
        }
        else
        {
            var metadata = ReadBackupMetadata(latestBackup);

            if (metadata != null)
            {
                _cardLastPublish.Set(
                    "LAST PUBLISH",
                    metadata.CreatedUtc.ToLocalTime().ToString("g"),
                    $"{metadata.Machine} / {metadata.User}",
                    AppTheme.Accent);
            }
            else
            {
                var folderName = Path.GetFileName(latestBackup);

                _cardLastPublish.Set(
                    "LAST PUBLISH",
                    GetBackupDateText(folderName),
                    "Legacy backup",
                    AppTheme.Accent);
            }
        }

        RefreshSessionPanel();
    }

    private async Task InitializeSharedAsync()
    {
        if (!ValidateInputs(out var world)) return;
        var local = Path.Combine(_settings.LocalWorldRoot, world);
        var shared = Path.Combine(_settings.SharedWorldRoot, world);

        if (!_worldService.IsValidWorldFolder(local))
        {
            MessageBox.Show(this, $"The local world was not found:\n\n{local}", "Local world missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_worldService.IsValidWorldFolder(shared))
        {
            var overwrite = MessageBox.Show(this, "A shared copy already exists. Replace it with your local copy?\n\nA backup is created first if backups are enabled.", "Replace shared copy?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (overwrite != DialogResult.Yes) return;
        }

        await RunBusyAsync("Uploading local world to shared folder...", async () =>
        {
            if (_chkBackups.Checked && _worldService.IsValidWorldFolder(shared)) CreateBackup(shared, world, "before_initialize");
            await Task.Run(() => _worldService.MirrorDirectory(local, shared));
            _log.Info($"Shared copy initialized for '{world}'.");
        });

        MessageBox.Show(this, "Shared copy created. Wait until OneDrive is fully synced before another person hosts.", "Shared world ready", MessageBoxButtons.OK, MessageBoxIcon.Information);
        RefreshStatusCards();
    }

    private async Task PullSharedToLocalAsync()
    {
        if (!ValidateInputs(out var world)) return;
        var shared = Path.Combine(_settings.SharedWorldRoot, world);
        var local = Path.Combine(_settings.LocalWorldRoot, world);

        if (!_worldService.IsValidWorldFolder(shared))
        {
            MessageBox.Show(this, "The shared copy does not exist yet.", "Shared world missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var hostLock = _lockService.ReadLock(_settings.SharedWorldRoot, world);
        if (hostLock != null)
        {
            MessageBox.Show(this, $"This world is currently locked by {hostLock.Machine} / {hostLock.User}.\n\nDo not download while another person is hosting.", "World is locked", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        await RunBusyAsync("Downloading shared world to Valheim...", async () =>
        {
            if (_chkBackups.Checked && _worldService.IsValidWorldFolder(local)) CreateBackup(local, world, "local_before_pull");
            await Task.Run(() => _worldService.MirrorDirectory(shared, local));
            _log.Info($"Downloaded '{world}' to local Valheim folder.");
        });

        RefreshStatusCards();
    }

    private void JoinWorld()
    {
        var world = _cmbWorld.Text.Trim();

        if (string.IsNullOrWhiteSpace(world))
            return;

        var session = _hostSessionService.Read(
            _settings.SharedWorldRoot,
            world);

        if (session != null &&
            !string.IsNullOrWhiteSpace(session.JoinCode))
        {
            try
            {
                Clipboard.SetText(session.JoinCode);

                _log.Info(
                    $"Copied join code for '{world}' to clipboard.");
            }
            catch
            {
                // Clipboard failure should not stop Valheim.
            }
        }

        _valheimService.Start();

        if (session != null &&
            !string.IsNullOrWhiteSpace(session.JoinCode))
        {
            MessageBox.Show(
                this,
                $"Valheim is starting.\n\n" +
                $"Join code: {session.JoinCode}\n" +
                $"Password: {session.Password}\n\n" +
                "The join code has been copied to your clipboard.",
                $"Join {world}",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }

    private async Task StartHostingAsync()
    {
        if (!ValidateInputs(out var world)) return;
        if (_valheimService.IsRunning())
        {
            MessageBox.Show(this, "Valheim is already running. Close it before starting a host session from the manager.", "Valheim is running", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var sharedRoot = _settings.SharedWorldRoot;
        var localRoot = _settings.LocalWorldRoot;
        var shared = Path.Combine(sharedRoot, world);
        var local = Path.Combine(localRoot, world);

        if (!_worldService.IsValidWorldFolder(shared))
        {
            if (_worldService.IsValidWorldFolder(local))
            {
                var initialize = MessageBox.Show(this, "This world exists locally but has no shared copy yet. Upload your local world now?", "Create shared copy", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (initialize != DialogResult.Yes) return;
                await InitializeSharedAsync();
                if (!_worldService.IsValidWorldFolder(shared)) return;
            }
            else
            {
                MessageBox.Show(this, "The selected world was not found locally or in the shared folder.", "World not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        var existingLock = _lockService.ReadLock(sharedRoot, world);
        if (existingLock != null)
        {
            MessageBox.Show(this, $"'{world}' is locked.\n\nMachine: {existingLock.Machine}\nUser: {existingLock.User}\nStarted: {existingLock.StartedUtc.ToLocalTime():g}\n\nDo not host until that session is finished.", "World already in use", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            RefreshStatusCards();
            return;
        }

        SaveSettings();
        // vi börjar en helt ny host-session
        _waitingForRelease = false;
        _settings.WaitingForRelease = false;

        SetBusy(true, "Preparing host session...");


        try
        {
            _activeLock = _lockService.CreateLock(sharedRoot, world);

            _settings.ActiveHostWorld = world;
            _settings.ActiveHostToken = _activeLock.Token;
            _settings.WaitingForRelease = false;

            _settingsService.Save(_settings);

            _log.Info($"Host lock created for '{world}' as {_activeLock.Machine}/{_activeLock.User}.");
            RefreshStatusCards();

            // efter RefreshStatusCards, så den inte skriver över vårt knapp-state
            _btnHost.Visible = true;
            _btnHost.Text = "HOSTING...";
            _btnHost.Enabled = false;

            await Task.Delay(2500);
            if (!_lockService.IsOurLock(sharedRoot, world, _activeLock.Token))
                throw new InvalidOperationException("The host lock changed immediately after creation. Another machine may also be trying to host.");

            _sharedFingerprintAtHostStart = await Task.Run(() => _worldService.BuildFingerprint(shared));
            if (_chkBackups.Checked) CreateBackup(shared, world, "before_hosting");

            SetSession($"Preparing {world}...");
            await Task.Run(() => _worldService.MirrorDirectory(shared, local));
            _log.Info($"Latest shared '{world}' copied to local Valheim folder.");

            if (_chkAutoLaunch.Checked)
            {
                SetSession(
                    $"Valheim is starting - select '{world}', enable Start Server and Crossplay.");

                _valheimService.Start();
                _log.Info("Started Valheim through Steam.");

                await _valheimService.WaitForStartAsync(
                    TimeSpan.FromMinutes(2),
                    CancellationToken.None);
            }
            else
            {
                SetSession(
                    $"Start Valheim manually and host '{world}'. Waiting for Valheim to start...");

                await _valheimService.WaitForStartAsync(
                    TimeSpan.FromMinutes(5),
                    CancellationToken.None);
            }

            SetSession($"HOSTING: {world} - waiting for Valheim to close...");
            _log.Info("Valheim detected. Waiting for it to close..");
            await _valheimService.WaitForExitAsync(CancellationToken.None);
            await Task.Delay(4000);

            if (!_worldService.IsValidWorldFolder(local))
                throw new InvalidOperationException("The local world is missing or invalid after Valheim closed. The shared copy was not overwritten.");
            if (!_lockService.IsOurLock(sharedRoot, world, _activeLock.Token))
                throw new InvalidOperationException("The host lock no longer belongs to this session. The shared world was not overwritten.");

            var currentFingerprint = await Task.Run(() => _worldService.BuildFingerprint(shared));
            if (!string.Equals(currentFingerprint, _sharedFingerprintAtHostStart, StringComparison.Ordinal))
                throw new InvalidOperationException("The shared world changed while you were hosting. Publishing was stopped to prevent overwriting someone else's changes. Check OneDrive conflicts and backups.");

            if (_chkBackups.Checked) CreateBackup(local, world, "after_hosting");
            SetSession("Publishing updated world to OneDrive...");
            await Task.Run(() => _worldService.MirrorDirectory(local, shared));
            _log.Info($"Published updated '{world}' to shared storage.");

            // HÄR börjar vänteläget efter publish
            SetBusy(false, "Waiting for OneDrive sync confirmation");

            _waitingForRelease = true;
            _settings.WaitingForRelease = true;
            _settingsService.Save(_settings);

            _btnHost.Visible = true;
            _btnHost.Enabled = true;
            _btnHost.Text = "RELEASE HOST LOCK";

            _btnInitialize.Enabled = false;
            _btnPull.Enabled = false;
            _btnForceUnlock.Enabled = false;

            SetSession(
                "WORLD PUBLISHED - wait for OneDrive to say 'Up to date', then release the lock.");

        }
        catch (Exception ex)
        {
            SetBusy(false, "Error - host lock kept for safety");
            SetSession("ERROR - host lock kept for safety. Check the activity log.");
            ShowError("Hosting session failed. The host lock was intentionally kept when possible.", ex);
          
            RefreshStatusCards();
        }
    }

    private void ReleaseCurrentLock()
    {
        if (_activeLock == null)
            return;

        try
        {
            _lockService.ReleaseLock(
                _settings.SharedWorldRoot,
                _activeLock.World,
                _activeLock.Token);

            _log.Info($"Host lock released for '{_activeLock.World}'.");

            _activeLock = null;
            _sharedFingerprintAtHostStart = null;
            _waitingForRelease = false;

            // Rensa sparad host-state
            _settings.ActiveHostWorld = null;
            _settings.ActiveHostToken = null;
            _settings.WaitingForRelease = false;
            _settingsService.Save(_settings);

            _btnHost.Text = "HOST WORLD";
            _btnHost.Enabled = true;
            _btnHost.Visible = true;

            EnableNormalActions();

            SetSession(
                "Ready - the next person can host after their OneDrive has received the files.");

            RefreshStatusCards();
        }
        catch (Exception ex)
        {
            ShowError("Could not release the host lock.", ex);
        }
    }

    private void ForceUnlock()
    {
        if (!ValidateInputs(out var world)) return;
        var existing = _lockService.ReadLock(_settings.SharedWorldRoot, world);
        if (existing == null)
        {
            MessageBox.Show(this, "This world is not locked.", "No lock", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var result = MessageBox.Show(this, $"FORCE UNLOCK '{world}'?\n\nCurrent lock: {existing.Machine} / {existing.User}\nStarted: {existing.StartedUtc.ToLocalTime():g}\n\nOnly continue if you are certain nobody is hosting and OneDrive is fully synced.", "Force unlock", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
        if (result != DialogResult.Yes) return;

        try
        {
            _lockService.ReleaseLock(_settings.SharedWorldRoot, world, existing.Token, force: true);

            _activeLock = null;
            _sharedFingerprintAtHostStart = null;
            _waitingForRelease = false;

            _settings.ActiveHostWorld = null;
            _settings.ActiveHostToken = null;
            _settings.WaitingForRelease = false;
            _settingsService.Save(_settings);

            _btnRelease.Enabled = false;

            EnableNormalActions();

            _log.Info($"Force-unlocked '{world}'.");
            RefreshStatusCards();
        }
        catch (Exception ex)
        {
            ShowError("Could not force-unlock the world.", ex);
        }
    }

    private void CreateBackup(string sourceFolder, string world, string label)
    {
        var now = DateTime.UtcNow;
        var timestamp = now.ToLocalTime().ToString("yyyy-MM-dd_HH-mm-ss");

        var backup = Path.Combine(
            GetBackupRoot(world),
            $"{timestamp}_{label}");

        _worldService.CopyDirectory(sourceFolder, backup);

        var metadata = new BackupMetadata
        {
            World = world,
            CreatedUtc = now,
            Type = label,
            Machine = Environment.MachineName,
            User = Environment.UserName
        };

        var metadataPath = Path.Combine(backup, "backup.json");

        File.WriteAllText(
            metadataPath,
            JsonSerializer.Serialize(
                metadata,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                }));

        _log.Info($"Backup created: {backup}");
    }

    private BackupMetadata? ReadBackupMetadata(string backupPath)
    {
        try
        {
            var metadataPath = Path.Combine(backupPath, "backup.json");

            if (!File.Exists(metadataPath))
                return null;

            var json = File.ReadAllText(metadataPath);

            return JsonSerializer.Deserialize<BackupMetadata>(json);
        }
        catch (Exception ex)
        {
            _log.Error($"Could not read backup metadata from '{backupPath}': {ex.Message}");
            return null;
        }
    }


    private bool ValidateInputs(out string world)
    {
        world = _cmbWorld.Text.Trim();
        if (string.IsNullOrWhiteSpace(_settings.LocalWorldRoot) || string.IsNullOrWhiteSpace(_settings.SharedWorldRoot))
        {
            MessageBox.Show(this, "Set up both folders first.", "Setup required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            OpenSetupWizard();
            return false;
        }
        if (string.IsNullOrWhiteSpace(world))
        {
            MessageBox.Show(this, "Select or type a world name first.", "Choose world", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        try
        {
            Directory.CreateDirectory(_settings.LocalWorldRoot);
            Directory.CreateDirectory(_settings.SharedWorldRoot);
        }
        catch (Exception ex)
        {
            ShowError("One of the configured folders cannot be accessed.", ex);
            return false;
        }

        SaveSettings();
        return true;
    }

    private async Task RunBusyAsync(string status, Func<Task> action)
    {
        SetBusy(true, status);
        try
        {
            await action();
            SetBusy(false, "Ready");
        }
        catch (Exception ex)
        {
            SetBusy(false, "Error");
            ShowError("Operation failed.", ex);
        }
    }

    private void SetBusy(bool busy, string status)
    {
        _busy = busy;
        _progress.Visible = busy;
        _cmbWorld.Enabled = !busy;
        _btnRefresh.Enabled = !busy;
        _btnSettings.Enabled = !busy;
        _btnHost.Enabled = !busy && _activeLock == null;
        _btnInitialize.Enabled = !busy && _activeLock == null;
        _btnPull.Enabled = !busy && _activeLock == null;
        _btnForceUnlock.Enabled = !busy && _activeLock == null;
        _chkBackups.Enabled = !busy;
        _chkAutoLaunch.Enabled = !busy;
        SetSession(status);
    }

    private void EnableNormalActions()
    {
        _btnHost.Enabled = true;
        _btnInitialize.Enabled = true;
        _btnPull.Enabled = true;
        _btnForceUnlock.Enabled = true;
        _btnRefresh.Enabled = true;
        _btnSettings.Enabled = true;
        _cmbWorld.Enabled = true;
        _chkBackups.Enabled = true;
        _chkAutoLaunch.Enabled = true;
    }

    private void SetSession(string text)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => SetSession(text));
            return;
        }
        _lblSession.Text = text;
        _lblSession.ForeColor = text.Contains("ERROR", StringComparison.OrdinalIgnoreCase) ? AppTheme.Danger : AppTheme.MutedText;
    }

    private void SaveSettings()
    {
        if (_loadingSettings)
            return;

        try
        {
            _settings.SelectedWorld = _cmbWorld.Text.Trim();
            _settings.CreateBackups = _chkBackups.Checked;
            _settings.LaunchValheimAutomatically = _chkAutoLaunch.Checked;
            _settingsService.Save(_settings);
        }
        catch (Exception ex)
        {
            _log.Error("Could not save settings: " + ex.Message);
        }
    }
    private void SaveCurrentHostSession()
    {
        var world = _cmbWorld.Text.Trim();

        if (_activeLock == null)
        {
            MessageBox.Show(
                this,
                "This app does not own the host session.",
                "Cannot save",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        if (!_lockService.IsOurLock(
                _settings.SharedWorldRoot,
                world,
                _activeLock.Token))
        {
            MessageBox.Show(
                this,
                "The host lock no longer belongs to this app.",
                "Cannot save",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        var info = new HostSessionInfo
        {
            World = world,

            JoinCode =
                _txtJoinCode.Text.Trim(),

            Password =
                _txtServerPassword.Text,

            UpdatedUtc =
                DateTime.UtcNow,

            HostMachine =
                Environment.MachineName,

            HostUser =
                Environment.UserName
        };

        _hostSessionService.Save(
            _settings.SharedWorldRoot,
            info);

        _log.Info(
            $"Updated server information for '{world}'.");

        RefreshSessionPanel();
    }
    private void RefreshSessionPanel()
    {
        var world = _cmbWorld.Text.Trim();

        if (string.IsNullOrWhiteSpace(world))
        {
            _sessionPanel.Visible = false;
            return;
        }

        var hostLock = _lockService.ReadLock(
            _settings.SharedWorldRoot,
            world);

        if (hostLock == null)
        {
            _sessionPanel.Visible = false;
            return;
        }

        _sessionPanel.Visible = true;

        var isOurLock =
            _activeLock != null &&
            string.Equals(
                hostLock.Token,
                _activeLock.Token,
                StringComparison.Ordinal);

        var session =
            _hostSessionService.Read(
                _settings.SharedWorldRoot,
                world);

        if (session == null)
        {
            _log.Error(
                $"No session info could be read for '{world}'. " +
                $"Shared root: {_settings.SharedWorldRoot}");
        }
        else
        {
            _log.Info(
                $"Session loaded for '{world}': " +
                $"JoinCode='{session.JoinCode}', " +
                $"Password='{session.Password}', " +
                $"Host={session.HostMachine}/{session.HostUser}");
        }

        _lblSessionHost.Text =
            $"Hosted by {hostLock.Machine} / {hostLock.User}";

        if (session != null)
        {
            _txtJoinCode.Text =
                session.JoinCode;

            _txtServerPassword.Text =
                session.Password;
        }
        else
        {
            _txtJoinCode.Text = "";
            _txtServerPassword.Text = "";
        }

        // Only the current host can edit.
        _txtJoinCode.ReadOnly = !isOurLock;
        _txtServerPassword.ReadOnly = !isOurLock;

        _btnSaveSession.Visible = isOurLock;

        _btnCopyJoinCode.Visible =
            !string.IsNullOrWhiteSpace(
                _txtJoinCode.Text);

        // Is the code from this hosting session?
        if (session != null &&
            session.UpdatedUtc < hostLock.StartedUtc)
        {
            _lblSessionWarning.Text =
                "Join code has not been updated for this hosting session yet.";

            _lblSessionWarning.Visible = true;
        }
        else
        {
            _lblSessionWarning.Visible = false;
        }
    }
    private void OpenFolder(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            Directory.CreateDirectory(path);
            Process.Start(new ProcessStartInfo { FileName = "explorer.exe", Arguments = $"\"{path}\"", UseShellExecute = true });
        }
        catch (Exception ex)
        {
            ShowError("Could not open the folder.", ex);
        }
    }

    private void AppendLog(string line)
    {
        if (IsDisposed) return;
        if (InvokeRequired)
        {
            BeginInvoke(() => AppendLog(line));
            return;
        }
        _txtLog.AppendText(line + Environment.NewLine);
    }

    private void ShowError(string title, Exception ex)
    {
        _log.Error(ex.ToString());
        MessageBox.Show(this, $"{title}\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        SaveSettings();
        if (_activeLock != null)
        {
            var result = MessageBox.Show(this, "This app still owns a host lock. Closing now will KEEP the lock for safety.\n\nIf the updated world has been copied to OneDrive, wait for OneDrive to finish syncing and release the lock first.\n\nClose anyway?", "Host lock still active", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes) e.Cancel = true;
        }
        else if (_busy)
        {
            var result = MessageBox.Show(this, "An operation is still running. Closing may interrupt it. Close anyway?", "Operation in progress", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes) e.Cancel = true;
        }
    }
}
