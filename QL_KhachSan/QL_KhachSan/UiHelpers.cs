using System.Drawing.Drawing2D;

namespace QL_KhachSan;

internal static class HotelUi
{
    public static readonly Color Green = Color.FromArgb(36, 137, 106);
    public static readonly Color DarkGreen = Color.FromArgb(28, 89, 74);
    public static readonly Color Background = Color.FromArgb(245, 248, 247);
    public static readonly Color Text = Color.FromArgb(42, 58, 53);
    public static readonly Color Muted = Color.FromArgb(105, 124, 117);
    public static readonly Color Border = Color.FromArgb(220, 230, 226);
    public static readonly Color Orange = Color.FromArgb(231, 143, 75);

    public static Label Heading(string text) => new()
    {
        Text = text,
        AutoSize = false,
        Dock = DockStyle.Fill,
        TextAlign = ContentAlignment.MiddleLeft,
        Padding = new Padding(0, 2, 0, 0),
        Font = new Font("Segoe UI Semibold", 20, FontStyle.Bold),
        ForeColor = DarkGreen,
        Margin = new Padding(0, 0, 0, 2)
    };

    public static Button Button(string text, bool primary = false) => new Button()
    {
        Text = text,
        AutoSize = true,
        MinimumSize = new Size(112, 38),
        FlatStyle = FlatStyle.Flat,
        BackColor = primary ? Green : Color.White,
        ForeColor = primary ? Color.White : DarkGreen,
        Font = new Font("Segoe UI", 10, FontStyle.Bold),
        Padding = new Padding(12, 6, 12, 6),
        Cursor = Cursors.Hand,
        Margin = new Padding(5)
    }.Also(b => b.FlatAppearance.BorderColor = primary ? Green : Border);

    public static TextBox TextBox(string placeholder = "") => new()
    {
        Font = new Font("Segoe UI", 10.5F),
        PlaceholderText = placeholder,
        BorderStyle = BorderStyle.FixedSingle,
        Height = 34,
        Margin = new Padding(5)
    };

    public static Label FieldLabel(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Font = new Font("Segoe UI", 10, FontStyle.Bold),
        ForeColor = Text,
        Margin = new Padding(5, 10, 5, 2)
    };

    public static DataGridView Grid() => new DataGridView()
    {
        Dock = DockStyle.Fill,
        BackgroundColor = Color.White,
        BorderStyle = BorderStyle.None,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        AllowUserToResizeRows = false,
        ReadOnly = true,
        MultiSelect = false,
        RowHeadersVisible = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        ColumnHeadersHeight = 42,
        RowTemplate = { Height = 38 },
        EnableHeadersVisualStyles = false,
        Font = new Font("Segoe UI", 10),
        GridColor = Border
    }.Also(g =>
    {
        g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(232, 242, 238);
        g.ColumnHeadersDefaultCellStyle.ForeColor = DarkGreen;
        g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(216, 239, 230);
        g.DefaultCellStyle.SelectionForeColor = Text;
        g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 251, 250);
    });

    public static Panel Card() => new()
    {
        BackColor = Color.White,
        Padding = new Padding(16),
        Margin = new Padding(8),
        BorderStyle = BorderStyle.FixedSingle
    };

    public static GraphicsPath RoundedPath(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        int d = radius * 2;
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}

internal static class ControlExtensions
{
    public static T Also<T>(this T value, Action<T> action)
    {
        action(value);
        return value;
    }
}
