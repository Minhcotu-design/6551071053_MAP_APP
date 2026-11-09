using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QL_KhachSan.DangNhap;
using QL_KhachSan.Models;

namespace QL_KhachSan.TaiKhoan;

internal sealed class FormSuaTaiKhoan : AccountEditorForm
{
    public FormSuaTaiKhoan(string username):base(username){}
}
