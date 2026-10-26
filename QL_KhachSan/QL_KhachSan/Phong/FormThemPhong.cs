using Microsoft.EntityFrameworkCore;
using QL_KhachSan.Models;

namespace QL_KhachSan.Phong;

public sealed class FormThemPhong : Form
{
    private readonly TextBox _txtMaPH = HotelUi.TextBox();
    private readonly ComboBox _cbLoaiPhong = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10.5F) };
    private readonly ComboBox _cbTrangThai = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10.5F) };
    private readonly ComboBox _cbDonDep = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10.5F) };
    private readonly TextBox _txtGhiChu = HotelUi.TextBox();

    public FormThemPhong()
    {
        Text = "Thêm mới phòng";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        ClientSize = new Size(595, 588);
        BuildLayout();
        Load += (_, _) => LoadLoaiPhong();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(24), AutoScroll = true, RightToLeft = RightToLeft.No };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 175));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.Controls.Add(HotelUi.Heading("Thêm mới thông tin phòng"), 0, 0);
        root.SetColumnSpan(root.Controls[0], 2);
        AddField(root, "Mã phòng", _txtMaPH);
        AddField(root, "Loại phòng", _cbLoaiPhong);
        AddField(root, "Trạng thái phòng", _cbTrangThai);
        AddField(root, "Tình trạng dọn dẹp", _cbDonDep);
        AddField(root, "Ghi chú", _txtGhiChu);
        _txtMaPH.MaxLength = 5;
        _txtGhiChu.MaxLength = 100;
        _txtTrangThaiOptions();
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

    private void _txtTrangThaiOptions()
    {
        _cbTrangThai.Items.AddRange(["Bình thường", "Đang sửa chữa"]);
        _cbTrangThai.SelectedIndex = 0;
        _cbDonDep.Items.AddRange(["Đã dọn dẹp", "Chưa dọn dẹp"]);
        _cbDonDep.SelectedIndex = 0;
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

    private void LoadLoaiPhong()
    {
        try
        {
            using var db = new HotelManagementContext();
            var types = db.LoaiPhongs.AsNoTracking().OrderBy(x => x.MaLph)
                .Select(x => new { x.MaLph, x.TenLph }).ToList();
            _cbLoaiPhong.DataSource = types;
            _cbLoaiPhong.DisplayMember = "TenLph";
            _cbLoaiPhong.ValueMember = "MaLph";
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi tải loại phòng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnThemMoi_Click(object? sender, EventArgs e)
    {
        var ma = _txtMaPH.Text.Trim();
        if (ma.Length == 0 || _cbLoaiPhong.SelectedValue is null)
        {
            MessageBox.Show("Vui lòng nhập đầy đủ thông tin.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        try
        {
            using var db = new HotelManagementContext();
            var existingRoom = db.Phongs.FirstOrDefault(p => p.MaPh == ma);
            if (existingRoom is not null && existingRoom.DaXoa != true)
            {
                MessageBox.Show("Mã phòng đã tồn tại!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtMaPH.Focus();
                return;
            }

            var loaiPhong = _cbLoaiPhong.SelectedValue.ToString()!;
            var trangThai = _cbTrangThai.SelectedItem?.ToString() ?? "Bình thường";
            var donDep = _cbDonDep.SelectedItem?.ToString() ?? "Đã dọn dẹp";
            var ghiChu = string.IsNullOrWhiteSpace(_txtGhiChu.Text) ? null : _txtGhiChu.Text.Trim();

            if (existingRoom is not null)
            {
                existingRoom.MaLph = loaiPhong;
                existingRoom.Ttph = trangThai;
                existingRoom.Ttdd = donDep;
                existingRoom.GhiChu = ghiChu;
                existingRoom.DaXoa = false;
            }
            else
            {
                db.Phongs.Add(new Models.Phong
                {
                    MaPh = ma,
                    MaLph = loaiPhong,
                    Ttph = trangThai,
                    Ttdd = donDep,
                    GhiChu = ghiChu,
                    DaXoa = false
                });
            }
            db.SaveChanges();
            MessageBox.Show(existingRoom is null ? "Thêm phòng thành công!" : "Khôi phục phòng thành công!",
                "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
}
