using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QL_KhachSan.DangNhap;
using QL_KhachSan.Models;

namespace QL_KhachSan.TaiKhoan;

internal sealed class AccountRow { public string TenTK { get; set; } = ""; public string TenNV { get; set; } = ""; public string MaNV { get; set; } = ""; public int CapDo { get; set; } public string Quyen => CapDo switch { 3 => "Cấp 3 - Admin", 2 => "Cấp 2 - Quản lý", 1 => "Cấp 1 - Nhân viên", _ => $"Cấp {CapDo}" }; }
