using QL_KhachSan.DangNhap;
using QL_KhachSan.DichVu;
using QL_KhachSan.ChiTietTienNghi;
using QL_KhachSan.KhachHang;
using QL_KhachSan.LoaiPhong;
using QL_KhachSan.NhanVien;
using QL_KhachSan.Phong;
using QL_KhachSan.SoDoPhong;
using QL_KhachSan.TienNghi;

namespace QL_KhachSan;

public partial class FormMain : Form
{
    private readonly Panel _panelMain = new();
    private readonly Label _lblTenNguoiDung = new();

    public FormMain()
    {
        InitializeComponent();
        BuildLayout();
        ShowHome();
    }

    private void BuildLayout()
    {
        BackColor = HotelUi.Background;
        Font = new Font("Segoe UI", 10F);

        var header = new Panel { Dock = DockStyle.Top, Height = 76, BackColor = HotelUi.Green };
        var logo = new PictureBox
        {
            Image = AppImages.Get("Menu"),
            SizeMode = PictureBoxSizeMode.Zoom,
            Location = new Point(18, 12),
            Size = new Size(48, 52)
        };
        var title = new Label
        {
            Text = "Hotel Management",
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 20, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(76, 19)
        };
        _lblTenNguoiDung.Text = FormDangNhap.TenNV_DangNhap;
        _lblTenNguoiDung.ForeColor = Color.White;
        _lblTenNguoiDung.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
        _lblTenNguoiDung.AutoSize = false;
        _lblTenNguoiDung.Dock = DockStyle.Fill;
        _lblTenNguoiDung.TextAlign = ContentAlignment.MiddleLeft;
        _lblTenNguoiDung.AutoEllipsis = false;
        _lblTenNguoiDung.Margin = new Padding(4, 0, 4, 0);
        var avatar = new PictureBox
        {
            Image = AppImages.Get("user"),
            Size = new Size(36, 36),
            SizeMode = PictureBoxSizeMode.Zoom,
            Anchor = AnchorStyles.None,
            Margin = new Padding(0),
            TabStop = false
        };
        var logout = new LinkLabel
        {
            Text = "Đăng xuất",
            LinkColor = Color.White,
            ActiveLinkColor = Color.LightYellow,
            VisitedLinkColor = Color.White,
            AutoSize = false,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Margin = new Padding(0)
        };
        logout.LinkClicked += (_, _) =>
        {
            if (MessageBox.Show("Bạn có chắc muốn đăng xuất?", "Đăng xuất",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            Hide();
            using var formLogin = new FormDangNhap();
            if (formLogin.ShowDialog() == DialogResult.OK)
            {
                _lblTenNguoiDung.Text = FormDangNhap.TenNV_DangNhap;
                ShowHome();
                Show();
            }
            else
            {
                Close();
            }
        };
        var account = new TableLayoutPanel
        {
            Dock = DockStyle.Right,
            Width = 340,
            Height = header.Height,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = HotelUi.Green,
            Padding = new Padding(10, 14, 10, 14),
            Margin = new Padding(0)
        };
        account.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40));
        account.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        account.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 86));
        account.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        account.Controls.AddRange([avatar, _lblTenNguoiDung, logout]);
        account.SetColumn(avatar, 0);
        account.SetColumn(_lblTenNguoiDung, 1);
        account.SetColumn(logout, 2);
        header.Controls.AddRange([logo, title, account]);
        header.Resize += (_, _) =>
        {
            var compact = header.ClientSize.Width < 680;
            logo.Visible = !compact;
            title.Visible = !compact;
            account.Width = Math.Min(340, Math.Max(240, header.ClientSize.Width - 16));
        };

        var side = new Panel { Dock = DockStyle.Left, Width = 275, BackColor = HotelUi.DarkGreen, AutoScroll = true };
        AddMenu(side, "Trang chủ", "MenuTrangChu", ShowHome);
        AddMenu(side, "Sơ đồ phòng", "MenuSoDoPhong", () => OpenChild(new FormSoDoPhong()));
        AddMenu(side, "Phòng", "MenuPhong", () => OpenChild(new FormDanhSachPhong()));
        AddMenu(side, "Loại phòng", "MenuLoaiPhong", () => OpenChild(new FormDanhSachLoaiPhong()));
        AddMenu(side, "Danh sách đặt phòng", "MenuDatPhong", () => HienThiDangPhatTrien("Danh sách đặt phòng"));
        AddMenu(side, "Danh sách dịch vụ", "MenuQuanLyDichVu", () => OpenChild(new FormDanhSachDichVu()));
        AddMenu(side, "Danh sách hóa đơn", "MenuDanhSachHoaDon", () => HienThiDangPhatTrien("Danh sách hóa đơn"));
        AddMenu(side, "Danh sách khách hàng", "MenuQuanLyKhachHang", () => OpenChild(new FormDanhSachKhachHang()));
        AddMenu(side, "Danh sách nhân viên", "MenuQuanLyNhanVien", () => OpenChild(new FormDanhSachNhanVien()));
        AddMenu(side, "Danh sách tiện nghi", "MenuTienNghi", () => OpenChild(new FormDanhSachTienNghi()));
        AddMenu(side, "Chi tiết tiện nghi", "MenuChiTietTienNghi", () => OpenChild(new FormDanhSachChiTietTienNghi()));
        AddMenu(side, "Danh sách tài khoản", "MenuQuanLyTaiKhoan", () => HienThiDangPhatTrien("Danh sách tài khoản"));
        AddMenu(side, "Thống kê", "MenuThongKe", () => HienThiDangPhatTrien("Thống kê"));

        _panelMain.Dock = DockStyle.Fill;
        _panelMain.Padding = new Padding(24);
        Controls.Add(_panelMain);
        Controls.Add(side);
        Controls.Add(header);
    }

    private static void AddMenu(Control parent, string caption, string imageName, Action action)
    {
        var button = new Button
        {
            Text = caption,
            Image = AppImages.Get(imageName),
            ImageAlign = ContentAlignment.MiddleLeft,
            TextAlign = ContentAlignment.MiddleLeft,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            Dock = DockStyle.Top,
            Height = 48,
            FlatStyle = FlatStyle.Flat,
            BackColor = HotelUi.DarkGreen,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10.5F),
            Padding = new Padding(12, 0, 8, 0),
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderSize = 0;
        button.Click += (_, _) => action();
        parent.Controls.Add(button);
        parent.Controls.SetChildIndex(button, parent.Controls.Count - 1);
    }

    private void ShowHome()
    {
        ClearMainPanel();
        _panelMain.Controls.Add(new FormTrangChu { Dock = DockStyle.Fill });
    }

    private void OpenChild(Form child)
    {
        ClearMainPanel();
        child.TopLevel = false;
        child.FormBorderStyle = FormBorderStyle.None;
        child.Dock = DockStyle.Fill;
        _panelMain.Controls.Add(child);
        child.Show();
    }

    private void ClearMainPanel()
    {
        foreach (Control control in _panelMain.Controls.Cast<Control>().ToList())
        {
            _panelMain.Controls.Remove(control);
            control.Dispose();
        }
    }

    private static void HienThiDangPhatTrien(string tenChucNang)
    {
        MessageBox.Show($"Chức năng {tenChucNang} đang được phát triển.", "Thông báo",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
