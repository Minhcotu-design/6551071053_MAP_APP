using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QL_KhachSan.DangNhap;
using QL_KhachSan.Models;
using QL_KhachSan.SoDoPhong;
using System.Globalization;

namespace QL_KhachSan.PhieuThue;

internal sealed class FormThemDichVuPhongDaDung : Form
{
    private sealed record Option(string MaDV, string TenDV, decimal DonGia, int? ConLai) { public override string ToString() => TenDV; }
    private readonly string _id; private readonly ComboBox _service = new() { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill }; private readonly NumericUpDown _qty = new() { Minimum = 1, Maximum = 100000, Value = 1, Width = 140 }; private readonly TextBox _code = new() { ReadOnly = true, Dock = DockStyle.Fill }; private readonly TextBox _price = new() { ReadOnly = true, Dock = DockStyle.Fill }; private readonly Label _total = new() { AutoSize = true, Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.DarkRed };
    public FormThemDichVuPhongDaDung(string id, string room) { _id = id; Text = "Thêm dịch vụ phòng đã dùng"; StartPosition = FormStartPosition.CenterParent; FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = MinimizeBox = false; ClientSize = new Size(640, 430); var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18), ColumnCount = 2, RowCount = 8 }; root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 175)); root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); root.Controls.Add(HotelUi.Heading("Thêm dịch vụ phòng"), 0, 0); root.SetColumnSpan(root.Controls[0], 2); AddRow(root, 1, "Mã CT thuê", new Label { Text = id, AutoSize = true }); AddRow(root, 2, "Số phòng", new Label { Text = room, AutoSize = true }); AddRow(root, 3, "Mã dịch vụ", _code); AddRow(root, 4, "Tên dịch vụ", _service); AddRow(root, 5, "Số lượng", _qty); AddRow(root, 6, "Đơn giá", _price); AddRow(root, 7, "Thành tiền", _total); var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 54, FlowDirection = FlowDirection.RightToLeft }; var save = HotelUi.Button("Thêm dịch vụ", true); var cancel = HotelUi.Button("Thoát"); save.Click += (_, _) => Save(); cancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); }; Controls.Add(root); Controls.Add(buttons); buttons.Controls.AddRange([cancel, save]); _service.SelectedIndexChanged += (_, _) => UpdateTotal(); _qty.ValueChanged += (_, _) => UpdateTotal(); Load += (_, _) => LoadData(); }
    private static void AddRow(TableLayoutPanel p, int r, string title, Control c) { p.RowStyles.Add(new RowStyle(SizeType.Absolute, 48)); p.Controls.Add(HotelUi.FieldLabel(title), 0, r); c.Margin = new Padding(5); p.Controls.Add(c, 1, r); }
    private void LoadData() { try { using var db = new HotelManagementContext(); var c = db.Ctdps.AsNoTracking().FirstOrDefault(x => x.MaCtdp == _id && x.DaXoa != true); if (c is null || c.TrangThai != "Đang thuê") { MessageBox.Show("Chỉ có thể thêm dịch vụ cho phòng đang thuê."); Close(); return; } var list = db.DichVus.AsNoTracking().Where(x => x.DaXoa != true).OrderBy(x => x.TenDv).Select(x => new Option(x.MaDv, x.TenDv, x.DonGia, x.SlconLai)).ToList(); _service.DataSource = list; _service.SelectedIndex = list.Count > 0 ? 0 : -1; UpdateTotal(); } catch (Exception ex) { MessageBox.Show("Lỗi tải danh sách dịch vụ: " + ex.GetBaseException().Message); Close(); } }
    private void UpdateTotal() { if (_service.SelectedItem is Option x) { _code.Text = x.MaDV; _price.Text = x.DonGia.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")); _total.Text = $"Thành tiền: {_qty.Value * x.DonGia:N0}"; } else { _code.Clear(); _price.Clear(); _total.Text = "Thành tiền: 0"; } }
    private void Save()
    {
        if (_service.SelectedItem is not Option option) { MessageBox.Show("Vui lòng chọn dịch vụ."); return; }
        try
        {
            using var db = new HotelManagementContext();
            using var tx = db.Database.BeginTransaction();
            var c = db.Ctdps.FirstOrDefault(x => x.MaCtdp == _id && x.DaXoa != true);
            if (c is null || c.TrangThai != "Đang thuê") throw new InvalidOperationException("Phòng không còn ở trạng thái đang thuê.");
            var d = db.DichVus.FirstOrDefault(x => x.MaDv == option.MaDV && x.DaXoa != true);
            if (d is null) throw new InvalidOperationException("Dịch vụ không còn tồn tại.");
            var quantity = (int)_qty.Value;

            // NULL means unlimited inventory in this database. Any numeric quantity, including
            // a legacy negative value, is limited and must be decremented atomically.
            if (d.SlconLai.HasValue)
            {
                var updated = db.DichVus
                    .Where(x => x.MaDv == d.MaDv && x.DaXoa != true && x.SlconLai >= quantity)
                    .ExecuteUpdate(s => s.SetProperty(x => x.SlconLai, x => x.SlconLai - quantity));
                if (updated == 0) throw new InvalidOperationException($"Dịch vụ chỉ còn {Math.Max(0, d.SlconLai.Value)} phần.");
            }

            var old = db.Ctdvs.FirstOrDefault(x => x.MaCtdp == _id && x.MaDv == d.MaDv && x.DonGia == d.DonGia);
            if (old is null) db.Ctdvs.Add(new Ctdv { MaCtdp = _id, MaDv = d.MaDv, DonGia = d.DonGia, Sl = quantity, ThanhTien = d.DonGia * quantity, DaXoa = false });
            else { old.Sl += quantity; old.ThanhTien = old.Sl * old.DonGia; old.DaXoa = false; }
            db.SaveChanges();
            tx.Commit();
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex) { MessageBox.Show("Thêm dịch vụ thất bại: " + ex.GetBaseException().Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}
