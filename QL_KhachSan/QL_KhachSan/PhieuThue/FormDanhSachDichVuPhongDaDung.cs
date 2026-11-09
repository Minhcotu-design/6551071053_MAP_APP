using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QL_KhachSan.DangNhap;
using QL_KhachSan.Models;
using QL_KhachSan.SoDoPhong;
using System.Globalization;

namespace QL_KhachSan.PhieuThue;

internal sealed class FormDanhSachDichVuPhongDaDung : Form
{
    private readonly string _id; private readonly string _room; private readonly DataGridView _grid = HotelUi.Grid();
    public FormDanhSachDichVuPhongDaDung(string id, string room) { _id = id; _room = room; Text = "Danh sách dịch vụ đã dùng"; StartPosition = FormStartPosition.CenterParent; Size = new Size(850, 500); var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(14), ColumnCount = 1, RowCount = 3 }; root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48)); root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54)); root.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); root.Controls.Add(HotelUi.Heading($"Dịch vụ đã dùng - Phòng {room} / CT {id}"), 0, 0); var tools = new FlowLayoutPanel { Dock = DockStyle.Fill }; var add = HotelUi.Button("Thêm dịch vụ", true); var close = HotelUi.Button("Đóng"); add.Click += (_, _) => Add(); close.Click += (_, _) => Close(); tools.Controls.AddRange([add, close]); root.Controls.Add(tools, 0, 1); _grid.Columns.Add("MaDV", "Mã dịch vụ"); _grid.Columns.Add("TenDV", "Tên dịch vụ"); _grid.Columns.Add("SL", "Số lượng"); _grid.Columns.Add("DonGia", "Đơn giá"); _grid.Columns.Add("ThanhTien", "Thành tiền"); _grid.Columns[3].DefaultCellStyle.Format = _grid.Columns[4].DefaultCellStyle.Format = "N0"; root.Controls.Add(_grid, 0, 2); Controls.Add(root); Load += (_, _) => LoadRows(); }
    private void LoadRows() { try { using var db = new HotelManagementContext(); _grid.Rows.Clear(); foreach (var x in db.Ctdvs.AsNoTracking().Where(v => v.MaCtdp == _id && v.DaXoa != true).Include(v => v.MaDvNavigation).OrderBy(v => v.MaDv).ThenBy(v => v.DonGia).ToList()) _grid.Rows.Add(x.MaDv, x.MaDvNavigation.TenDv, x.Sl, x.DonGia, x.ThanhTien ?? x.Sl * x.DonGia); } catch (Exception ex) { MessageBox.Show("Lỗi tải dịch vụ đã dùng: " + ex.GetBaseException().Message); } }
    private void Add() { using var f = new FormThemDichVuPhongDaDung(_id, _room); if (f.ShowDialog(this) == DialogResult.OK) LoadRows(); }
}
