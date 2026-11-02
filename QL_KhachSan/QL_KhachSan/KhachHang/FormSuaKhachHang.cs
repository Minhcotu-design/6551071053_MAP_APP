using QL_KhachSan.DichVu;

namespace QL_KhachSan.KhachHang;

internal sealed class FormSuaKhachHang : RecordEditorForm
{
    public FormSuaKhachHang(Dictionary<string, FieldValue> fields) : base("Cập nhật thông tin khách hàng", fields) { }
}
