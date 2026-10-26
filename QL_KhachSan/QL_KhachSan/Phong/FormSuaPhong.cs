using Microsoft.EntityFrameworkCore;
using QL_KhachSan.Models;

namespace QL_KhachSan.Phong;

public sealed class FormSuaPhong : Form
{
    private readonly Label _lblMaPH = new() { AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold) };
    private readonly ComboBox _cbLoaiPhong = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10.5F) };
    private readonly ComboBox _cbTrangThai = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10.5F) };
    private readonly ComboBox _cbDonDep = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10.5F) };
    private readonly TextBox _txtGhiChu = HotelUi.TextBox();

    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public string? MaPhong { get; set; }

    public FormSuaPhong()
    {
        Text = "Cập nhật thông tin phòng";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        ClientSize = new Size(595, 588);
        BuildLayout();
        Load += async (_, _) => await LoadRoomAsync();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(24), AutoScroll = true, RightToLeft = RightToLeft.No };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 175));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.Controls.Add(HotelUi.Heading("Cập nhật thông tin phòng"), 0, 0);
        root.SetColumnSpan(root.Controls[0], 2);
        AddField(root, "Mã phòng", _lblMaPH);
        AddField(root, "Loại phòng", _cbLoaiPhong);
        _cbTrangThai.Items.AddRange(["Bình thường", "Đang sửa chữa"]);
        AddField(root, "Trạng thái phòng", _cbTrangThai);
        _cbDonDep.Items.AddRange(["Đã dọn dẹp", "Chưa dọn dẹp"]);
        AddField(root, "Tình trạng dọn dẹp", _cbDonDep);
        AddField(root, "Ghi chú", _txtGhiChu);
        _txtGhiChu.MaxLength = 100;
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

    private async Task LoadRoomAsync()
    {
        if (string.IsNullOrWhiteSpace(MaPhong))
        {
            MessageBox.Show("Chưa xác định được phòng cần cập nhật.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
            return;
        }
        try
        {
            using var db = new HotelManagementContext();
            var types = await db.LoaiPhongs.AsNoTracking().OrderBy(x => x.MaLph)
                .Select(x => new { x.MaLph, x.TenLph }).ToListAsync();
            _cbLoaiPhong.DataSource = types;
            _cbLoaiPhong.DisplayMember = "TenLph";
            _cbLoaiPhong.ValueMember = "MaLph";
            var room = await db.Phongs.AsNoTracking().FirstOrDefaultAsync(x => x.MaPh == MaPhong && x.DaXoa != true);
            if (room is null)
            {
                MessageBox.Show("Không tìm thấy phòng cần cập nhật.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
            _lblMaPH.Text = room.MaPh;
            _cbLoaiPhong.SelectedValue = room.MaLph;
            _cbTrangThai.SelectedItem = room.Ttph;
            _cbDonDep.SelectedItem = room.Ttdd;
            _txtGhiChu.Text = room.GhiChu ?? string.Empty;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi tải thông tin phòng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnCapNhat_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(MaPhong) || _cbLoaiPhong.SelectedValue is null)
        {
            MessageBox.Show("Thông tin phòng chưa đầy đủ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        try
        {
            using var db = new HotelManagementContext();
            var room = db.Phongs.FirstOrDefault(x => x.MaPh == MaPhong && x.DaXoa != true);
            if (room is null)
            {
                MessageBox.Show("Không tìm thấy phòng cần cập nhật.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            room.MaLph = _cbLoaiPhong.SelectedValue.ToString()!;
            room.Ttph = _cbTrangThai.SelectedItem?.ToString() ?? room.Ttph;
            room.Ttdd = _cbDonDep.SelectedItem?.ToString() ?? room.Ttdd;
            room.GhiChu = string.IsNullOrWhiteSpace(_txtGhiChu.Text) ? null : _txtGhiChu.Text.Trim();
            db.SaveChanges();
            MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
