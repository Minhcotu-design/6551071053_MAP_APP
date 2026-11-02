namespace QL_KhachSan.ChiTietTienNghi;

internal sealed record ChiTietOption(string Code, string Label);

internal class FormChiTietTienNghiEditor : Form
{
    private readonly ComboBox _roomType = new() { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
    private readonly ComboBox _amenity = new() { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
    private readonly NumericUpDown _quantity = new() { Minimum = 0, Maximum = 1000, Value = 1, Dock = DockStyle.Left, Width = 120 };
    public string MaLoaiPhong => (_roomType.SelectedItem as ChiTietOption)?.Code ?? "";
    public string MaTienNghi => (_amenity.SelectedItem as ChiTietOption)?.Code ?? "";
    public int SoLuong => (int)_quantity.Value;

    protected FormChiTietTienNghiEditor(string title, IReadOnlyList<ChiTietOption> types, IReadOnlyList<ChiTietOption> amenities,
        string? typeCode = null, string? amenityCode = null, int quantity = 1, bool editing = false)
    {
        Text = title; StartPosition = FormStartPosition.CenterParent; FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = MinimizeBox = false; ClientSize = new Size(530, 255);
        _roomType.DisplayMember = nameof(ChiTietOption.Label); _roomType.ValueMember = nameof(ChiTietOption.Code); _roomType.DataSource = types.ToList();
        _amenity.DisplayMember = nameof(ChiTietOption.Label); _amenity.ValueMember = nameof(ChiTietOption.Code); _amenity.DataSource = amenities.ToList();
        SelectOption(_roomType, typeCode); SelectOption(_amenity, amenityCode);
        _amenity.Enabled = !editing; _quantity.Value = Math.Clamp(quantity, (int)_quantity.Minimum, (int)_quantity.Maximum);
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), ColumnCount = 2, RowCount = 4 };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 155)); root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        AddField(root, 0, "Loại phòng", _roomType); AddField(root, 1, "Tiện nghi", _amenity); AddField(root, 2, "Số lượng", _quantity);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        var save = HotelUi.Button("Lưu", true); var cancel = HotelUi.Button("Thoát");
        save.Click += (_, _) => { if (_roomType.SelectedItem is not ChiTietOption || _amenity.SelectedItem is not ChiTietOption) { MessageBox.Show("Cần có loại phòng và tiện nghi.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; } DialogResult = DialogResult.OK; Close(); };
        cancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        buttons.Controls.AddRange([cancel, save]); root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48)); root.Controls.Add(buttons, 0, 3); root.SetColumnSpan(buttons, 2); Controls.Add(root); AcceptButton = save; CancelButton = cancel;
    }

    private static void AddField(TableLayoutPanel root, int row, string label, Control input)
    {
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48)); root.Controls.Add(HotelUi.FieldLabel(label), 0, row); input.Margin = new Padding(5); root.Controls.Add(input, 1, row);
    }

    private static void SelectOption(ComboBox combo, string? code)
    {
        for (var i = 0; i < combo.Items.Count; i++)
        {
            if (combo.Items[i] is ChiTietOption option && string.Equals(option.Code, code, StringComparison.Ordinal))
            {
                combo.SelectedIndex = i;
                return;
            }
        }
        combo.SelectedIndex = combo.Items.Count > 0 ? 0 : -1;
    }
}

internal sealed class FormThemChiTietTienNghi : FormChiTietTienNghiEditor
{
    public FormThemChiTietTienNghi(IReadOnlyList<ChiTietOption> types, IReadOnlyList<ChiTietOption> amenities)
        : base("Thêm tiện nghi cho loại phòng", types, amenities) { }
}

