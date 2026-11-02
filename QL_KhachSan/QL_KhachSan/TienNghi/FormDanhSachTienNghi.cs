using QL_KhachSan.DichVu;

namespace QL_KhachSan.TienNghi;

internal sealed class FormDanhSachTienNghi : CatalogListForm
{
    public FormDanhSachTienNghi() : base(CatalogKind.TienNghi, "Quản lý thông tin tiện nghi",
        ("MaTN", "Mã tiện nghi"), ("TenTN", "Tên tiện nghi")) { }
}
