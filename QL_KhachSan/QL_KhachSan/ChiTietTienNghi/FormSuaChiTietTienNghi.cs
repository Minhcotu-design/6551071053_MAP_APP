namespace QL_KhachSan.ChiTietTienNghi;

internal sealed class FormSuaChiTietTienNghi : FormChiTietTienNghiEditor
{
    public FormSuaChiTietTienNghi(IReadOnlyList<ChiTietOption> types, IReadOnlyList<ChiTietOption> amenities,
        string typeCode, string amenityCode, int quantity)
        : base("Cập nhật chi tiết tiện nghi", types, amenities, typeCode, amenityCode, quantity, editing: true) { }
}
