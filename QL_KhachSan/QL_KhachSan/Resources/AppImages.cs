using System.Collections.Concurrent;

namespace QL_KhachSan;

internal static class AppImages
{
    private static readonly ConcurrentDictionary<string, Image> Cache = new(StringComparer.OrdinalIgnoreCase);

    public static Image Get(string name)
    {
        var key = name.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ? name : name + ".png";
        return Cache.GetOrAdd(key, fileName =>
        {
            var resourceName = Path.GetFileNameWithoutExtension(fileName);
            if (Properties.Resources.GetObject(resourceName) is Image embeddedImage)
                return embeddedImage;

            var path = Path.Combine(AppContext.BaseDirectory, "Resources", fileName);
            if (!File.Exists(path))
                throw new FileNotFoundException($"Không tìm thấy hình ảnh tài nguyên: {fileName}", path);
            using var source = Image.FromFile(path);
            return new Bitmap(source);
        });
    }
}
