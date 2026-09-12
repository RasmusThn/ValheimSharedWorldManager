using ValheimSharedWorldManager.Utilities;

namespace ValheimSharedWorldManager.Controls;

public sealed class StatusCard : Panel
{
    private readonly Label _title = new();
    private readonly Label _value = new();
    private readonly Label _detail = new();
    private readonly Panel _indicator = new();
    private readonly TableLayoutPanel _stack;

    public event EventHandler? CardClick;

    public StatusCard()
    {
        Height = 104;
        BackColor = AppTheme.Surface;
        Padding = new Padding(14);
        Margin = new Padding(0, 0, 10, 0);

        _indicator.Width = 6;
        _indicator.Dock = DockStyle.Left;
        _indicator.BackColor = AppTheme.MutedText;
        Controls.Add(_indicator);

        _stack = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(10, 0, 0, 0)
        };

        _stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _stack.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        Controls.Add(_stack);

        _title.AutoSize = true;
        _title.ForeColor = AppTheme.MutedText;
        _title.Font = new Font("Segoe UI", 9F);
        _stack.Controls.Add(_title, 0, 0);

        _value.AutoSize = true;
        _value.ForeColor = AppTheme.Text;
        _value.Font = new Font("Segoe UI Semibold", 13F);
        _value.Margin = new Padding(0, 4, 0, 0);
        _stack.Controls.Add(_value, 0, 1);

        _detail.AutoSize = true;
        _detail.ForeColor = AppTheme.MutedText;
        _detail.Font = new Font("Segoe UI", 8.5F);
        _detail.Margin = new Padding(0, 3, 0, 0);
        _stack.Controls.Add(_detail, 0, 2);

        AttachClickHandlers(this);
    }

    public void Set(
        string title,
        string value,
        string detail,
        Color indicator)
    {
        _title.Text = title;
        _value.Text = value;
        _detail.Text = detail;
        _indicator.BackColor = indicator;
    }

    public void SetClickable(bool clickable)
    {
        Cursor = clickable ? Cursors.Hand : Cursors.Default;

        _title.Cursor = Cursor;
        _value.Cursor = Cursor;
        _detail.Cursor = Cursor;
        _stack.Cursor = Cursor;
    }

    private void AttachClickHandlers(Control control)
    {
        control.Click += (_, e) => CardClick?.Invoke(this, e);

        foreach (Control child in control.Controls)
            AttachClickHandlers(child);
    }
}
