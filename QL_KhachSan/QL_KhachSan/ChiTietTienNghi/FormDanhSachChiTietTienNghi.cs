using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QL_KhachSan.Models;

namespace QL_KhachSan.ChiTietTienNghi;

internal sealed class FormDanhSachChiTietTienNghi : Form
{
    private readonly DataGridView _grid = HotelUi.Grid();
    private readonly TextBox _search = HotelUi.TextBox("Tìm mã loại phòng hoặc tiện nghi...");
    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public string? MaLoaiPhong { get; set; }
    public FormDanhSachChiTietTienNghi()
    {
        Text = "Quản lý chi tiết tiện nghi"; BackColor = HotelUi.Background;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(12) };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48)); root.RowStyles.Add(new RowStyle(SizeType.AutoSize)); root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(HotelUi.Heading("Quản lý tiện nghi theo loại phòng"), 0, 0);
        var tools = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true, AutoScroll = true }; _search.Width = 260;
        var add = HotelUi.Button("Thêm tiện nghi", true); var edit = HotelUi.Button("Cập nhật số lượng"); var remove = HotelUi.Button("Xóa"); var export = HotelUi.Button("Xuất Excel");
        add.Click += (_, _) => Edit(null); edit.Click += (_, _) => EditSelected(); remove.Click += (_, _) => Remove(); export.Click += (_, _) => Export();
        tools.Controls.AddRange([_search, add, edit, remove, export]); root.Controls.Add(tools, 0, 1); root.Controls.Add(_grid, 0, 2); Controls.Add(root);
        _grid.Columns.Add("MaLPH", "Mã loại phòng"); _grid.Columns.Add("TenLPH", "Loại phòng"); _grid.Columns.Add("MaTN", "Mã tiện nghi"); _grid.Columns.Add("TenTN", "Tên tiện nghi"); _grid.Columns.Add("SL", "Số lượng");
        Load += (_, _) => LoadData(); _search.TextChanged += (_, _) => Filter();
    }
    private (string Type, string Amenity)? Selected
    {
        get
        {
            var row = _grid.SelectedRows.Cast<DataGridViewRow>().FirstOrDefault() ?? _grid.CurrentRow;
            if (row is null || row.IsNewRow || row.Index < 0) return null;
            var type = row.Cells[0].Value?.ToString();
            var amenity = row.Cells[2].Value?.ToString();
            return string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(amenity) ? null : (type, amenity);
        }
    }

    private void EditSelected()
    {
        if (Selected is { } x) Edit(x);
        else MessageBox.Show("Hãy chọn dòng tiện nghi cần cập nhật số lượng.", "Chưa chọn dòng", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    private void LoadData()
    {
        try { using var db = new HotelManagementContext(); _grid.Rows.Clear(); var query = db.Cttns.AsNoTracking().Where(x => x.DaXoa != true); if (!string.IsNullOrWhiteSpace(MaLoaiPhong)) query = query.Where(x => x.MaLph == MaLoaiPhong); var rows = query.Include(x => x.MaLphNavigation).Include(x => x.MaTnNavigation).OrderBy(x => x.MaLph).ThenBy(x => x.MaTn).ToList(); foreach (var x in rows) _grid.Rows.Add(x.MaLph, x.MaLphNavigation.TenLph, x.MaTn, x.MaTnNavigation.TenTn, x.Sl?.ToString() ?? ""); Filter(); }
        catch (Exception ex) { MessageBox.Show("Lỗi tải chi tiết tiện nghi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
    private void Filter() { var q = _search.Text.Trim(); _grid.CurrentCell = null; foreach (DataGridViewRow r in _grid.Rows) r.Visible = q.Length == 0 || string.Join(" ", r.Cells.Cast<DataGridViewCell>().Select(c => c.Value?.ToString())).Contains(q, StringComparison.OrdinalIgnoreCase); }
    private void Edit((string Type, string Amenity)? current)
    {
        try
        {
            using var db = new HotelManagementContext();
            var typeQuery = db.LoaiPhongs.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(MaLoaiPhong)) typeQuery = typeQuery.Where(x => x.MaLph == MaLoaiPhong);
            var types = typeQuery.OrderBy(x => x.MaLph).Select(x => new ChiTietOption(x.MaLph, x.MaLph + " - " + x.TenLph)).ToList();
            var amenities = db.TienNghis.AsNoTracking().Where(x => x.DaXoa != true).OrderBy(x => x.MaTn).Select(x => new ChiTietOption(x.MaTn, x.MaTn + " - " + x.TenTn)).ToList();
            FormChiTietTienNghiEditor dialog;
            if (current is { } selected)
            {
                var old = db.Cttns.FirstOrDefault(x => x.MaLph == selected.Type && x.MaTn == selected.Amenity);
                dialog = new FormSuaChiTietTienNghi(types, amenities, selected.Type, selected.Amenity, old?.Sl ?? 1);
            }
            else dialog = new FormThemChiTietTienNghi(types, amenities);
            using (dialog)
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                var type = dialog.MaLoaiPhong; var amenity = dialog.MaTienNghi;
                if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(amenity))
                {
                    MessageBox.Show("Hãy chọn loại phòng và tiện nghi.", "Thông tin chưa đầy đủ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                var item = db.Cttns.FirstOrDefault(x => x.MaLph == type && x.MaTn == amenity);
                if (current is null && item is { DaXoa: not true }) { MessageBox.Show($"Tiện nghi {amenity} đã được gán cho loại phòng {type}. Hãy chọn Cập nhật số lượng nếu muốn đổi số lượng.", "Đã tồn tại", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (item is null) { item = new Cttn { MaLph = type, MaTn = amenity }; db.Cttns.Add(item); }
                item.Sl = dialog.SoLuong;
                item.DaXoa = false;
                db.SaveChanges();
                var savedQuantity = db.Cttns.AsNoTracking().Where(x => x.MaLph == type && x.MaTn == amenity).Select(x => x.Sl).SingleOrDefault();
                if (savedQuantity != dialog.SoLuong) throw new InvalidOperationException($"Database trả về số lượng {savedQuantity?.ToString() ?? "NULL"} cho {type}/{amenity}, không khớp số vừa nhập {dialog.SoLuong}.");
                _search.Clear();
                LoadData();
                var refreshedRow = _grid.Rows.Cast<DataGridViewRow>().FirstOrDefault(row =>
                    string.Equals(row.Cells[0].Value?.ToString(), type, StringComparison.Ordinal) &&
                    string.Equals(row.Cells[2].Value?.ToString(), amenity, StringComparison.Ordinal));
                if (refreshedRow is not null)
                {
                    refreshedRow.Cells[4].Value = savedQuantity?.ToString() ?? "";
                    refreshedRow.Selected = true;
                    _grid.CurrentCell = refreshedRow.Cells[4];
                }
                var success = current is null
                    ? $"Đã thêm {type}/{amenity}, số lượng {savedQuantity}."
                    : $"Đã cập nhật {type}/{amenity}, số lượng {savedQuantity}.";
                MessageBox.Show(success, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex) { MessageBox.Show("Không lưu được chi tiết tiện nghi:\r\n" + ex.GetBaseException().Message, "Lỗi lưu", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
    private void Remove()
    {
        if (Selected is not { } x) { MessageBox.Show("Hãy chọn dòng tiện nghi cần xóa.", "Chưa chọn dòng", MessageBoxButtons.OK, MessageBoxIcon.Information); return; } if (MessageBox.Show("Xóa tiện nghi khỏi loại phòng đã chọn?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        try { using var db = new HotelManagementContext(); var item = db.Cttns.FirstOrDefault(t => t.MaLph == x.Type && t.MaTn == x.Amenity); if (item != null) { item.DaXoa = true; db.SaveChanges(); } LoadData(); }
        catch (Exception ex) { MessageBox.Show("Lỗi xóa chi tiết tiện nghi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
    private void Export()
    {
        using var s = new SaveFileDialog { Filter = "Excel Workbook (*.xlsx)|*.xlsx", FileName = "ChiTietTienNghi.xlsx" }; if (s.ShowDialog(this) != DialogResult.OK) return;
        try { using var wb = new XLWorkbook(); var sh = wb.Worksheets.Add("Chi tiết tiện nghi"); for (int c = 0; c < _grid.Columns.Count; c++) { sh.Cell(1, c + 1).Value = _grid.Columns[c].HeaderText; sh.Cell(1, c + 1).Style.Font.Bold = true; } int r = 2; foreach (DataGridViewRow row in _grid.Rows.Cast<DataGridViewRow>().Where(x => x.Visible)) { for (int c = 0; c < row.Cells.Count; c++) sh.Cell(r, c + 1).Value = row.Cells[c].Value?.ToString() ?? ""; r++; } sh.Columns().AdjustToContents(); wb.SaveAs(s.FileName); MessageBox.Show("Xuất Excel thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        catch (Exception ex) { MessageBox.Show("Lỗi xuất Excel: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}






