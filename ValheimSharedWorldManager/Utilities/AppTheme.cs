namespace ValheimSharedWorldManager.Utilities;

public static class AppTheme
{
    public static readonly Color Background = Color.FromArgb(19, 22, 27);
    public static readonly Color Surface = Color.FromArgb(29, 33, 40);
    public static readonly Color SurfaceAlt = Color.FromArgb(37, 42, 50);
    public static readonly Color Border = Color.FromArgb(57, 63, 73);
    public static readonly Color Text = Color.FromArgb(239, 242, 246);
    public static readonly Color MutedText = Color.FromArgb(166, 174, 186);
    public static readonly Color Accent = Color.FromArgb(89, 155, 255);
    public static readonly Color Success = Color.FromArgb(74, 201, 128);
    public static readonly Color Warning = Color.FromArgb(242, 182, 72);
    public static readonly Color Danger = Color.FromArgb(235, 91, 91);

    public static Button PrimaryButton(string text)
    {
        return new Button
        {
            Text = text,
            AutoSize = false,
            Height = 44,
            FlatStyle = FlatStyle.Flat,
            BackColor = Accent,
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 10.5F),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 8, 0)
        };
    }

    public static Button SecondaryButton(string text)
    {
        var button = PrimaryButton(text);
        button.BackColor = SurfaceAlt;
        button.FlatAppearance.BorderColor = Border;
        button.FlatAppearance.BorderSize = 1;
        button.ForeColor = Text;
        return button;
    }

    public static Label Heading(string text, float size = 16F)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", size),
            ForeColor = Text,
            BackColor = Color.Transparent
        };
    }

    public static Label Muted(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            ForeColor = MutedText,
            BackColor = Color.Transparent
        };
    }
}
