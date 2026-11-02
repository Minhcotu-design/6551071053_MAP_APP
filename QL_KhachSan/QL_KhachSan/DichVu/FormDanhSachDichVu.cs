namespace QL_KhachSan.DichVu;

internal sealed class FormDanhSachDichVu : CatalogListForm
{
    public FormDanhSachDichVu() : base(CatalogKind.DichVu, "Quản lý thông tin dịch vụ",
        ("MaDV", "Mã dịch vụ"), ("TenDV", "Tên dịch vụ"), ("DonGia", "Đơn giá"),
        ("SLConLai", "Số lượng còn lại"), ("LoaiDV", "Loại dịch vụ")) { }
}
