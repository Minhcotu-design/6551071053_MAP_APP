namespace QL_KhachSan;

public sealed class FormTrangChu : UserControl
{
    public FormTrangChu()
    {
        BackColor = HotelUi.Background;
        Padding = new Padding(18);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = HotelUi.Background
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));

        var hero = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(28),
            Margin = new Padding(0, 0, 0, 12)
        };
        var accent = new Panel { Dock = DockStyle.Left, Width = 7, BackColor = HotelUi.Green };
        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(26, 8, 10, 8),
            BackColor = Color.White
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 61));
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 39));

        var message = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            BackColor = Color.White
        };
        message.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        message.RowStyles.Add(new RowStyle(SizeType.Absolute, 148));
        message.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));
        message.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        message.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        message.Controls.Add(new Label
        {
            Text = "HOTEL MANAGEMENT",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            ForeColor = HotelUi.Green,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 0);
        message.Controls.Add(new Label
        {
            Text = "Chào mừng\nđến với khách sạn",
            Dock = DockStyle.Fill,
            ForeColor = HotelUi.DarkGreen,
            Font = new Font("Segoe UI Semibold", 25, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(0, 3, 0, 3),
            Margin = new Padding(0)
        }, 0, 1);
        message.Controls.Add(new Label
        {
            Text = "Hệ thống quản lý khách sạn",
            Dock = DockStyle.Fill,
            ForeColor = HotelUi.Muted,
            Font = new Font("Segoe UI", 15),
            TextAlign = ContentAlignment.TopLeft
        }, 0, 2);
        message.Controls.Add(new Label
        {
            Text = "Chọn một chức năng ở thanh bên để bắt đầu làm việc.",
            Dock = DockStyle.Fill,
            ForeColor = HotelUi.Text,
            Font = new Font("Segoe UI", 11),
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 4);

        var picture = new PictureBox
        {
            Image = AppImages.Get("pictureIconNoneBackGroundResize"),
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.White,
            TabStop = false,
            Margin = new Padding(12)
        };
        content.Controls.Add(message, 0, 0);
        content.Controls.Add(picture, 1, 0);
        hero.Controls.Add(content);
        hero.Controls.Add(accent);
        layout.Controls.Add(hero, 0, 0);

        var footer = new Label
        {
            Text = "QUẢN LÝ PHÒNG  ·  QUẢN LÝ LOẠI PHÒNG  ·  SƠ ĐỒ PHÒNG",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = HotelUi.Muted,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = Color.FromArgb(232, 242, 238),
            Margin = new Padding(0)
        };
        layout.Controls.Add(footer, 0, 1);
        Controls.Add(layout);
    }
}
