using Microsoft.EntityFrameworkCore;

namespace QL_KhachSan.Models;

// Separate partial file so EF Core Power Tools can regenerate the scaffolded context safely.
public partial class HotelManagementContext
{
    private const string ConnectionString =
        "Server=.\\SQLEXPRESS;Database=HotelManagement;Trusted_Connection=True;TrustServerCertificate=True;";

    public HotelManagementContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlServer(ConnectionString);
    }
}
