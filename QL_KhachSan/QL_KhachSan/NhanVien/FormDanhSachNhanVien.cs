using QL_KhachSan.DichVu;

namespace QL_KhachSan.NhanVien;

internal sealed class FormDanhSachNhanVien : CatalogListForm
{
    public FormDanhSachNhanVien() : base(CatalogKind.NhanVien, "Quản lý thông tin nhân viên",
        ("MaNV", "Mã nhân viên"), ("TenNV", "Họ và tên"), ("ChucVu", "Chức vụ"),
        ("SDT", "Số điện thoại"), ("CCCD", "CCCD"), ("NgaySinh", "Ngày sinh"),
        ("GioiTinh", "Giới tính"), ("Email", "Email")) { }
}
