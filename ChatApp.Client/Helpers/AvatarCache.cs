using System;
using System.IO;
using System.Threading.Tasks;

namespace ChatApp.Client.Helpers;

public static class AvatarCache
{
    private static string CacheDir
        => Path.Combine(AppContext.BaseDirectory, "AvatarCache");

    public static async Task SaveAsync(Guid userId, byte[] bytes)
    {
        Directory.CreateDirectory(CacheDir);
        var path = Path.Combine(CacheDir, userId + ".bin");
        await File.WriteAllBytesAsync(path, bytes);
    }

    public static async Task<byte[]?> TryLoadAsync(Guid userId)
    {
        var path = Path.Combine(CacheDir, userId + ".bin");
        if (File.Exists(path))
        {
            return await File.ReadAllBytesAsync(path);
        }
        return null;
    }
}

