using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ValheimSharedWorldManager.Utilities;

namespace ValheimSharedWorldManager
{
    public sealed class HelpForm : Form
    {
        public HelpForm()
        {
            Text = "Valheim Shared World Manager - Help";
            Width = 760;
            Height = 620;
            StartPosition = FormStartPosition.CenterParent;

            BackColor = AppTheme.Background;
            ForeColor = AppTheme.Text;
            Font = new Font("Segoe UI", 10F);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                ColumnCount = 1,
                RowCount = 4
            };

            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            Controls.Add(root);

            var heading = AppTheme.Heading(
                "Valheim Shared World Manager",
                18F);

            root.Controls.Add(heading, 0, 0);

            var text = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = AppTheme.Surface,
                ForeColor = AppTheme.Text,
                Font = new Font("Segoe UI", 10F),
                Text =
                @"Quick guide

                1. Choose a shared world.
                2. Click HOST WORLD.
                3. Start the world in Valheim.
                4. Enter Join Code and Password in the manager.
                5. When finished, close Valheim.
                6. Wait for the world to publish.
                7. Wait for OneDrive to be up to date.
                8. Click RELEASE HOST LOCK.

                If somebody else is hosting, use JOIN WORLD instead.

                Important:
                - Only one person should host at a time.
                - Never Force unlock unless you are certain nobody is hosting.
                - Backups are created automatically when enabled."
                };

            root.Controls.Add(text, 0, 1);

            var github = AppTheme.SecondaryButton("Open GitHub");
            github.AutoSize = true;
            github.Click += (_, _) =>
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName =
                        "https://github.com/RasmusThn/ValheimSharedWorldManager",
                    UseShellExecute = true
                });
            };

            root.Controls.Add(github, 0, 2);

            var releases =
                AppTheme.PrimaryButton("Download latest release");

            releases.AutoSize = true;

            releases.Click += (_, _) =>
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName =
                        "https://github.com/RasmusThn/ValheimSharedWorldManager/releases/latest",
                    UseShellExecute = true
                });
            };

            root.Controls.Add(releases, 0, 3);
        }
    }
}
