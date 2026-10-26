using Microsoft.EntityFrameworkCore;
using QL_KhachSan.Models;

namespace QL_KhachSan.DangNhap;

public sealed class FormDangNhap : Form
{
    public static string MaNV_DangNhap { get; private set; } = string.Empty;
    public static string TenNV_DangNhap { get; private set; } = string.Empty;
    public static int CapDoQuyen_DangNhap { get; private set; }

    private readonly TextBox _txtTenDangNhap = HotelUi.TextBox();
    private readonly TextBox _txtMatKhau = HotelUi.TextBox();
    private readonly Button _btnDangNhap = HotelUi.Button("Đăng nhập", primary: true);

    public FormDangNhap()
    {
        Text = "FormDangNhap";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(855, 562);
        BackColor = Color.Beige;
        BuildLayout();
    }

    private void BuildLayout()
    {
        var picture = new PictureBox
        {
            Image = AppImages.Get("pictureIcon"),
            Location = new Point(0, 16),
            Size = new Size(440, 530),
            SizeMode = PictureBoxSizeMode.StretchImage
        };
        var panel = new Panel
        {
            Location = new Point(448, 16),
            Size = new Size(395, 531),
            BackColor = Color.FromArgb(250, 247, 236)
        };
        var logo = new PictureBox
        {
            Image = AppImages.Get("DangNhap"),
            Location = new Point(97, 44),
            Size = new Size(203, 75),
            SizeMode = PictureBoxSizeMode.StretchImage
        };
        AddField(panel, "Tên đăng nhập", _txtTenDangNhap, 182);
        AddField(panel, "Mật khẩu", _txtMatKhau, 267);
        _txtTenDangNhap.Location = new Point(177, 170);
        _txtMatKhau.Location = new Point(177, 255);
        _txtTenDangNhap.Size = new Size(192, 34);
        _txtMatKhau.Size = new Size(192, 34);
        _txtMatKhau.UseSystemPasswordChar = true;

        var forgot = new LinkLabel
        {
            Text = "Quên mật khẩu",
            Font = new Font("Microsoft Sans Serif", 13F, FontStyle.Italic),
            AutoSize = true,
            Location = new Point(222, 328)
        };
        _btnDangNhap.Location = new Point(117, 380);
        _btnDangNhap.Size = new Size(134, 66);
        _btnDangNhap.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold);
        _btnDangNhap.Click += BtnDangNhap_Click;
        _txtMatKhau.KeyDown += TxtMatKhau_KeyDown;
        AcceptButton = _btnDangNhap;

        panel.Controls.AddRange([logo, forgot, _btnDangNhap]);
        Controls.AddRange([picture, panel]);
        ActiveControl = _txtTenDangNhap;
    }

    private static void AddField(Control parent, string caption, TextBox input, int labelTop)
    {
        parent.Controls.Add(new Label
        {
            Text = caption,
            AutoSize = true,
            ForeColor = Color.FromArgb(150, 0, 0),
            Font = new Font("Microsoft Sans Serif", 11F),
            Location = new Point(30, labelTop)
        });
        parent.Controls.Add(input);
    }

    private async void BtnDangNhap_Click(object? sender, EventArgs e)
    {
        var tenDangNhap = _txtTenDangNhap.Text.Trim();
        var matKhau = _txtMatKhau.Text;
        if (tenDangNhap.Length == 0 || matKhau.Length == 0)
        {
            MessageBox.Show("Vui lòng nhập đầy đủ thông tin.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _btnDangNhap.Enabled = false;
        try
        {
            await using var db = new HotelManagementContext();
            var user = await db.TaiKhoans.AsNoTracking()
                .Where(t => t.TenTk == tenDangNhap && t.DaXoa != true)
                .Select(t => new
                {
                    t.Password,
                    t.MaNv,
                    t.CapDoQuyen,
                    TenNv = t.MaNvNavigation.TenNv,
                    NhanVienDaXoa = t.MaNvNavigation.DaXoa
                })
                .FirstOrDefaultAsync();

            var valid = user is not null
                && user.NhanVienDaXoa != true
                && string.Equals(user.Password, matKhau, StringComparison.Ordinal);
            if (!valid)
            {
                MessageBox.Show("Sai tên đăng nhập hoặc mật khẩu.", "Đăng nhập thất bại",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                _txtMatKhau.Clear();
                _txtMatKhau.Focus();
                return;
            }

            MaNV_DangNhap = user!.MaNv;
            TenNV_DangNhap = user.TenNv;
            CapDoQuyen_DangNhap = user.CapDoQuyen;
            Tag = user.TenNv;
            DialogResult = DialogResult.OK;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi kết nối CSDL: " + ex.Message, "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _btnDangNhap.Enabled = true;
        }
    }

    private void TxtMatKhau_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            _btnDangNhap.PerformClick();
        }
    }
}
