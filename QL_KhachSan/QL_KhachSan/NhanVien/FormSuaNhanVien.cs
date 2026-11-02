using QL_KhachSan.DichVu;

namespace QL_KhachSan.NhanVien;

internal sealed class FormSuaNhanVien : RecordEditorForm
{
    public FormSuaNhanVien(Dictionary<string, FieldValue> fields) : base("Cập nhật thông tin nhân viên", fields) { }
}
