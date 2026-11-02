using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QL_KhachSan.DangNhap;
using QL_KhachSan.Models;

namespace QL_KhachSan.SoDoPhong;

internal sealed class FormDatPhong : Form
{
    private readonly TextBox _name = HotelUi.TextBox();
    private readonly TextBox _idNumber = HotelUi.TextBox();
    private readonly TextBox _phone = HotelUi.TextBox();
    private readonly TextBox _nationality = HotelUi.TextBox();
    private readonly ComboBox _gender = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10.5F) };
    private readonly DateTimePicker _checkIn = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm", Width = 190 };
    private readonly DateTimePicker _checkOut = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm", Width = 190 };
    private readonly NumericUpDown _guests = new() { Minimum = 1, Maximum = 20, Value = 1, Width = 90 };
    private readonly DataGridView _available = HotelUi.Grid();
    private readonly DataGridView _selected = HotelUi.Grid();
    private readonly string? _initialRoom;

    public FormDatPhong(string? maPhong = null)
    {
        _initialRoom = maPhong;
        Text = "Đặt phòng trước"; StartPosition = FormStartPosition.CenterParent; FormBorderStyle = FormBorderStyle.Sizable; MinimizeBox = false; BackColor = HotelUi.Background; MinimumSize = new Size(950, 680); Size = new Size(1080, 740);
        _checkIn.Value = DateTime.Now.AddHours(1); _checkOut.Value = DateTime.Now.AddDays(1);
        _gender.Items.AddRange(["Nam", "Nữ"]); _gender.SelectedIndex = 0;
        BuildLayout(); _idNumber.TextChanged += (_, _) => FindGuest(); _checkIn.ValueChanged += (_, _) => LoadAvailableRooms(); _checkOut.ValueChanged += (_, _) => LoadAvailableRooms();
        Load += (_, _) => { LoadAvailableRooms(); if (!string.IsNullOrWhiteSpace(_initialRoom)) AddRoomByCode(_initialRoom); };
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(12) };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48)); root.RowStyles.Add(new RowStyle(SizeType.Absolute, 168)); root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(HotelUi.Heading("Thông tin đặt phòng"), 0, 0);
        var guest = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 3, BackColor = Color.White, Padding = new Padding(8) };
        guest.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105)); guest.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); guest.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 125)); guest.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        Field(guest, 0, 0, "Họ và tên", _name); Field(guest, 0, 2, "CCCD/Passport", _idNumber); Field(guest, 1, 0, "Số điện thoại", _phone); Field(guest, 1, 2, "Quốc tịch", _nationality); Field(guest, 2, 0, "Giới tính", _gender);
        var datePanel = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false }; datePanel.Controls.Add(HotelUi.FieldLabel("Từ")); datePanel.Controls.Add(_checkIn); datePanel.Controls.Add(HotelUi.FieldLabel("Đến")); datePanel.Controls.Add(_checkOut); datePanel.Controls.Add(HotelUi.FieldLabel("Số khách/phòng")); datePanel.Controls.Add(_guests);
        guest.Controls.Add(datePanel, 1, 2); guest.SetColumnSpan(datePanel, 3); root.Controls.Add(guest, 0, 1);

        var area = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Padding = new Padding(0, 8, 0, 0) }; area.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); area.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); area.RowStyles.Add(new RowStyle(SizeType.Absolute, 34)); area.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        area.Controls.Add(HotelUi.FieldLabel("Phòng còn trống trong thời gian chọn"), 0, 0); area.Controls.Add(HotelUi.FieldLabel("Phòng đã chọn"), 1, 0);
        ConfigureGrid(_available, ["Mã phòng", "Loại phòng", "Sức chứa", "Giá ngày"]); ConfigureGrid(_selected, ["Mã phòng", "Loại phòng", "Số khách", "Giá ngày"]);
        area.Controls.Add(_available, 0, 1); area.Controls.Add(_selected, 1, 1); root.Controls.Add(area, 0, 2);
        var add = HotelUi.Button("Thêm phòng →", true); var remove = HotelUi.Button("← Bỏ phòng"); var save = HotelUi.Button("Đặt trước", true); var cancel = HotelUi.Button("Hủy");
        add.Click += (_, _) => AddSelectedRoom(); remove.Click += (_, _) => RemoveSelectedRoom(); save.Click += (_, _) => SaveBooking(); cancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        var actions = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 54, FlowDirection = FlowDirection.RightToLeft }; actions.Controls.AddRange([cancel, save, remove, add]);
        Controls.Add(root); Controls.Add(actions); Controls.SetChildIndex(actions, 0);
        _available.CellDoubleClick += (_, _) => AddSelectedRoom(); _selected.CellDoubleClick += (_, _) => RemoveSelectedRoom();
    }

    private static void Field(TableLayoutPanel panel, int row, int col, string label, Control input)
    {
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44)); panel.Controls.Add(HotelUi.FieldLabel(label), col, row); input.Dock = DockStyle.Fill; input.Margin = new Padding(5); panel.Controls.Add(input, col + 1, row);
    }
    private static void ConfigureGrid(DataGridView grid, string[] headers) { foreach (var h in headers) grid.Columns.Add(h, h); }

    private void LoadAvailableRooms()
    {
        if (!IsHandleCreated || _checkOut.Value <= _checkIn.Value) return;
        try
        {
            using var db = new HotelManagementContext(); _available.Rows.Clear();
            var chosen = _selected.Rows.Cast<DataGridViewRow>().Select(r => r.Cells[0].Value?.ToString()).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var rooms = db.Phongs.AsNoTracking().Where(p => p.DaXoa != true && p.Ttph == "Bình thường").Include(p => p.MaLphNavigation).Include(p => p.Ctdps).OrderBy(p => p.MaPh).ToList();
            foreach (var p in rooms)
            {
                var overlap = p.Ctdps.Any(c => c.DaXoa != true && c.CheckIn < _checkOut.Value && c.CheckOut > _checkIn.Value);
                if (!overlap && !chosen.Contains(p.MaPh)) _available.Rows.Add(p.MaPh, p.MaLphNavigation.TenLph, p.MaLphNavigation.SoNguoiToiDa, p.MaLphNavigation.GiaNgay);
            }
        }
        catch (Exception ex) { MessageBox.Show("Lỗi tải phòng còn trống: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void AddSelectedRoom()
    {
        var code = _available.CurrentRow?.Cells[0].Value?.ToString(); if (code is null) return; AddRoomByCode(code);
    }
    private void AddRoomByCode(string code)
    {
        if (_selected.Rows.Cast<DataGridViewRow>().Any(r => string.Equals(r.Cells[0].Value?.ToString(), code, StringComparison.OrdinalIgnoreCase))) return;
        var row = _available.Rows.Cast<DataGridViewRow>().FirstOrDefault(r => string.Equals(r.Cells[0].Value?.ToString(), code, StringComparison.OrdinalIgnoreCase));
        if (row is null) return;
        _selected.Rows.Add(row.Cells[0].Value ?? "", row.Cells[1].Value ?? "", _guests.Value, row.Cells[3].Value ?? ""); _available.Rows.Remove(row);
    }
    private void RemoveSelectedRoom()
    {
        if (_selected.CurrentRow is not { } row) return; var values = row.Cells.Cast<DataGridViewCell>().Select(c => c.Value).ToArray(); _selected.Rows.Remove(row); _available.Rows.Add(values[0] ?? "", values[1] ?? "", "", values[3] ?? ""); LoadAvailableRooms();
    }

    private void FindGuest()
    {
        var id = _idNumber.Text.Trim(); if (id.Length < 5) return;
        try
        {
            using var db = new HotelManagementContext(); var customer = db.KhachHangs.AsNoTracking().FirstOrDefault(x => x.CccdPassport == id);
            if (customer is null) return;
            _name.Text = customer.TenKh; _phone.Text = customer.Sdt; _nationality.Text = customer.QuocTich; _gender.SelectedItem = customer.GioiTinh;
            _name.ReadOnly = _phone.ReadOnly = _nationality.ReadOnly = true; _gender.Enabled = false;
        }
        catch { }
    }

    private void SaveBooking()
    {
        if (_checkOut.Value <= _checkIn.Value) { MessageBox.Show("Thời gian trả phòng phải sau thời gian nhận phòng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        if (_selected.Rows.Count == 0) { MessageBox.Show("Hãy chọn ít nhất một phòng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        if (new[] { _name.Text, _idNumber.Text, _phone.Text, _nationality.Text }.Any(string.IsNullOrWhiteSpace)) { MessageBox.Show("Vui lòng nhập đầy đủ thông tin khách hàng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        try
        {
            using var db = new HotelManagementContext(); using var tx = db.Database.BeginTransaction();
            var customer = db.KhachHangs.FirstOrDefault(x => x.CccdPassport == _idNumber.Text.Trim());
            if (customer is null)
            {
                customer = new QL_KhachSan.Models.KhachHang { MaKh = NextCode(db.KhachHangs.Select(x => x.MaKh).ToList(), "KH", 3, 5), TenKh = _name.Text.Trim(), CccdPassport = _idNumber.Text.Trim(), Sdt = _phone.Text.Trim(), QuocTich = _nationality.Text.Trim(), GioiTinh = _gender.Text, DaXoa = false }; db.KhachHangs.Add(customer);
            }
            else { customer.TenKh = _name.Text.Trim(); customer.Sdt = _phone.Text.Trim(); customer.QuocTich = _nationality.Text.Trim(); customer.GioiTinh = _gender.Text; customer.DaXoa = false; }
            var staff = db.NhanViens.FirstOrDefault(x => x.MaNv == FormDangNhap.MaNV_DangNhap && x.DaXoa != true);
            if (staff is null) throw new InvalidOperationException("Không tìm thấy nhân viên đang đăng nhập để lập phiếu thuê.");
            var maPt = NextCode(db.PhieuThues.Select(x => x.MaPt).ToList(), "PT", 3, 5);
            db.PhieuThues.Add(new PhieuThue { MaPt = maPt, NgPt = DateTime.Now, MaKh = customer.MaKh, MaNv = staff.MaNv, DaXoa = false }); db.SaveChanges();
            var codes = db.Ctdps.Select(x => x.MaCtdp).ToList();
            foreach (DataGridViewRow selected in _selected.Rows)
            {
                var code = selected.Cells[0].Value?.ToString() ?? ""; var room = db.Phongs.Include(x => x.MaLphNavigation).FirstOrDefault(x => x.MaPh == code && x.DaXoa != true);
                if (room is null || room.Ttph != "Bình thường") throw new InvalidOperationException($"Phòng {code} hiện không thể đặt.");
                if (db.Ctdps.Any(x => x.MaPh == code && x.DaXoa != true && x.CheckIn < _checkOut.Value && x.CheckOut > _checkIn.Value)) throw new InvalidOperationException($"Phòng {code} vừa được đặt trong khoảng thời gian này. Hãy tải lại và chọn phòng khác.");
                var guestCount = Convert.ToInt32(selected.Cells[2].Value ?? 1);
                if (guestCount > room.MaLphNavigation.SoNguoiToiDa) throw new InvalidOperationException($"Số khách vượt sức chứa phòng {code}.");
                var ctdpCode = NextCode(codes, "CTDP", 3, 7); codes.Add(ctdpCode);
                var days = Math.Max(1, (int)Math.Ceiling((_checkOut.Value - _checkIn.Value).TotalDays));
                db.Ctdps.Add(new Ctdp { MaCtdp = ctdpCode, MaPt = maPt, MaPh = code, CheckIn = _checkIn.Value, CheckOut = _checkOut.Value, SoNguoi = guestCount, TrangThai = "Đã đặt", DonGia = room.MaLphNavigation.GiaNgay, ThanhTien = room.MaLphNavigation.GiaNgay * days, DaXoa = false, TheoGio = false });
                room.Ttph = "Đã đặt";
                // CapNhatGiaCTDP is a row-oriented trigger in the supplied database.
                // Save one detail at a time so EF does not batch several inserted rows
                // into one statement that makes the trigger's scalar subquery fail.
                db.SaveChanges();
            }
            tx.Commit(); MessageBox.Show("Đặt phòng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information); DialogResult = DialogResult.OK; Close();
        }
        catch (Exception ex) { MessageBox.Show("Không thể hoàn tất đặt phòng: " + (ex.InnerException?.Message ?? ex.Message), "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }

    private static string NextCode(List<string> existing, string prefix, int digits, int maxLength)
    {
        var used = existing.Select(x => x.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && int.TryParse(x[prefix.Length..], out var n) ? n : 0).ToHashSet();
        var number = Enumerable.Range(1, 999).FirstOrDefault(x => !used.Contains(x)); if (number == 0) throw new InvalidOperationException($"Đã hết mã {prefix}.");
        var result = prefix + number.ToString($"D{digits}", CultureInfo.InvariantCulture); return result.Length <= maxLength ? result : throw new InvalidOperationException("Mã sinh tự động vượt độ dài cho phép.");
    }
}
