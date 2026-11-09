using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QL_KhachSan.DangNhap;
using QL_KhachSan.Models;
using QL_KhachSan.SoDoPhong;
using System.Globalization;

namespace QL_KhachSan.PhieuThue;

internal sealed class FormDanhSachPhieuThue : Form
{
    private readonly DataGridView _grid = HotelUi.Grid();
    private readonly TextBox _search = HotelUi.TextBox("Tìm mã phiếu hoặc tên khách...");
    private List<Row> _rows = [];
    private sealed record Row(string MaPT, string TenKH, DateTime NgPT, string TenNV);
    public FormDanhSachPhieuThue()
    {
        Text = "Danh sách phiếu thuê"; BackColor = HotelUi.Background; _grid.AutoGenerateColumns = false;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(12) };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48)); root.RowStyles.Add(new RowStyle(SizeType.AutoSize)); root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(HotelUi.Heading("Quản lý phiếu thuê phòng"), 0, 0);
        var tools = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true }; _search.Width = 280;
        var book = HotelUi.Button("Đặt phòng", true); var export = HotelUi.Button("Xuất Excel");
        book.Click += (_, _) => { using var f = new FormDatPhong(); if (f.ShowDialog(this) == DialogResult.OK) LoadRows(); }; export.Click += (_, _) => Export(); _search.TextChanged += (_, _) => Filter();
        tools.Controls.AddRange([_search, book, export]); root.Controls.Add(tools, 0, 1);
        AddText("MaPT", "Mã phiếu thuê", "MaPT", 18); AddText("TenKH", "Tên khách hàng", "TenKH", 28); AddText("NgPT", "Ngày lập phiếu", "NgPT", 22, "dd/MM/yyyy HH:mm"); AddText("TenNV", "Nhân viên lập", "TenNV", 26);
        _grid.Columns.Add(new DataGridViewButtonColumn { Name = "ChiTiet", HeaderText = "Chi tiết", Text = "Xem", UseColumnTextForButtonValue = true, FillWeight = 14 });
        _grid.CellContentClick += (_, e) => { if (e.RowIndex < 0 || _grid.Columns[e.ColumnIndex].Name != "ChiTiet") return; var ma = _grid.Rows[e.RowIndex].Cells["MaPT"].Value?.ToString(); if (!string.IsNullOrWhiteSpace(ma)) { using var f = new FormChiTietPhieuThue(ma); f.ShowDialog(this); LoadRows(); } };
        root.Controls.Add(_grid, 0, 2); Controls.Add(root); Load += (_, _) => LoadRows();
    }
    private void AddText(string name, string title, string prop, float weight, string? format = null) { var c = new DataGridViewTextBoxColumn { Name = name, HeaderText = title, DataPropertyName = prop, FillWeight = weight }; if (format is not null) c.DefaultCellStyle.Format = format; _grid.Columns.Add(c); }
    private void LoadRows() { try { using var db = new HotelManagementContext(); _rows = db.PhieuThues.AsNoTracking().Where(p => p.DaXoa != true).OrderByDescending(p => p.NgPt).Select(p => new Row(p.MaPt, p.MaKhNavigation.TenKh, p.NgPt, p.MaNvNavigation.TenNv)).ToList(); Filter(); } catch (Exception ex) { MessageBox.Show("Lỗi tải danh sách phiếu thuê: " + ex.GetBaseException().Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); } }
    private void Filter() { var q = _search.Text.Trim(); _grid.DataSource = null; _grid.DataSource = _rows.Where(r => q.Length == 0 || r.MaPT.Contains(q, StringComparison.OrdinalIgnoreCase) || r.TenKH.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList(); _grid.ClearSelection(); }
    private void Export() { if (_grid.Rows.Count == 0) { MessageBox.Show("Không có dữ liệu để xuất."); return; } using var save = new SaveFileDialog { Filter = "Excel Workbook (*.xlsx)|*.xlsx", FileName = "DanhSachPhieuThue.xlsx" }; if (save.ShowDialog(this) != DialogResult.OK) return; try { using var wb = new XLWorkbook(); var ws = wb.Worksheets.Add("Danh sách phiếu thuê"); for (var c = 0; c < 4; c++) { ws.Cell(1, c + 1).Value = _grid.Columns[c].HeaderText; ws.Cell(1, c + 1).Style.Font.Bold = true; } for (var r = 0; r < _grid.Rows.Count; r++) for (var c = 0; c < 4; c++) ws.Cell(r + 2, c + 1).Value = _grid.Rows[r].Cells[c].Value?.ToString() ?? ""; ws.Columns().AdjustToContents(); wb.SaveAs(save.FileName); MessageBox.Show("Xuất Excel thành công."); } catch (Exception ex) { MessageBox.Show("Lỗi xuất Excel: " + ex.Message); } }
}
