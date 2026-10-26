using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QL_KhachSan.Models;

namespace QL_KhachSan.Phong;

public sealed class FormDanhSachPhong : Form
{
    private readonly DataGridView _grid = HotelUi.Grid();
    private readonly TextBox _txtTimKiem = HotelUi.TextBox("Tìm theo mã phòng...");

    public FormDanhSachPhong()
    {
        Text = "Quản lý thông tin phòng";
        BackColor = HotelUi.Background;
        BuildLayout();
        Load += (_, _) => LoadPhong();
        _txtTimKiem.TextChanged += (_, _) => LocTheoTuKhoa();
        _grid.CellClick += Grid_CellClick;
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(12) };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(HotelUi.Heading("Quản lý thông tin phòng khách sạn"), 0, 0);

        var tools = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, FlowDirection = FlowDirection.LeftToRight };
        _txtTimKiem.Width = 270;
        var export = HotelUi.Button("Xuất Excel");
        var add = HotelUi.Button("Thêm mới phòng", primary: true);
        export.Click += (_, _) => XuatExcel();
        add.Click += (_, _) => ThemPhong();
        tools.Controls.AddRange([_txtTimKiem, export, add]);
        root.Controls.Add(tools, 0, 1);
        root.Controls.Add(_grid, 0, 2);
        Controls.Add(root);
    }

    private void CauHinhLuoi()
    {
        if (_grid.Columns.Count > 0)
            return;
        _grid.Columns.Add(ImageColumn("colIcon", "", "Phong"));
        _grid.Columns.Add("MaPH", "Mã phòng");
        _grid.Columns.Add("TTPH", "Trạng thái phòng");
        _grid.Columns.Add("TTDD", "Tình trạng dọn dẹp");
        _grid.Columns.Add("TenLPH", "Loại phòng");
        _grid.Columns.Add(ImageColumn("colSua", "Sửa", "edit"));
        _grid.Columns.Add(ImageColumn("colXoa", "Xóa", "delete"));
    }

    private static DataGridViewImageColumn ImageColumn(string name, string title, string image) => new()
    {
        Name = name,
        HeaderText = title,
        Image = AppImages.Get(image),
        ImageLayout = DataGridViewImageCellLayout.Zoom,
        AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
        Width = 50,
        Resizable = DataGridViewTriState.False,
        SortMode = DataGridViewColumnSortMode.NotSortable
    };

    private void LoadPhong()
    {
        try
        {
            CauHinhLuoi();
            using var db = new HotelManagementContext();
            var rooms = db.Phongs.AsNoTracking()
                .Where(p => p.DaXoa != true)
                .OrderBy(p => p.MaPh)
                .Select(p => new { p.MaPh, p.MaLphNavigation.TenLph, p.Ttph, p.Ttdd })
                .ToList();
            _grid.Rows.Clear();
            foreach (var room in rooms)
                _grid.Rows.Add(AppImages.Get("Phong"), room.MaPh, room.Ttph, room.Ttdd, room.TenLph,
                    AppImages.Get("edit"), AppImages.Get("delete"));
            LocTheoTuKhoa();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi khi tải phòng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LocTheoTuKhoa()
    {
        var keyword = _txtTimKiem.Text.Trim();
        _grid.CurrentCell = null;
        foreach (DataGridViewRow row in _grid.Rows)
        {
            var ma = row.Cells["MaPH"].Value?.ToString() ?? string.Empty;
            row.Visible = keyword.Length == 0 || ma.Contains(keyword, StringComparison.OrdinalIgnoreCase);
        }
    }

    private void ThemPhong()
    {
        using var form = new FormThemPhong();
        if (form.ShowDialog(this) == DialogResult.OK)
            LoadPhong();
    }

    private void Grid_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0)
            return;
        var row = _grid.Rows[e.RowIndex];
        var maPhong = row.Cells["MaPH"].Value?.ToString();
        if (string.IsNullOrWhiteSpace(maPhong))
            return;
        var column = _grid.Columns[e.ColumnIndex].Name;
        if (column == "colSua")
        {
            using var form = new FormSuaPhong { MaPhong = maPhong };
            if (form.ShowDialog(this) == DialogResult.OK)
                LoadPhong();
        }
        else if (column == "colXoa")
        {
            XoaPhong(maPhong);
        }
    }

    private void XoaPhong(string maPhong)
    {
        try
        {
            using var db = new HotelManagementContext();
            var room = db.Phongs.FirstOrDefault(p => p.MaPh == maPhong && p.DaXoa != true);
            if (room is null)
            {
                MessageBox.Show("Không tìm thấy phòng để xóa.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (room.Ttph == "Đang thuê" || room.Ttph == "Đã đặt")
            {
                MessageBox.Show($"Phòng {maPhong} đang ở trạng thái \"{room.Ttph}\" nên không thể xóa.",
                    "Không thể xóa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (MessageBox.Show($"Bạn có chắc muốn xóa phòng {maPhong}?", "Xác nhận xóa",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            room.DaXoa = true;
            db.SaveChanges();
            LoadPhong();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi khi xóa phòng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void XuatExcel()
    {
        var rows = _grid.Rows.Cast<DataGridViewRow>().Where(r => r.Visible).ToList();
        if (rows.Count == 0)
        {
            MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        using var dialog = new SaveFileDialog
        {
            Title = "Xuất danh sách phòng",
            Filter = "Excel Workbook (*.xlsx)|*.xlsx",
            FileName = $"DanhSachPhong_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;
        try
        {
            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Danh sách phòng");
            string[] headers = ["MaPH", "TTPH", "TTDD", "TenLPH"];
            string[] titles = ["Mã phòng", "Tình trạng phòng", "Dọn dẹp", "Loại phòng"];
            for (var c = 0; c < headers.Length; c++)
            {
                sheet.Cell(1, c + 1).Value = titles[c];
                sheet.Cell(1, c + 1).Style.Font.Bold = true;
            }
            for (var r = 0; r < rows.Count; r++)
                for (var c = 0; c < headers.Length; c++)
                    sheet.Cell(r + 2, c + 1).Value = rows[r].Cells[headers[c]].Value?.ToString() ?? string.Empty;
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
