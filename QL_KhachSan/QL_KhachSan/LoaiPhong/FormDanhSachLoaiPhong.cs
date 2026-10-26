using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QL_KhachSan.Models;

namespace QL_KhachSan.LoaiPhong;

public sealed class FormDanhSachLoaiPhong : Form
{
    private readonly DataGridView _grid = HotelUi.Grid();
    private readonly TextBox _txtTimKiem = HotelUi.TextBox("Tìm mã hoặc tên loại phòng...");

    public FormDanhSachLoaiPhong()
    {
        Text = "Quản lý thông tin loại phòng";
        BackColor = HotelUi.Background;
        BuildLayout();
        Load += (_, _) => { CauHinhLuoi(); LoadLoaiPhong(); };
        _txtTimKiem.TextChanged += (_, _) => LocTheoTuKhoa();
        _grid.CellClick += Grid_CellClick;
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(12) };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(HotelUi.Heading("Quản lý thông tin loại phòng"), 0, 0);
        var tools = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        _txtTimKiem.Width = 320;
        var export = HotelUi.Button("Xuất Excel");
        var add = HotelUi.Button("Thêm mới", true);
        export.Click += (_, _) => XuatExcel();
        add.Click += (_, _) => ThemLoaiPhong();
        tools.Controls.AddRange([_txtTimKiem, export, add]);
        root.Controls.Add(tools, 0, 1);
        root.Controls.Add(_grid, 0, 2);
        Controls.Add(root);
    }

    private void CauHinhLuoi()
    {
        if (_grid.Columns.Count > 0)
            return;
        _grid.Columns.Add(ImageColumn("colIcon", "", "Giuong", 52));
        _grid.Columns.Add("MaLPH", "Mã loại");
        _grid.Columns.Add("TenLPH", "Tên loại");
        _grid.Columns.Add("SoGiuong", "Số giường");
        _grid.Columns.Add("SoNguoiToiDa", "Số người tối đa");
        _grid.Columns.Add("GiaNgay", "Giá ngày");
        _grid.Columns.Add("GiaGio", "Giá giờ");
        _grid.Columns["GiaNgay"]!.DefaultCellStyle.Format = "N0";
        _grid.Columns["GiaGio"]!.DefaultCellStyle.Format = "N0";
        _grid.Columns["GiaNgay"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        _grid.Columns["GiaGio"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        _grid.Columns.Add(ImageColumn("colTienNghi", "Tiện nghi", "details", 60));
        _grid.Columns.Add(ImageColumn("colSua", "Cập nhật", "edit", 70));
    }

    private static DataGridViewImageColumn ImageColumn(string name, string header, string image, int width) => new()
    {
        Name = name,
        HeaderText = header,
        Image = AppImages.Get(image),
        ImageLayout = DataGridViewImageCellLayout.Zoom,
        AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
        Width = width,
        Resizable = DataGridViewTriState.False,
        SortMode = DataGridViewColumnSortMode.NotSortable
    };

    private void LoadLoaiPhong()
    {
        try
        {
            using var db = new HotelManagementContext();
            var items = db.LoaiPhongs.AsNoTracking().OrderBy(x => x.MaLph)
                .Select(x => new { x.MaLph, x.TenLph, x.SoGiuong, x.SoNguoiToiDa, x.GiaNgay, x.GiaGio }).ToList();
            _grid.Rows.Clear();
            foreach (var item in items)
                _grid.Rows.Add(AppImages.Get("Giuong"), item.MaLph, item.TenLph, item.SoGiuong,
                    item.SoNguoiToiDa, item.GiaNgay, item.GiaGio, AppImages.Get("details"), AppImages.Get("edit"));
            LocTheoTuKhoa();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi khi tải loại phòng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LocTheoTuKhoa()
    {
        var keyword = _txtTimKiem.Text.Trim();
        _grid.CurrentCell = null;
        foreach (DataGridViewRow row in _grid.Rows)
        {
            var code = row.Cells["MaLPH"].Value?.ToString() ?? string.Empty;
            var name = row.Cells["TenLPH"].Value?.ToString() ?? string.Empty;
            row.Visible = keyword.Length == 0
                || code.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || name.Contains(keyword, StringComparison.OrdinalIgnoreCase);
        }
    }

    private void ThemLoaiPhong()
    {
        using var form = new FormThemLoaiPhong();
        if (form.ShowDialog(this) == DialogResult.OK)
            LoadLoaiPhong();
    }

    private void Grid_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0)
            return;
        var row = _grid.Rows[e.RowIndex];
        var ma = row.Cells["MaLPH"].Value?.ToString();
        var ten = row.Cells["TenLPH"].Value?.ToString();
        if (string.IsNullOrWhiteSpace(ma))
            return;
        var column = _grid.Columns[e.ColumnIndex].Name;
        if (column == "colSua")
        {
            using var form = new FormSuaLoaiPhong { MaLoaiPhong = ma };
            if (form.ShowDialog(this) == DialogResult.OK)
                LoadLoaiPhong();
        }
        else if (column == "colTienNghi")
        {
            MessageBox.Show($"Chức năng chi tiết tiện nghi của loại phòng {ten} ({ma}) đang được xây dựng.",
                "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void XuatExcel()
    {
        var rows = _grid.Rows.Cast<DataGridViewRow>().Where(r => r.Visible && !r.IsNewRow).ToList();
        if (rows.Count == 0)
        {
            MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        using var dialog = new SaveFileDialog
        {
            Title = "Xuất danh sách loại phòng",
            Filter = "Excel Workbook (*.xlsx)|*.xlsx",
            FileName = $"DanhSachLoaiPhong_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;
        try
        {
            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Danh sách loại phòng");
            string[] columns = ["MaLPH", "TenLPH", "SoGiuong", "SoNguoiToiDa", "GiaNgay", "GiaGio"];
            string[] titles = ["Mã loại", "Tên loại", "Số giường", "Số người tối đa", "Giá ngày", "Giá giờ"];
            for (var c = 0; c < columns.Length; c++)
            {
                sheet.Cell(1, c + 1).Value = titles[c];
                sheet.Cell(1, c + 1).Style.Font.Bold = true;
            }
            for (var r = 0; r < rows.Count; r++)
            {
                for (var c = 0; c < columns.Length; c++)
                {
                    var value = rows[r].Cells[columns[c]].Value;
                    var cell = sheet.Cell(r + 2, c + 1);
                    if (value is decimal amount)
                    {
                        cell.Value = amount;
                        cell.Style.NumberFormat.Format = "#,##0";
                    }
                    else if (value is int number)
                    {
                        cell.Value = number;
                    }
                    else
                    {
                        cell.Value = value?.ToString() ?? string.Empty;
                    }
                }
            }
            sheet.Columns().AdjustToContents();
            workbook.SaveAs(dialog.FileName);
            var answer = MessageBox.Show("Xuất dữ liệu thành công!\nBạn có muốn mở file ngay không?", "Thông báo",
                MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (answer == DialogResult.Yes)
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(dialog.FileName) { UseShellExecute = true });
        }
        catch (IOException)
        {
            MessageBox.Show("Không ghi được file. Có thể file đang được mở trong Excel, hãy đóng lại rồi thử lần nữa.",
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi khi xuất Excel: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
