using System;
using System.IO;
using System.Text.Json;
using ChatApp.Client.Helpers;

namespace ChatApp.Client.Services;

public static class NotificationSettingsService
{
    private static readonly string SettingsDirectory =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ChatApp");

    private static readonly string SettingsFilePath = Path.Combine(SettingsDirectory, "notificationSettings.json");

    private static readonly object SyncRoot = new();
    private static NotificationSettings _current = new();
    private static bool _initialized;

    public static event Action<NotificationSettings>? SettingsChanged;

    static NotificationSettingsService()
    {
        Load();
    }

    private static void Load()
    {
        lock (SyncRoot)
        {
            if (_initialized)
            {
                return;
            }

            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    var json = File.ReadAllText(SettingsFilePath);
                    var settings = JsonSerializer.Deserialize<NotificationSettings>(json);
                    if (settings != null)
                    {
                        _current = settings;
                    }
                }
            }
            catch (Exception ex)
            {
                DebugLogger.Log("NotificationSettingsService", $"Failed to load settings: {ex.Message}");
            }
            finally
            {
                _initialized = true;
            }
        }
    }

    private static void Save()
    {
        try
        {
            Directory.CreateDirectory(SettingsDirectory);
            var json = JsonSerializer.Serialize(_current, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFilePath, json);
        }
        catch (Exception ex)
        {
            DebugLogger.Log("NotificationSettingsService", $"Failed to save settings: {ex.Message}");
        }
    }

    public static NotificationSettings GetSnapshot()
    {
        Load();
        return new NotificationSettings
        {
            MuteAll = _current.MuteAll,
            EnableSound = _current.EnableSound
        };
    }

    public static void Update(NotificationSettings settings)
    {
        Load();
        lock (SyncRoot)
        {
            _current = new NotificationSettings
            {
                MuteAll = settings.MuteAll,
                EnableSound = settings.EnableSound
            };
            Save();
        }

        SettingsChanged?.Invoke(GetSnapshot());
    }

    public static void HandleIncomingMessage()
    {
        Load();
        if (_current.MuteAll)
        {
            return;
        }

        if (_current.EnableSound)
        {
            MessageSoundPlayer.Play();
        }
    }
}

public class NotificationSettings
{
    public bool MuteAll { get; set; }
    public bool EnableSound { get; set; } = true;
}
