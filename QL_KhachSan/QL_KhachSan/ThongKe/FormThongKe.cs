using Microsoft.EntityFrameworkCore;
using QL_KhachSan.Models;
using ClosedXML.Excel;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace QL_KhachSan.ThongKe;

internal sealed class FormThongKe : Form
{
    private readonly DateTimePicker _from = new() { Format = DateTimePickerFormat.Short, Width = 140 };
    private readonly DateTimePicker _to = new() { Format = DateTimePickerFormat.Short, Width = 140 };
    private readonly ComboBox _groupBy = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
    private readonly Label _revenue = MetricValue();
    private readonly Label _invoiceCount = MetricValue();
    private readonly Label _roomNights = MetricValue();
    private readonly Label _guestCount = MetricValue();
    private readonly RevenueChart _chart = new();
    private readonly DataGridView _topServices = HotelUi.Grid();
    private List<PointValue> _currentBreakdown = [];

    private sealed record InvoiceRow(DateTime? Date, decimal Amount, string MaCTDP, string RoomType, string Employee, string MaKH);
    private sealed record PointValue(string Label, decimal Amount);
    private sealed record ServiceValue(string Name, int Quantity, decimal Amount);

    public FormThongKe()
    {
        Text = "Thống kê kinh doanh";
        BackColor = HotelUi.Background;
        _from.Value = DateTime.Today.AddDays(-29);
        _to.Value = DateTime.Today;
        _groupBy.Items.AddRange(["Ngày", "Tháng", "Loại phòng", "Nhân viên"]);
        _groupBy.SelectedIndex = 0;

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 1, RowCount = 4 };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(HotelUi.Heading("Báo cáo tình hình kinh doanh"), 0, 0);

        var filters = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = true };
        var apply = HotelUi.Button("Thống kê", true);
        var export = HotelUi.Button("Xuất Excel");
        filters.Controls.AddRange([
            new Label { Text = "Từ ngày", AutoSize = true, Margin = new Padding(5, 13, 2, 0) }, _from,
            new Label { Text = "Đến ngày", AutoSize = true, Margin = new Padding(10, 13, 2, 0) }, _to,
            new Label { Text = "Nhóm theo", AutoSize = true, Margin = new Padding(10, 13, 2, 0) }, _groupBy, apply, export
        ]);
        apply.Click += (_, _) => LoadReport();
        export.Click += (_, _) => ExportReport();
        _groupBy.SelectedIndexChanged += (_, _) => { if (IsHandleCreated) LoadReport(); };
        root.Controls.Add(filters, 0, 1);

        var cards = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, BackColor = HotelUi.Background };
        for (var i = 0; i < 4; i++) cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        cards.Controls.Add(MetricCard("Tổng doanh thu", _revenue), 0, 0);
        cards.Controls.Add(MetricCard("Số hóa đơn", _invoiceCount), 1, 0);
        cards.Controls.Add(MetricCard("Đêm phòng", _roomNights), 2, 0);
        cards.Controls.Add(MetricCard("Khách hàng", _guestCount), 3, 0);
        root.Controls.Add(cards, 0, 2);

        var body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        var chartCard = HotelUi.Card();
        chartCard.Dock = DockStyle.Fill;
        chartCard.Margin = new Padding(4);
        chartCard.Controls.Add(_chart);
        _chart.Dock = DockStyle.Fill;
        var serviceCard = HotelUi.Card();
        serviceCard.Dock = DockStyle.Fill;
        serviceCard.Margin = new Padding(4);
        serviceCard.Controls.Add(_topServices);
        _topServices.Dock = DockStyle.Fill;
        _topServices.Columns.Add("DichVu", "Dịch vụ");
        _topServices.Columns.Add("SoLuong", "SL");
        _topServices.Columns.Add("DoanhThu", "Doanh thu");
        _topServices.Columns[0].FillWeight = 48;
        _topServices.Columns[1].FillWeight = 15;
        _topServices.Columns[2].FillWeight = 37;
        _topServices.Columns[2].DefaultCellStyle.Format = "N0";
        body.Controls.Add(chartCard, 0, 0);
        body.Controls.Add(serviceCard, 1, 0);
        root.Controls.Add(body, 0, 3);
        Controls.Add(root);
        Load += (_, _) => { SetDefaultRangeFromData(); LoadReport(); };
    }

    private static Label MetricValue() => new() { AutoSize = true, Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = HotelUi.DarkGreen, Dock = DockStyle.Bottom };

    private static Control MetricCard(string title, Label value)
    {
        var card = HotelUi.Card();
        var label = new Label { Text = title, AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = HotelUi.Muted, Dock = DockStyle.Top };
        card.Controls.Add(value);
        card.Controls.Add(label);
        return card;
    }

    private void SetDefaultRangeFromData()
    {
        try
        {
            using var db = new HotelManagementContext();
            var latest = db.HoaDons.AsNoTracking()
                .Where(h => h.DaXoa != true && h.TrangThai == "Đã thanh toán" && h.NgHd != null)
                .Select(h => h.NgHd)
                .Max();
            if (latest is not DateTime date) return;
            _to.Value = date.Date;
            _from.Value = date.Date.AddDays(-29);
        }
        catch
        {
            // Keep the default rolling 30-day range if the database cannot provide a latest date.
        }
    }

    private void LoadReport()
    {
        if (_from.Value.Date > _to.Value.Date)
        {
            MessageBox.Show("Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.", "Khoảng ngày không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            using var db = new HotelManagementContext();
            var from = _from.Value.Date;
            var toExclusive = _to.Value.Date.AddDays(1);
            var invoices = db.HoaDons.AsNoTracking()
                .Where(h => h.DaXoa != true && h.TrangThai == "Đã thanh toán" && h.NgHd >= from && h.NgHd < toExclusive)
                .Select(h => new InvoiceRow(
                    h.NgHd,
                    h.TriGia ?? 0m,
                    h.MaCtdp,
                    h.MaCtdpNavigation.MaPhNavigation.MaLphNavigation.TenLph,
                    h.MaNvNavigation == null ? "(Chưa xác định)" : h.MaNvNavigation.TenNv,
                    h.MaCtdpNavigation.MaPtNavigation.MaKh))
                .ToList();

            _revenue.Text = invoices.Sum(x => x.Amount).ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " đ";
            _invoiceCount.Text = invoices.Count.ToString("N0");
            var stays = db.Ctdps.AsNoTracking()
                .Where(c => c.DaXoa != true && c.CheckIn < toExclusive && c.CheckOut > from)
                .Select(c => new { c.CheckIn, c.CheckOut })
                .ToList();
            var nights = stays.Sum(x =>
            {
                var start = x.CheckIn.Date > from ? x.CheckIn.Date : from;
                var end = x.CheckOut.Date < toExclusive ? x.CheckOut.Date : toExclusive;
                return Math.Max(0, (int)(end - start).TotalDays);
            });
            _roomNights.Text = nights.ToString("N0");
            _guestCount.Text = invoices.Select(x => x.MaKH).Distinct(StringComparer.OrdinalIgnoreCase).Count().ToString("N0");

            var mode = _groupBy.SelectedItem?.ToString() ?? "Ngày";
            var chartRows = mode switch
            {
                "Tháng" => invoices.Where(x => x.Date.HasValue)
                    .GroupBy(x => new DateTime(x.Date!.Value.Year, x.Date.Value.Month, 1))
                    .OrderBy(g => g.Key)
                    .Select(g => new PointValue(g.Key.ToString("MM/yyyy"), g.Sum(x => x.Amount))).ToList(),
                "Loại phòng" => invoices.GroupBy(x => x.RoomType)
                    .OrderByDescending(g => g.Sum(x => x.Amount))
                    .Select(g => new PointValue(g.Key, g.Sum(x => x.Amount))).ToList(),
                "Nhân viên" => invoices.GroupBy(x => x.Employee)
                    .OrderByDescending(g => g.Sum(x => x.Amount))
                    .Select(g => new PointValue(g.Key, g.Sum(x => x.Amount))).ToList(),
                _ => invoices.Where(x => x.Date.HasValue)
                    .GroupBy(x => x.Date!.Value.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new PointValue(g.Key.ToString("dd/MM"), g.Sum(x => x.Amount))).ToList()
            };
            _currentBreakdown = chartRows;
            _chart.SetData(chartRows, $"Doanh thu theo {mode.ToLowerInvariant()}");

            var invoiceDetails = invoices.Select(x => x.MaCTDP).Distinct(StringComparer.OrdinalIgnoreCase).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var serviceRows = db.Ctdvs.AsNoTracking()
                .Where(v => v.DaXoa != true && invoiceDetails.Contains(v.MaCtdp))
                .Include(v => v.MaDvNavigation)
                .ToList()
                .GroupBy(v => v.MaDvNavigation.TenDv)
                .Select(g => new ServiceValue(g.Key, g.Sum(x => x.Sl), g.Sum(x => x.ThanhTien ?? x.DonGia * x.Sl)))
                .OrderByDescending(x => x.Amount)
                .Take(10)
                .ToList();
            _topServices.Rows.Clear();
            foreach (var x in serviceRows) _topServices.Rows.Add(x.Name, x.Quantity, x.Amount);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi lập báo cáo: " + ex.GetBaseException().Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ExportReport()
    {
        using var save = new SaveFileDialog
        {
            Filter = "Excel Workbook (*.xlsx)|*.xlsx",
            FileName = $"BaoCaoDoanhThu_{_from.Value:yyyyMMdd}_{_to.Value:yyyyMMdd}.xlsx",
            Title = "Xuất báo cáo doanh thu"
        };
        if (save.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            using var workbook = new XLWorkbook();
            var summary = workbook.Worksheets.Add("Doanh thu");
            summary.Cell(1, 1).Value = "BÁO CÁO DOANH THU KHÁCH SẠN";
            summary.Range(1, 1, 1, 3).Merge().Style.Font.Bold = true;
            summary.Cell(2, 1).Value = "Từ ngày";
            summary.Cell(2, 2).Value = _from.Value.Date;
            summary.Cell(2, 2).Style.DateFormat.Format = "dd/MM/yyyy";
            summary.Cell(3, 1).Value = "Đến ngày";
            summary.Cell(3, 2).Value = _to.Value.Date;
            summary.Cell(3, 2).Style.DateFormat.Format = "dd/MM/yyyy";
            summary.Cell(4, 1).Value = "Tổng doanh thu";
            summary.Cell(4, 2).Value = decimal.TryParse(_revenue.Text.Replace(" đ", "").Replace(".", "").Replace(",", ""), NumberStyles.Number, CultureInfo.InvariantCulture, out var revenue) ? revenue : 0m;
            summary.Cell(4, 2).Style.NumberFormat.Format = "#,##0";
            summary.Cell(5, 1).Value = "Số hóa đơn";
            summary.Cell(5, 2).Value = int.TryParse(_invoiceCount.Text, NumberStyles.Number, CultureInfo.GetCultureInfo("vi-VN"), out var invoices) ? invoices : 0;
            summary.Cell(6, 1).Value = "Đêm phòng";
            summary.Cell(6, 2).Value = int.TryParse(_roomNights.Text, NumberStyles.Number, CultureInfo.GetCultureInfo("vi-VN"), out var nights) ? nights : 0;
            summary.Cell(7, 1).Value = "Khách hàng";
            summary.Cell(7, 2).Value = int.TryParse(_guestCount.Text, NumberStyles.Number, CultureInfo.GetCultureInfo("vi-VN"), out var guests) ? guests : 0;
            summary.Cell(9, 1).Value = $"Doanh thu theo {_groupBy.SelectedItem}";
            summary.Range(9, 1, 9, 2).Style.Font.Bold = true;
            summary.Cell(10, 1).Value = "Nhóm";
            summary.Cell(10, 2).Value = "Doanh thu";
            summary.Range(10, 1, 10, 2).Style.Font.Bold = true;
            for (var i = 0; i < _currentBreakdown.Count; i++)
            {
                summary.Cell(i + 11, 1).Value = _currentBreakdown[i].Label;
                summary.Cell(i + 11, 2).Value = _currentBreakdown[i].Amount;
                summary.Cell(i + 11, 2).Style.NumberFormat.Format = "#,##0";
            }
            summary.Columns().AdjustToContents();

            var services = workbook.Worksheets.Add("Dịch vụ");
            for (var col = 0; col < _topServices.Columns.Count; col++)
            {
                services.Cell(1, col + 1).Value = _topServices.Columns[col].HeaderText;
                services.Cell(1, col + 1).Style.Font.Bold = true;
            }
            for (var row = 0; row < _topServices.Rows.Count; row++)
                for (var col = 0; col < _topServices.Columns.Count; col++)
                    services.Cell(row + 2, col + 1).Value = _topServices.Rows[row].Cells[col].Value?.ToString() ?? string.Empty;
            services.Columns().AdjustToContents();
            workbook.SaveAs(save.FileName);
            MessageBox.Show("Xuất báo cáo Excel thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { MessageBox.Show("Lỗi xuất báo cáo Excel: " + ex.GetBaseException().Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private sealed class RevenueChart : Panel
    {
        private List<PointValue> _values = [];
        private string _title = "Doanh thu";

        public RevenueChart() { BackColor = Color.White; DoubleBuffered = true; MinimumSize = new Size(360, 260); }
        public void SetData(List<PointValue> values, string title) { _values = values; _title = title; Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var g = e.Graphics;
            var area = new Rectangle(54, 48, Math.Max(20, ClientSize.Width - 82), Math.Max(20, ClientSize.Height - 98));
            using var titleFont = new Font("Segoe UI", 11, FontStyle.Bold);
            using var font = new Font("Segoe UI", 8);
            using var titleBrush = new SolidBrush(HotelUi.DarkGreen);
            using var textBrush = new SolidBrush(HotelUi.Muted);
            using var gridPen = new Pen(HotelUi.Border);
            using var barBrush = new SolidBrush(HotelUi.Green);
            g.DrawString(_title, titleFont, titleBrush, 14, 12);
            if (_values.Count == 0) { g.DrawString("Chưa có hóa đơn trong khoảng thời gian này.", font, textBrush, area.Left, area.Top + 20); return; }

            var max = Math.Max(1m, _values.Max(x => x.Amount));
            const int ticks = 4;
            for (var i = 0; i <= ticks; i++)
            {
                var y = area.Bottom - (int)(area.Height * i / (float)ticks);
                g.DrawLine(gridPen, area.Left, y, area.Right, y);
                g.DrawString((max * i / ticks).ToString("N0"), font, textBrush, 2, y - 8);
            }

            var slot = area.Width / (float)_values.Count;
            var barWidth = Math.Max(3f, Math.Min(30f, slot * .62f));
            var labelStep = Math.Max(1, _values.Count / 10);
            for (var i = 0; i < _values.Count; i++)
            {
                var height = (float)(_values[i].Amount / max) * area.Height;
                var x = area.Left + i * slot + (slot - barWidth) / 2;
                g.FillRectangle(barBrush, new RectangleF(x, area.Bottom - height, barWidth, height));
                if (_values.Count <= 14 || i % labelStep == 0)
                {
                    var label = _values[i].Label.Length > 12 ? _values[i].Label[..12] + "…" : _values[i].Label;
                    var size = g.MeasureString(label, font);
                    g.DrawString(label, font, textBrush, x + (barWidth - size.Width) / 2, area.Bottom + 5);
                }
            }
        }
    }
}
