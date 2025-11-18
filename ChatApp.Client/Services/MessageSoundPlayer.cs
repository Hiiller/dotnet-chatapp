using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform;
using ChatApp.Client.Helpers;
using NAudio.Wave;

namespace ChatApp.Client.Services;

public static class MessageSoundPlayer
{
    private static readonly object InitLock = new();
    private static string? _cachedFilePath;

    private static string? EnsureSoundFile()
    {
        if (_cachedFilePath != null && File.Exists(_cachedFilePath))
        {
            return _cachedFilePath;
        }

        lock (InitLock)
        {
            if (_cachedFilePath != null && File.Exists(_cachedFilePath))
            {
                return _cachedFilePath;
            }

            try
            {
                var uri = new Uri("avares://ChatApp.Client/Assets/messagering.wav");
                using var stream = AssetLoader.Open(uri);
                var tempPath = Path.Combine(Path.GetTempPath(), "chatapp_messagering.wav");
                using var file = File.Create(tempPath);
                stream.CopyTo(file);
                _cachedFilePath = tempPath;
                return _cachedFilePath;
            }
            catch (Exception ex)
            {
                DebugLogger.Log("MessageSoundPlayer", $"Failed to prepare sound file: {ex.Message}");
                return null;
            }
        }
    }

    public static void Play()
    {
        Task.Run(() =>
        {
            try
            {
                var soundFile = EnsureSoundFile();
                if (string.IsNullOrEmpty(soundFile))
                {
                    return;
                }

                using var reader = new MediaFoundationReader(soundFile);
                using var output = new WaveOutEvent();
                output.Init(reader);
                output.Play();
                while (output.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(50);
                }
            }
            catch (Exception ex)
            {
                DebugLogger.Log("MessageSoundPlayer", $"Failed to play sound: {ex.Message}");
            }
        });
    }
}
