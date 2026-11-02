using QL_KhachSan.DichVu;

namespace QL_KhachSan.TienNghi;

internal sealed class FormSuaTienNghi : RecordEditorForm
{
    public FormSuaTienNghi(Dictionary<string, FieldValue> fields) : base("Cập nhật tiện nghi", fields) { }
}
