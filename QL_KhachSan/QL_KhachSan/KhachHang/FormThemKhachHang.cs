using QL_KhachSan.DichVu;

namespace QL_KhachSan.KhachHang;

internal sealed class FormThemKhachHang : RecordEditorForm
{
    public FormThemKhachHang(Dictionary<string, FieldValue> fields) : base("Thêm thông tin khách hàng", fields) { }
}
