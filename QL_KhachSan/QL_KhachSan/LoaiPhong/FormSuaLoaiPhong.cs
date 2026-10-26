using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QL_KhachSan.Models;

namespace QL_KhachSan.LoaiPhong;

public sealed class FormSuaLoaiPhong : Form
{
    private readonly TextBox _txtTenLP = HotelUi.TextBox();
    private readonly TextBox _txtSoGiuong = HotelUi.TextBox();
    private readonly TextBox _txtSoNguoiToiDa = HotelUi.TextBox();
    private readonly TextBox _txtGiaNgay = HotelUi.TextBox();
    private readonly TextBox _txtGiaGio = HotelUi.TextBox();

    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public string? MaLoaiPhong { get; set; }

    public FormSuaLoaiPhong()
    {
        Text = "Cập nhật thông tin loại phòng";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        ClientSize = new Size(604, 626);
        BuildLayout();
        Load += async (_, _) => await LoadLoaiPhongAsync();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(24), AutoScroll = true, RightToLeft = RightToLeft.No };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 175));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.Controls.Add(HotelUi.Heading("Cập nhật thông tin loại phòng"), 0, 0);
        root.SetColumnSpan(root.Controls[0], 2);
        AddField(root, "Tên loại phòng", _txtTenLP);
        AddField(root, "Số giường", _txtSoGiuong);
        AddField(root, "Số người tối đa", _txtSoNguoiToiDa);
        AddField(root, "Giá ngày", _txtGiaNgay);
        AddField(root, "Giá giờ", _txtGiaGio);
        _txtTenLP.MaxLength = 20;
        _txtSoGiuong.MaxLength = _txtSoNguoiToiDa.MaxLength = 2;
        _txtSoGiuong.KeyPress += ChiChoNhapSo;
        _txtSoNguoiToiDa.KeyPress += ChiChoNhapSo;
        var buttons = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill };
        var save = HotelUi.Button("Cập nhật", true);
        var close = HotelUi.Button("Thoát");
        save.Click += BtnCapNhat_Click;
        close.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        buttons.Controls.AddRange([close, save]);
        root.Controls.Add(buttons, 0, root.RowCount);
        root.SetColumnSpan(buttons, 2);
        Controls.Add(root);
        AcceptButton = save;
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

    private async Task LoadLoaiPhongAsync()
    {
        if (string.IsNullOrWhiteSpace(MaLoaiPhong))
        {
            MessageBox.Show("Chưa xác định được loại phòng cần cập nhật.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
            return;
        }
        try
        {
            using var db = new HotelManagementContext();
            var item = await db.LoaiPhongs.AsNoTracking().FirstOrDefaultAsync(x => x.MaLph == MaLoaiPhong);
            if (item is null)
            {
                MessageBox.Show("Không tìm thấy loại phòng cần cập nhật.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
            _txtTenLP.Text = item.TenLph;
            _txtSoGiuong.Text = item.SoGiuong.ToString(CultureInfo.CurrentCulture);
            _txtSoNguoiToiDa.Text = item.SoNguoiToiDa.ToString(CultureInfo.CurrentCulture);
            _txtGiaNgay.Text = item.GiaNgay.ToString("0.##", CultureInfo.CurrentCulture);
            _txtGiaGio.Text = item.GiaGio.ToString("0.##", CultureInfo.CurrentCulture);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi tải thông tin loại phòng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnCapNhat_Click(object? sender, EventArgs e)
    {
        var name = _txtTenLP.Text.Trim();
        if (string.IsNullOrWhiteSpace(MaLoaiPhong) || name.Length == 0)
        {
            MessageBox.Show("Vui lòng nhập tên loại phòng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            var item = db.LoaiPhongs.FirstOrDefault(x => x.MaLph == MaLoaiPhong);
            if (item is null)
            {
                MessageBox.Show("Không tìm thấy loại phòng cần cập nhật.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            item.TenLph = name;
            item.SoGiuong = beds;
            item.SoNguoiToiDa = capacity;
            item.GiaNgay = dailyPrice;
            item.GiaGio = hourlyPrice;
            var rows = db.SaveChanges();
            if (rows > 0)
            {
                MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Thông tin không thay đổi nên không có gì để cập nhật.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (DbUpdateException ex)
        {
            MessageBox.Show("Lỗi khi cập nhật: " + (ex.InnerException?.Message ?? ex.Message), "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi khi cập nhật: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
}
