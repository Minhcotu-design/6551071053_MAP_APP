using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QL_KhachSan.Models;

namespace QL_KhachSan.LoaiPhong;

public sealed class FormThemLoaiPhong : Form
{
    private readonly TextBox _txtMaLoaiPhong = HotelUi.TextBox();
    private readonly TextBox _txtTenLP = HotelUi.TextBox();
    private readonly TextBox _txtSoGiuong = HotelUi.TextBox();
    private readonly TextBox _txtSoNguoiToiDa = HotelUi.TextBox();
    private readonly TextBox _txtGiaNgay = HotelUi.TextBox();
    private readonly TextBox _txtGiaGio = HotelUi.TextBox();

    public FormThemLoaiPhong()
    {
        Text = "Thêm mới loại phòng";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        ClientSize = new Size(595, 588);
        BuildLayout();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(24), AutoScroll = true, RightToLeft = RightToLeft.No };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 175));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.Controls.Add(HotelUi.Heading("Thêm mới thông tin loại phòng"), 0, 0);
        root.SetColumnSpan(root.Controls[0], 2);
        AddField(root, "Mã loại phòng", _txtMaLoaiPhong);
        AddField(root, "Tên loại phòng", _txtTenLP);
        AddField(root, "Số giường", _txtSoGiuong);
        AddField(root, "Số người tối đa", _txtSoNguoiToiDa);
        AddField(root, "Giá ngày", _txtGiaNgay);
        AddField(root, "Giá giờ", _txtGiaGio);
        _txtMaLoaiPhong.MaxLength = 5;
        _txtMaLoaiPhong.CharacterCasing = CharacterCasing.Upper;
        _txtMaLoaiPhong.KeyPress += ChiChoNhapMa;
        _txtTenLP.MaxLength = 20;
        _txtSoGiuong.MaxLength = _txtSoNguoiToiDa.MaxLength = 2;
        _txtSoGiuong.KeyPress += ChiChoNhapSo;
        _txtSoNguoiToiDa.KeyPress += ChiChoNhapSo;
        var buttons = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill };
        var add = HotelUi.Button("Thêm mới", true);
        var close = HotelUi.Button("Thoát");
        add.Click += BtnThemMoi_Click;
        close.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        buttons.Controls.AddRange([close, add]);
        root.Controls.Add(buttons, 0, root.RowCount);
        root.SetColumnSpan(buttons, 2);
        Controls.Add(root);
        AcceptButton = add;
        CancelButton = close;
    }

    private static void AddField(TableLayoutPanel root, string label, Control input)
    {
        var row = root.RowCount++;
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
        root.Controls.Add(HotelUi.FieldLabel(label), 0, row);
        input.Dock = DockStyle.Fill;
        input.Margin = new Padding(5, 8, 5, 5);
        root.Controls.Add(input, 1, row);
    }

    private void BtnThemMoi_Click(object? sender, EventArgs e)
    {
        var ma = _txtMaLoaiPhong.Text.Trim();
        var ten = _txtTenLP.Text.Trim();
        if (ma.Length == 0 || ten.Length == 0)
        {
            MessageBox.Show("Vui lòng nhập đầy đủ thông tin.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (!int.TryParse(_txtSoGiuong.Text.Trim(), out var beds) || beds <= 0)
        {
            MessageBox.Show("Số giường không hợp lệ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtSoGiuong.Focus();
            return;
        }
        if (!int.TryParse(_txtSoNguoiToiDa.Text.Trim(), out var capacity) || capacity <= 0)
        {
            MessageBox.Show("Số người tối đa không hợp lệ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtSoNguoiToiDa.Focus();
            return;
        }
        if (!TryReadMoney(_txtGiaNgay.Text, out var dailyPrice) || dailyPrice <= 0)
        {
            MessageBox.Show("Giá ngày không hợp lệ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtGiaNgay.Focus();
            return;
        }
        if (!TryReadMoney(_txtGiaGio.Text, out var hourlyPrice) || hourlyPrice <= 0)
        {
            MessageBox.Show("Giá giờ không hợp lệ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtGiaGio.Focus();
            return;
        }
        try
        {
            using var db = new HotelManagementContext();
            if (db.LoaiPhongs.Any(x => x.MaLph == ma))
            {
                MessageBox.Show("Mã loại phòng đã tồn tại!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtMaLoaiPhong.Focus();
                return;
            }
            db.LoaiPhongs.Add(new Models.LoaiPhong
            {
                MaLph = ma,
                TenLph = ten,
                SoGiuong = beds,
                SoNguoiToiDa = capacity,
                GiaNgay = dailyPrice,
                GiaGio = hourlyPrice
            });
            db.SaveChanges();
            MessageBox.Show("Thêm loại phòng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (DbUpdateException ex)
        {
            MessageBox.Show("Lỗi: " + (ex.InnerException?.Message ?? ex.Message), "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static bool TryReadMoney(string input, out decimal value) =>
        decimal.TryParse(input.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out value)
        || decimal.TryParse(input.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out value);

    private static void ChiChoNhapSo(object? sender, KeyPressEventArgs e)
    {
        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            e.Handled = true;
    }

    private static void ChiChoNhapMa(object? sender, KeyPressEventArgs e)
    {
        if (!char.IsControl(e.KeyChar) && !char.IsLetterOrDigit(e.KeyChar))
            e.Handled = true;
    }
}
