using System.Data;
using System.Globalization;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QL_KhachSan.KhachHang;
using QL_KhachSan.NhanVien;
using QL_KhachSan.Models;
using QL_KhachSan.TienNghi;

namespace QL_KhachSan.DichVu;

internal enum CatalogKind { DichVu, TienNghi, KhachHang, NhanVien }

internal abstract class CatalogListForm : Form
{
    private readonly CatalogKind _kind;
    private readonly DataGridView _grid = HotelUi.Grid();
    private readonly TextBox _search = HotelUi.TextBox("Tìm theo mã hoặc tên...");
    private readonly DataTable _table = new();
    private readonly (string Key, string Header)[] _columns;
    private readonly string _title;

    protected CatalogListForm(CatalogKind kind, string title, params (string Key, string Header)[] columns)
    {
        _kind = kind; _title = title; _columns = columns;
        Text = title; BackColor = HotelUi.Background;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(12) };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48)); root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54)); root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(HotelUi.Heading(title), 0, 0);
        var tools = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false }; _search.Width = 300;
        var add = HotelUi.Button("Thêm mới", true); var edit = HotelUi.Button("Cập nhật"); var delete = HotelUi.Button("Xóa"); var export = HotelUi.Button("Xuất Excel");
        add.Click += (_, _) => EditRecord(null); edit.Click += (_, _) => EditSelected(); delete.Click += (_, _) => DeleteSelected(); export.Click += (_, _) => ExportRows();
        tools.Controls.AddRange([_search, add, edit, delete, export]); root.Controls.Add(tools, 0, 1); root.Controls.Add(_grid, 0, 2); Controls.Add(root);
        foreach (var c in columns) { _table.Columns.Add(c.Key); _grid.Columns.Add(c.Key, c.Header); }
        _grid.AutoGenerateColumns = false;
        for (int i = 0; i < columns.Length; i++) _grid.Columns[i].DataPropertyName = columns[i].Key;
        _grid.DataSource = _table; Load += (_, _) => LoadRows(); _search.TextChanged += (_, _) => FilterRows();
        _grid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) EditSelected(); };
    }

    private void LoadRows()
    {
        try
        {
            using var db = new HotelManagementContext(); _table.Rows.Clear();
            switch (_kind)
            {
                case CatalogKind.DichVu: foreach (var x in db.DichVus.AsNoTracking().Where(x => x.DaXoa != true).OrderBy(x => x.MaDv)) _table.Rows.Add(x.MaDv, x.TenDv, x.DonGia, x.SlconLai is null or < 0 ? "Không giới hạn" : x.SlconLai.Value.ToString(CultureInfo.CurrentCulture), x.LoaiDv); break;
                case CatalogKind.TienNghi: foreach (var x in db.TienNghis.AsNoTracking().Where(x => x.DaXoa != true).OrderBy(x => x.MaTn)) _table.Rows.Add(x.MaTn, x.TenTn); break;
                case CatalogKind.KhachHang: foreach (var x in db.KhachHangs.AsNoTracking().Where(x => x.DaXoa != true).OrderBy(x => x.MaKh)) _table.Rows.Add(x.MaKh, x.TenKh, x.Sdt, x.CccdPassport, x.QuocTich, x.GioiTinh); break;
                case CatalogKind.NhanVien: foreach (var x in db.NhanViens.AsNoTracking().Where(x => x.DaXoa != true).OrderBy(x => x.MaNv)) _table.Rows.Add(x.MaNv, x.TenNv, x.ChucVu, x.Sdt, x.Cccd, x.NgaySinh.ToString("yyyy-MM-dd"), x.GioiTinh, x.Email); break;
            }
            FilterRows();
        }
        catch (Exception ex) { MessageBox.Show("Lỗi khi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private string? SelectedKey() => _grid.CurrentRow?.Cells[0].Value?.ToString();
    private void EditSelected() { if (SelectedKey() is string key) EditRecord(key); }

    private void EditRecord(string? key)
    {
        try
        {
            using var db = new HotelManagementContext(); var values = GetValues(db, key);
            using var editor = CreateEditor(key, values);
            if (editor.ShowDialog(this) != DialogResult.OK) return;
            SaveRecord(db, key, editor.Values); db.SaveChanges(); LoadRows();
            MessageBox.Show(key is null ? "Thêm mới thành công!" : "Cập nhật thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (DbUpdateException ex) { MessageBox.Show("Không thể lưu dữ liệu. Kiểm tra mã và thông tin đã nhập.\n" + (ex.InnerException?.Message ?? ex.Message), "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        catch (Exception ex) { MessageBox.Show("Lỗi khi lưu dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private RecordEditorForm CreateEditor(string? key, Dictionary<string, FieldValue> fields) => (_kind, key is not null) switch
    {
        (CatalogKind.DichVu, false) => new FormThemDichVu(fields),
        (CatalogKind.DichVu, true) => new FormSuaDichVu(fields),
        (CatalogKind.TienNghi, false) => new FormThemTienNghi(fields),
        (CatalogKind.TienNghi, true) => new FormSuaTienNghi(fields),
        (CatalogKind.KhachHang, false) => new FormThemKhachHang(fields),
        (CatalogKind.KhachHang, true) => new FormSuaKhachHang(fields),
        (CatalogKind.NhanVien, false) => new FormThemNhanVien(fields),
        (CatalogKind.NhanVien, true) => new FormSuaNhanVien(fields),
        _ => throw new InvalidOperationException("Không xác định được biểu mẫu cần mở.")
    };

    private Dictionary<string, FieldValue> GetValues(HotelManagementContext db, string? key)
    {
        var row = key is null ? null : _table.Rows.Cast<DataRow>().FirstOrDefault(r => r[0]?.ToString() == key);
        string V(int i) => row?[i]?.ToString() ?? "";
        var f = new Dictionary<string, FieldValue>();
        void Add(string id, string label, string value = "", string[]? choices = null, bool readOnly = false) => f[id] = new(label, value, choices, readOnly);
        switch (_kind)
        {
            case CatalogKind.DichVu:
                var quantity = key is null ? null : db.DichVus.AsNoTracking().Where(x => x.MaDv == key).Select(x => x.SlconLai).FirstOrDefault();
                Add("Ma", "Mã dịch vụ", V(0), readOnly: key != null); Add("Ten", "Tên dịch vụ", V(1)); Add("Gia", "Đơn giá", V(2));
                Add("SL", "Số lượng còn lại", quantity is null or < 0 ? "" : quantity.Value.ToString(CultureInfo.CurrentCulture)); Add("Loai", "Loại dịch vụ", V(4)); break;
            case CatalogKind.TienNghi: Add("Ma", "Mã tiện nghi", V(0), readOnly: key != null); Add("Ten", "Tên tiện nghi", V(1)); break;
            case CatalogKind.KhachHang: Add("Ma", "Mã khách hàng", V(0), readOnly: key != null); Add("Ten", "Họ và tên", V(1)); Add("Sdt", "Số điện thoại", V(2)); Add("CCCD", "CCCD/Passport", V(3)); Add("QuocTich", "Quốc tịch", V(4)); Add("GioiTinh", "Giới tính", V(5), ["Nam", "Nữ"]); break;
            case CatalogKind.NhanVien:
                var nv = key is null ? null : db.NhanViens.AsNoTracking().FirstOrDefault(x => x.MaNv == key);
                Add("Ma", "Mã nhân viên", V(0), readOnly: key != null); Add("Ten", "Họ và tên", V(1)); Add("ChucVu", "Chức vụ", V(2)); Add("Sdt", "Số điện thoại", V(3)); Add("CCCD", "CCCD", V(4)); Add("NgaySinh", "Ngày sinh (yyyy-MM-dd)", V(5)); Add("GioiTinh", "Giới tính", V(6), ["Nam", "Nữ"]); Add("Email", "Email", V(7)); Add("DiaChi", "Địa chỉ", nv?.DiaChi ?? ""); Add("Luong", "Lương", nv?.Luong.ToString(CultureInfo.CurrentCulture) ?? "0"); break;
        }
        return f;
    }

    private void SaveRecord(HotelManagementContext db, string? key, Dictionary<string, string> v)
    {
        string G(string k) => v.GetValueOrDefault(k, "").Trim();
        switch (_kind)
        {
            case CatalogKind.DichVu:
            {
                if (G("Ma").Length == 0 || G("Ten").Length == 0 || G("Loai").Length == 0) throw new InvalidOperationException("Vui lòng nhập đầy đủ thông tin.");
                var enteredCode = G("Ma");
                if (key is null && db.DichVus.Any(x => x.MaDv == enteredCode && x.DaXoa != true)) throw new InvalidOperationException("Mã dịch vụ đã tồn tại.");
                if (!decimal.TryParse(G("Gia"), NumberStyles.Number, CultureInfo.CurrentCulture, out var price) || price < 0) throw new InvalidOperationException("Đơn giá không hợp lệ.");
                var quantityText = G("SL");
                var qty = -1;
                if (quantityText.Length > 0 && (!int.TryParse(quantityText, out qty) || qty < 0)) throw new InvalidOperationException("Số lượng không hợp lệ.");
                var x = key is null ? db.DichVus.FirstOrDefault(a => a.MaDv == enteredCode) : db.DichVus.FirstOrDefault(a => a.MaDv == key);
                if (x is null) { x = new QL_KhachSan.Models.DichVu { MaDv = enteredCode }; db.DichVus.Add(x); }
                x.TenDv = G("Ten"); x.DonGia = price; x.SlconLai = qty; x.LoaiDv = G("Loai"); x.DaXoa = false; break;
            }
            case CatalogKind.TienNghi:
            {
                if (G("Ma").Length == 0 || G("Ten").Length == 0) throw new InvalidOperationException("Vui lòng nhập đầy đủ thông tin.");
                var enteredCode = G("Ma");
                if (key is null && db.TienNghis.Any(x => x.MaTn == enteredCode && x.DaXoa != true)) throw new InvalidOperationException("Mã tiện nghi đã tồn tại.");
                var x = key is null ? db.TienNghis.FirstOrDefault(a => a.MaTn == enteredCode) : db.TienNghis.FirstOrDefault(a => a.MaTn == key);
                if (x is null) { x = new QL_KhachSan.Models.TienNghi { MaTn = enteredCode }; db.TienNghis.Add(x); } x.TenTn = G("Ten"); x.DaXoa = false; break;
            }
            case CatalogKind.KhachHang:
            {
                if (new[] { "Ma", "Ten", "Sdt", "CCCD", "QuocTich", "GioiTinh" }.Any(x => G(x).Length == 0)) throw new InvalidOperationException("Vui lòng nhập đầy đủ thông tin.");
                var enteredCode = G("Ma");
                if (key is null && db.KhachHangs.Any(x => x.MaKh == enteredCode && x.DaXoa != true)) throw new InvalidOperationException("Mã khách hàng đã tồn tại.");
                var x = key is null ? db.KhachHangs.FirstOrDefault(a => a.MaKh == enteredCode) : db.KhachHangs.FirstOrDefault(a => a.MaKh == key);
                if (x is null) { x = new QL_KhachSan.Models.KhachHang { MaKh = enteredCode }; db.KhachHangs.Add(x); } x.TenKh = G("Ten"); x.Sdt = G("Sdt"); x.CccdPassport = G("CCCD"); x.QuocTich = G("QuocTich"); x.GioiTinh = G("GioiTinh"); x.DaXoa = false; break;
            }
            case CatalogKind.NhanVien:
            {
                if (new[] { "Ma", "Ten", "ChucVu", "Sdt", "CCCD", "NgaySinh", "GioiTinh", "DiaChi", "Luong" }.Any(x => G(x).Length == 0)) throw new InvalidOperationException("Vui lòng nhập đầy đủ thông tin.");
                if (!DateOnly.TryParse(G("NgaySinh"), CultureInfo.CurrentCulture, out var dob)) throw new InvalidOperationException("Ngày sinh không hợp lệ.");
                if (!decimal.TryParse(G("Luong"), NumberStyles.Number, CultureInfo.CurrentCulture, out var salary) || salary < 0) throw new InvalidOperationException("Lương không hợp lệ.");
                var enteredCode = G("Ma");
                if (key is null && db.NhanViens.Any(x => x.MaNv == enteredCode && x.DaXoa != true)) throw new InvalidOperationException("Mã nhân viên đã tồn tại.");
                var x = key is null ? db.NhanViens.FirstOrDefault(a => a.MaNv == enteredCode) : db.NhanViens.FirstOrDefault(a => a.MaNv == key);
                if (x is null) { x = new QL_KhachSan.Models.NhanVien { MaNv = enteredCode }; db.NhanViens.Add(x); } x.TenNv = G("Ten"); x.ChucVu = G("ChucVu"); x.Sdt = G("Sdt"); x.Cccd = G("CCCD"); x.NgaySinh = dob; x.GioiTinh = G("GioiTinh"); x.Email = G("Email"); x.DiaChi = G("DiaChi"); x.Luong = salary; x.DaXoa = false; break;
            }
        }
    }

    private void DeleteSelected()
    {
        var key = SelectedKey(); if (key is null) return;
        if (MessageBox.Show($"Bạn có chắc muốn xóa {key}?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        try
        {
            using var db = new HotelManagementContext();
            switch (_kind)
            {
                case CatalogKind.DichVu: var dv = db.DichVus.FirstOrDefault(x => x.MaDv == key); if (dv != null) dv.DaXoa = true; break;
                case CatalogKind.TienNghi: var tn = db.TienNghis.FirstOrDefault(x => x.MaTn == key); if (tn != null) tn.DaXoa = true; break;
                case CatalogKind.KhachHang: var kh = db.KhachHangs.FirstOrDefault(x => x.MaKh == key); if (kh != null) kh.DaXoa = true; break;
                case CatalogKind.NhanVien: var nv = db.NhanViens.FirstOrDefault(x => x.MaNv == key); if (nv != null) nv.DaXoa = true; break;
            }
            db.SaveChanges(); LoadRows();
        }
        catch (Exception ex) { MessageBox.Show("Lỗi khi xóa dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void FilterRows()
    {
        var term = _search.Text.Trim(); _grid.CurrentCell = null;
        foreach (DataGridViewRow row in _grid.Rows) { var code = row.Cells[0].Value?.ToString() ?? ""; var name = row.Cells.Count > 1 ? row.Cells[1].Value?.ToString() ?? "" : ""; row.Visible = term.Length == 0 || code.Contains(term, StringComparison.OrdinalIgnoreCase) || name.Contains(term, StringComparison.OrdinalIgnoreCase); }
    }

    private void ExportRows()
    {
        using var dialog = new SaveFileDialog { Filter = "Excel Workbook (*.xlsx)|*.xlsx", FileName = $"DanhSach_{_kind}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try { using var book = new XLWorkbook(); var sheet = book.Worksheets.Add("Danh sách"); for (var c = 0; c < _columns.Length; c++) { sheet.Cell(1, c + 1).Value = _columns[c].Header; sheet.Cell(1, c + 1).Style.Font.Bold = true; } var rows = _grid.Rows.Cast<DataGridViewRow>().Where(r => r.Visible && !r.IsNewRow).ToList(); for (var r = 0; r < rows.Count; r++) for (var c = 0; c < _columns.Length; c++) sheet.Cell(r + 2, c + 1).Value = rows[r].Cells[c].Value?.ToString() ?? ""; sheet.Columns().AdjustToContents(); book.SaveAs(dialog.FileName); MessageBox.Show("Xuất Excel thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        catch (Exception ex) { MessageBox.Show("Lỗi xuất Excel: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}

internal sealed record FieldValue(string Label, string Value = "", string[]? Choices = null, bool ReadOnly = false);

internal class RecordEditorForm : Form
{
    private readonly Dictionary<string, Control> _controls = new();
    public Dictionary<string, string> Values => _controls.ToDictionary(x => x.Key, x => x.Value.Text);
    public RecordEditorForm(string title, Dictionary<string, FieldValue> fields)
    {
        Text = title; StartPosition = FormStartPosition.CenterParent; FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = MinimizeBox = false; ClientSize = new Size(550, Math.Max(270, 110 + fields.Count * 48));
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoScroll = true, Padding = new Padding(18) }; root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170)); root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); int row = 0;
        foreach (var pair in fields)
        {
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44)); root.Controls.Add(HotelUi.FieldLabel(pair.Value.Label), 0, row);
            Control input;
            if (pair.Value.Choices is { Length: > 0 }) { var combo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10.5F), Dock = DockStyle.Fill }; combo.Items.AddRange(pair.Value.Choices); combo.SelectedItem = pair.Value.Value; if (combo.SelectedIndex < 0) combo.SelectedIndex = 0; input = combo; }
            else input = HotelUi.TextBox(pair.Key == "SL" ? "Để trống = không giới hạn" : "");
            input.Text = pair.Value.Value; input.Enabled = !pair.Value.ReadOnly; input.Dock = DockStyle.Fill; root.Controls.Add(input, 1, row++); _controls[pair.Key] = input;
        }
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft }; var save = HotelUi.Button("Lưu", true); var cancel = HotelUi.Button("Hủy");
        save.Click += (_, _) => { if (_controls.Any(x => x.Key != "SL" && string.IsNullOrWhiteSpace(x.Value.Text))) { MessageBox.Show("Vui lòng nhập đầy đủ thông tin.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; } DialogResult = DialogResult.OK; Close(); }; cancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); }; buttons.Controls.AddRange([cancel, save]); root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48)); root.Controls.Add(buttons, 0, row); root.SetColumnSpan(buttons, 2); Controls.Add(root); AcceptButton = save; CancelButton = cancel;
    }
}

