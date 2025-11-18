using System;
using System.IO;
using System.Text.Json;

namespace ChatApp.Client.Services;

public enum ThemeSelection
{
    ClassicDark,
    BreezeLight
}

public static class ThemeSettingsService
{
    private static readonly string SettingsDirectory =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ChatApp");
    private static readonly string SettingsPath = Path.Combine(SettingsDirectory, "themeSettings.json");

    public static ThemeSelection Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                var stored = JsonSerializer.Deserialize<ThemeSelection>(json);
                return stored;
            }
        }
        catch
        {
            // ignore corrupted file
        }

        return ThemeSelection.ClassicDark;
    }

    public static void Save(ThemeSelection selection)
    {
        try
        {
            Directory.CreateDirectory(SettingsDirectory);
            var json = JsonSerializer.Serialize(selection);
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // ignore write failures
        }
    }
}
