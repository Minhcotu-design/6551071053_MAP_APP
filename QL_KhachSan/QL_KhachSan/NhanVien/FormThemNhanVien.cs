using QL_KhachSan.DichVu;

namespace QL_KhachSan.NhanVien;

internal sealed class FormThemNhanVien : RecordEditorForm
{
    public FormThemNhanVien(Dictionary<string, FieldValue> fields) : base("Thêm thông tin nhân viên", fields) { }
}
