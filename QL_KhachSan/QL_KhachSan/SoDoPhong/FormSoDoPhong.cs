using Microsoft.EntityFrameworkCore;
using QL_KhachSan.Models;

namespace QL_KhachSan.SoDoPhong;

public sealed class FormSoDoPhong : Form
{
    private readonly TextBox _txtSearch = HotelUi.TextBox("Nhập mã phòng...");
    private readonly DateTimePicker _dtNgayChon = new() { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false, Width = 150 };
    private readonly DateTimePicker _dtGioChon = new() { Format = DateTimePickerFormat.Time, ShowUpDown = true, Width = 110 };
    private readonly DataGridView _rooms = HotelUi.Grid();
    private readonly RadioButton _rdbTatCaTrangThai = new() { Text = "Tất cả phòng", Checked = true, AutoSize = true };
    private readonly RadioButton _rdbPhongTrong = new() { Text = "Phòng trống", AutoSize = true };
    private readonly RadioButton _rdbPhongDaDat = new() { Text = "Phòng đã đặt", AutoSize = true };
    private readonly RadioButton _rdbPhongDangThue = new() { Text = "Phòng đang thuê", AutoSize = true };
    private readonly RadioButton _rdbPhongDangSuaChua = new() { Text = "Phòng đang sửa chữa", AutoSize = true };
    private readonly RadioButton _rdbTatCaPhong = new() { Text = "Tất cả", Checked = true, AutoSize = true };
    private readonly RadioButton _rdbPhongChuaDon = new() { Text = "Chưa dọn dẹp", AutoSize = true };
    private readonly RadioButton _rdbPhongDaDonDep = new() { Text = "Đã dọn dẹp", AutoSize = true };
    private readonly RadioButton _rdbTatCaLoaiPhong = new() { Text = "Tất cả phòng", Checked = true, AutoSize = true };
    private readonly RadioButton _rdbPhongVipDoi = new() { Text = "Phòng VIP đôi", AutoSize = true };
    private readonly RadioButton _rdbPhongVipDon = new() { Text = "Phòng VIP đơn", AutoSize = true };
    private readonly RadioButton _rdbPhongThuongDoi = new() { Text = "Phòng thường đôi", AutoSize = true };
    private readonly RadioButton _rdbPhongThuongDon = new() { Text = "Phòng thường đơn", AutoSize = true };
    private string? _maPhongDangChon;

    public FormSoDoPhong()
    {
        Text = "Sơ đồ phòng";
        BackColor = HotelUi.Background;
        Font = new Font("Segoe UI", 9.5F);
        BuildLayout();
        HookFilters();
        ConfigureGrid();
        Load += (_, _) => TaiLaiDanhSach();
    }

    private void BuildLayout()
    {
        var page = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(12) };
        page.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        page.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        page.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        page.RowStyles.Add(new RowStyle(SizeType.Absolute, 86));
        var heading = HotelUi.Heading("Sơ đồ phòng khách sạn");
        page.Controls.Add(heading, 0, 0);

        var filters = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, WrapContents = true, BackColor = Color.White, Padding = new Padding(8) };
        filters.Controls.Add(HotelUi.FieldLabel("Mã phòng"));
        _txtSearch.Width = 140;
        filters.Controls.Add(_txtSearch);
        filters.Controls.Add(FilterGroup("Trạng thái phòng", _rdbTatCaTrangThai, _rdbPhongTrong, _rdbPhongDaDat, _rdbPhongDangThue, _rdbPhongDangSuaChua));
        filters.Controls.Add(FilterGroup("Tình trạng dọn dẹp", _rdbTatCaPhong, _rdbPhongChuaDon, _rdbPhongDaDonDep));
        filters.Controls.Add(FilterGroup("Loại phòng", _rdbTatCaLoaiPhong, _rdbPhongVipDoi, _rdbPhongVipDon, _rdbPhongThuongDoi, _rdbPhongThuongDon));
        page.Controls.Add(filters, 0, 1);
        page.Controls.Add(_rooms, 0, 2);

        var bottom = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Padding = new Padding(3, 6, 0, 0) };
        bottom.Controls.Add(HotelUi.FieldLabel("Chọn ngày"));
        bottom.Controls.Add(_dtNgayChon);
        bottom.Controls.Add(HotelUi.FieldLabel("Chọn giờ"));
        bottom.Controls.Add(_dtGioChon);
        bottom.Controls.Add(new Label { Width = 12 });
        var book = HotelUi.Button("Đặt phòng", primary: true);
        book.Click += (_, _) =>
        {
            var selectedRoom = MaPhongDangChon();
            MessageBox.Show("Chức năng đặt phòng đang được xây dựng."
                + (selectedRoom is not null ? Environment.NewLine + "Phòng đang chọn: " + selectedRoom : string.Empty),
                "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        };
        bottom.Controls.Add(book);
        page.Controls.Add(bottom, 0, 3);
        Controls.Add(page);
    }

    private static GroupBox FilterGroup(string title, params RadioButton[] radios)
    {
        var group = new GroupBox { Text = title, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(8), Margin = new Padding(8, 2, 8, 2) };
        var items = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Dock = DockStyle.Fill };
        foreach (var radio in radios)
            items.Controls.Add(radio);
        group.Controls.Add(items);
        return group;
    }

    private void HookFilters()
    {
        foreach (var radio in new[] { _rdbTatCaTrangThai, _rdbPhongTrong, _rdbPhongDaDat, _rdbPhongDangThue, _rdbPhongDangSuaChua,
                     _rdbTatCaPhong, _rdbPhongChuaDon, _rdbPhongDaDonDep, _rdbTatCaLoaiPhong, _rdbPhongVipDoi, _rdbPhongVipDon, _rdbPhongThuongDoi, _rdbPhongThuongDon })
            radio.CheckedChanged += (_, _) => { if (radio.Checked) TaiLaiDanhSach(); };
        _txtSearch.TextChanged += (_, _) => TaiLaiDanhSach();
        _dtNgayChon.ValueChanged += (_, _) => TaiLaiDanhSach();
        _dtGioChon.ValueChanged += (_, _) => TaiLaiDanhSach();
    }

    private void ConfigureGrid()
    {
        _rooms.AutoGenerateColumns = false;
        _rooms.Columns.Add(new DataGridViewTextBoxColumn { Name = "MaPH", DataPropertyName = nameof(PhongHienThi.MaPH), HeaderText = "Mã phòng" });
        _rooms.Columns.Add(new DataGridViewTextBoxColumn { Name = "Tang", DataPropertyName = nameof(PhongHienThi.Tang), HeaderText = "Tầng" });
        _rooms.Columns.Add(new DataGridViewTextBoxColumn { Name = "TenLPH", DataPropertyName = nameof(PhongHienThi.TenLPH), HeaderText = "Loại phòng" });
        _rooms.Columns.Add(new DataGridViewTextBoxColumn { Name = "TTPH", DataPropertyName = nameof(PhongHienThi.TTPH), HeaderText = "Trạng thái phòng" });
        _rooms.Columns.Add(new DataGridViewTextBoxColumn { Name = "TTDD", DataPropertyName = nameof(PhongHienThi.TTDD), HeaderText = "Tình trạng dọn dẹp" });
        _rooms.Columns.Add(new DataGridViewTextBoxColumn { Name = "GhiChu", DataPropertyName = nameof(PhongHienThi.GhiChu), HeaderText = "Ghi chú" });
        _rooms.SelectionChanged += (_, _) => _maPhongDangChon = MaPhongDangChon();
        _txtSearch.PlaceholderText = "Ví dụ: 101, P20...";
        new ToolTip().SetToolTip(_txtSearch, "Ví dụ: 101, P20...");
    }

    private void TaiLaiDanhSach()
    {
        if (!IsHandleCreated || IsDisposed)
            return;
        try
        {
            using var db = new HotelManagementContext();
            var allRooms = db.Phongs.AsNoTracking()
                .Where(p => p.DaXoa != true)
                .Include(p => p.MaLphNavigation)
                .Include(p => p.Ctdps)
                .OrderBy(p => p.MaPh)
                .ToList();
            var visibleRooms = allRooms
                .Where(p => LocLoaiPhong(p.MaLph))
                .Where(p => LocTrangThai(p.Ttph))
                .Where(p => LocDonDep(p.Ttdd))
                .Where(p => p.MaPh.Contains(_txtSearch.Text.Trim(), StringComparison.OrdinalIgnoreCase))
                .Where(p => !_dtNgayChon.Checked || PhongTrongNgayChon(p))
                .Select(p => new PhongHienThi
                {
                    MaPH = p.MaPh,
                    Tang = p.MaPh.Length > 1 ? p.MaPh.Substring(1, 1) : string.Empty,
                    TenLPH = p.MaLphNavigation.TenLph,
                    TTPH = p.Ttph,
                    TTDD = p.Ttdd,
                    GhiChu = p.GhiChu
                })
                .OrderBy(p => p.MaPH)
                .ToList();
            _rooms.DataSource = visibleRooms;
            if (_rooms.Rows.Count > 0)
                _rooms.Rows[0].Selected = true;
            else
                _maPhongDangChon = null;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi khi tải sơ đồ phòng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private bool PhongTrongNgayChon(Models.Phong room)
    {
        if (room.Ttph != "Bình thường")
            return false;
        var day = _dtNgayChon.Value.Date;
        return !room.Ctdps.Any(c => c.DaXoa != true
            && day >= c.CheckIn.Date
            && day < c.CheckOut.Date);
    }

    private bool LocTrangThai(string status) => _rdbTatCaTrangThai.Checked
        || (_rdbPhongTrong.Checked && status == "Bình thường")
        || (_rdbPhongDaDat.Checked && status == "Đã đặt")
        || (_rdbPhongDangThue.Checked && status == "Đang thuê")
        || (_rdbPhongDangSuaChua.Checked && status == "Đang sửa chữa");

    private bool LocDonDep(string status) => _rdbTatCaPhong.Checked
        || (_rdbPhongChuaDon.Checked && status == "Chưa dọn dẹp")
        || (_rdbPhongDaDonDep.Checked && status == "Đã dọn dẹp");

    private bool LocLoaiPhong(string code) => MaLoaiPhongDangChon() is not string selected || code == selected;

    private string? MaLoaiPhongDangChon()
    {
        if (_rdbPhongThuongDon.Checked) return "NOR01";
        if (_rdbPhongThuongDoi.Checked) return "NOR02";
        if (_rdbPhongVipDon.Checked) return "VIP01";
        if (_rdbPhongVipDoi.Checked) return "VIP02";
        return null;
    }

    private string? MaPhongDangChon() => _rooms.CurrentRow?.Cells["MaPH"].Value?.ToString();

    private sealed class PhongHienThi
    {
        public string MaPH { get; init; } = string.Empty;
        public string Tang { get; init; } = string.Empty;
        public string TenLPH { get; init; } = string.Empty;
        public string TTPH { get; init; } = string.Empty;
        public string TTDD { get; init; } = string.Empty;
        public string? GhiChu { get; init; }
    }
}
