using System.Resources;

namespace QL_KhachSan.Properties;

internal static class Resources
{
    private static readonly ResourceManager Manager = new("QL_KhachSan.Properties.Resources", typeof(Resources).Assembly);

    public static object? GetObject(string name) => Manager.GetObject(name);
}
