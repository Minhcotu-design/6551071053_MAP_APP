using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.EntityFrameworkCore;
using QL_KhachSan.Models;
using System.Globalization;
using System.IO;
using PdfFont = iTextSharp.text.Font;
using PdfParagraph = iTextSharp.text.Paragraph;
using PdfRectangle = iTextSharp.text.Rectangle;

namespace QL_KhachSan.HoaDon;

internal sealed class FormDanhSachHoaDon : Form
{
    private readonly DataGridView _grid = HotelUi.Grid();
    private readonly TextBox _search = HotelUi.TextBox("Tìm mã hóa đơn, khách hàng...");
    private readonly DateTimePicker _date = new() { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false, Width = 145 };
    private sealed record Row(string MaHD, DateTime? NgHD, string TenKH, string TenNV, string MaCTDP, decimal TriGia, string TrangThai);
    private List<Row> _rows = [];
    public FormDanhSachHoaDon()
    {
        Text = "Danh sách hóa đơn"; BackColor = HotelUi.Background; _grid.AutoGenerateColumns = false;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 1, RowCount = 3 };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48)); root.RowStyles.Add(new RowStyle(SizeType.AutoSize)); root.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); root.Controls.Add(HotelUi.Heading("Quản lý hóa đơn"), 0, 0);
        var tools = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true }; _search.Width = 280; var export = HotelUi.Button("Xuất Excel"); export.Click += (_, _) => Export(); _search.TextChanged += (_, _) => Filter(); _date.ValueChanged += (_, _) => Filter(); tools.Controls.AddRange([_search, new Label { Text = "Ngày lập:", AutoSize = true, Margin = new Padding(8, 13, 2, 0) }, _date, export]); root.Controls.Add(tools, 0, 1);
        Add("MaHD", "Mã hóa đơn", "MaHD", 14); Add("NgHD", "Ngày lập", "NgHD", 18, "dd/MM/yyyy HH:mm"); Add("TenKH", "Khách hàng", "TenKH", 20); Add("TenNV", "Nhân viên", "TenNV", 18); Add("MaCTDP", "Mã CT thuê", "MaCTDP", 13); Add("TriGia", "Trị giá", "TriGia", 17, "N0"); Add("TrangThai", "Trạng thái", "TrangThai", 15);
        _grid.Columns.Add(new DataGridViewButtonColumn { Name = "ChiTiet", HeaderText = "Chi tiết", Text = "Xem / PDF", UseColumnTextForButtonValue = true, FillWeight = 18, MinimumWidth = 90 }); _grid.CellContentClick += (_, e) => { if (e.RowIndex < 0 || _grid.Columns[e.ColumnIndex].Name != "ChiTiet") return; var code = _grid.Rows[e.RowIndex].Cells["MaHD"].Value?.ToString(); if (string.IsNullOrWhiteSpace(code)) return; using var f = new FormHoaDon(code); f.ShowDialog(this); LoadRows(); };
        root.Controls.Add(_grid, 0, 2); Controls.Add(root); Load += (_, _) => LoadRows();
    }
    private void Add(string n, string h, string p, float w, string? f = null) { var c = new DataGridViewTextBoxColumn { Name = n, HeaderText = h, DataPropertyName = p, FillWeight = w }; if (f is not null) c.DefaultCellStyle.Format = f; _grid.Columns.Add(c); }
    private void LoadRows() { try { using var db = new HotelManagementContext(); _rows = db.HoaDons.AsNoTracking().Where(h => h.DaXoa != true).OrderByDescending(h => h.NgHd).Select(h => new Row(h.MaHd, h.NgHd, h.MaCtdpNavigation.MaPtNavigation.MaKhNavigation.TenKh, h.MaNvNavigation.TenNv, h.MaCtdp, h.TriGia ?? 0, h.TrangThai)).ToList(); Filter(); } catch (Exception ex) { MessageBox.Show("Lỗi tải danh sách hóa đơn: " + ex.GetBaseException().Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); } }
    private void Filter() { var q = _search.Text.Trim(); var result = _rows.Where(x => (q.Length == 0 || x.MaHD.Contains(q, StringComparison.OrdinalIgnoreCase) || x.TenKH.Contains(q, StringComparison.OrdinalIgnoreCase) || x.MaCTDP.Contains(q, StringComparison.OrdinalIgnoreCase)) && (!_date.Checked || x.NgHD?.Date == _date.Value.Date)).ToList(); _grid.DataSource = null; _grid.DataSource = result; }
    private void Export() { if (_grid.Rows.Count == 0) { MessageBox.Show("Không có dữ liệu để xuất."); return; } using var save = new SaveFileDialog { Filter = "Excel Workbook (*.xlsx)|*.xlsx", FileName = "DanhSachHoaDon.xlsx" }; if (save.ShowDialog(this) != DialogResult.OK) return; try { using var wb = new XLWorkbook(); var ws = wb.Worksheets.Add("Danh sách hóa đơn"); for (var c = 0; c < 7; c++) { ws.Cell(1, c + 1).Value = _grid.Columns[c].HeaderText; ws.Cell(1, c + 1).Style.Font.Bold = true; } for (var r = 0; r < _grid.Rows.Count; r++) for (var c = 0; c < 7; c++) ws.Cell(r + 2, c + 1).Value = _grid.Rows[r].Cells[c].Value?.ToString() ?? ""; ws.Columns().AdjustToContents(); wb.SaveAs(save.FileName); MessageBox.Show("Xuất dữ liệu thành công."); } catch (Exception ex) { MessageBox.Show("Lỗi xuất Excel: " + ex.Message); } }
}
