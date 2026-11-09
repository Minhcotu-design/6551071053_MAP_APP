using Microsoft.EntityFrameworkCore;
using QL_KhachSan.Models;

namespace QL_KhachSan.PhieuThue;

internal sealed class FormSuaChiTietPhieuThue : Form
{
    private readonly string _id;
    private readonly ComboBox _status = new() { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
    private readonly Button _save = HotelUi.Button("Cập nhật", true);

    public FormSuaChiTietPhieuThue(string id)
    {
        _id = id;
        Text = "Cập nhật chi tiết phiếu thuê";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        ClientSize = new Size(580, 310);
        _status.Items.AddRange(["Đang thuê", "Đã thanh toán"]);

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20, 14, 20, 8), ColumnCount = 2, RowCount = 3 };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        root.Controls.Add(HotelUi.Heading("Cập nhật trạng thái chi tiết"), 0, 0);
        root.SetColumnSpan(root.Controls[0], 2);
        root.Controls.Add(HotelUi.FieldLabel("Mã CT phiếu thuê"), 0, 1);
        root.Controls.Add(new Label { Text = id, AutoSize = true, Anchor = AnchorStyles.Left }, 1, 1);
        root.Controls.Add(HotelUi.FieldLabel("Trạng thái"), 0, 2);
        root.Controls.Add(_status, 1, 2);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(8, 8, 16, 8), WrapContents = false };
        var cancel = HotelUi.Button("Thoát");
        _save.Click += (_, _) => Save();
        cancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        buttons.Controls.AddRange([cancel, _save]);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(0) };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
        layout.Controls.Add(root, 0, 0);
        layout.Controls.Add(buttons, 0, 1);
        Controls.Add(layout);
        Load += (_, _) => LoadData();
    }

    private void LoadData()
    {
        try
        {
            using var db = new HotelManagementContext();
            var c = db.Ctdps.AsNoTracking().FirstOrDefault(x => x.MaCtdp == _id && x.DaXoa != true);
            if (c is null) { MessageBox.Show("Không tìm thấy chi tiết phiếu thuê."); Close(); return; }

            if (c.TrangThai is "Đã đặt" or "Đang thuê")
            {
                _status.SelectedItem = "Đang thuê";
                return;
            }

            // Keep the database's terminal/legacy state visible and prevent the old value
            // "Đã xong" from being mistaken for the first editable status in the combo.
            _status.Items.Insert(0, c.TrangThai);
            _status.SelectedIndex = 0;
            _status.Enabled = false;
            _save.Enabled = false;
        }
        catch (Exception ex) { MessageBox.Show("Lỗi tải chi tiết: " + ex.GetBaseException().Message); Close(); }
    }

    private void Save()
    {
        if (_status.SelectedItem is not string value) return;
        try
        {
            using var db = new HotelManagementContext();
            var c = db.Ctdps.FirstOrDefault(x => x.MaCtdp == _id && x.DaXoa != true);
            if (c is null) throw new InvalidOperationException("Không tìm thấy chi tiết phiếu thuê.");
            if (c.TrangThai != "Đã đặt" && c.TrangThai != "Đang thuê")
                throw new InvalidOperationException("Chỉ có thể cập nhật chi tiết đang ở trạng thái Đã đặt hoặc Đang thuê.");
            if (c.TrangThai == value) { MessageBox.Show("Trạng thái không thay đổi."); return; }

            c.TrangThai = value;
            var room = db.Phongs.FirstOrDefault(x => x.MaPh == c.MaPh);
            if (room is not null && value == "Đang thuê") room.Ttph = "Đang thuê";
            if (room is not null && value == "Đã thanh toán")
            {
                var another = db.Ctdps.Any(x => x.MaPh == c.MaPh && x.MaCtdp != c.MaCtdp && x.DaXoa != true &&
                    (x.TrangThai == "Đã đặt" || x.TrangThai == "Đang thuê"));
                if (!another && (room.Ttph == "Đang thuê" || room.Ttph == "Đã đặt")) room.Ttph = "Bình thường";
            }
            db.SaveChanges();
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex) { MessageBox.Show("Lỗi cập nhật: " + ex.GetBaseException().Message); }
    }
}
