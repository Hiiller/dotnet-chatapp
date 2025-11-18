using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace ChatApp.Client.Helpers;

public static class SystemImageProvider
{
    private static readonly string[] AvatarAssets =
    {
        "head1.png","head2.png","head3.png","head4.png","head5.png",
        "head6.png","head7.png","head8.png","head9.png"
    };

    private static readonly string[] EmojiAssets =
    {
        "emj1.png","emj2.png","emj3.png","emj4.png","emj5.png","emj6.png","emj7.png",
        "emj8.png","emj9.png","emj10.png","emj11.png","emj12.png","emj13.png","emj14.png"
    };

    private static readonly Random Randomizer = new();

    public static IReadOnlyList<string> Avatars => AvatarAssets;
    public static IReadOnlyList<string> Emojis => EmojiAssets;

    public static string GetRandomAvatar() => AvatarAssets[Randomizer.Next(AvatarAssets.Length)];

    public static byte[]? LoadAssetBytes(string assetName)
    {
        try
        {
            var uri = new Uri($"avares://ChatApp.Client/Assets/{assetName}");
            using var stream = AssetLoader.Open(uri);
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }
        catch
        {
            return null;
        }
    }

    public static Bitmap? LoadAssetBitmap(string assetName)
    {
        var bytes = LoadAssetBytes(assetName);
        if (bytes == null || bytes.Length == 0)
        {
            return null;
        }

        return new Bitmap(new MemoryStream(bytes));
    }

    public static string? SaveAssetToTemp(string assetName)
    {
        var bytes = LoadAssetBytes(assetName);
        if (bytes == null || bytes.Length == 0)
        {
            return null;
        }

        var tempPath = Path.Combine(Path.GetTempPath(), $"chatapp_{Guid.NewGuid():N}_{assetName}");
        File.WriteAllBytes(tempPath, bytes);
        return tempPath;
    }
}
