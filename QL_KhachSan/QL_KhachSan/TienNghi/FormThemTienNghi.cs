using QL_KhachSan.DichVu;

namespace QL_KhachSan.TienNghi;

internal sealed class FormThemTienNghi : RecordEditorForm
{
    public FormThemTienNghi(Dictionary<string, FieldValue> fields) : base("Thêm mới tiện nghi", fields) { }
}
