using QL_KhachSan.DichVu;

namespace QL_KhachSan.KhachHang;

internal sealed class FormDanhSachKhachHang : CatalogListForm
{
    public FormDanhSachKhachHang() : base(CatalogKind.KhachHang, "Quản lý thông tin khách hàng",
        ("MaKH", "Mã khách hàng"), ("TenKH", "Họ và tên"), ("SDT", "Số điện thoại"),
        ("CCCD/Passport", "CCCD/Passport"), ("QuocTich", "Quốc tịch"), ("GioiTinh", "Giới tính")) { }
}
