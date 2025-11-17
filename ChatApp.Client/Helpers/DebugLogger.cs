using System;
using System.Diagnostics;
using System.IO;

namespace ChatApp.Client.Helpers;

public static class DebugLogger
{
    private static readonly string LogFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ChatApp",
        "debug.log");

    static DebugLogger()
    {
        // Ensure log directory exists
        var logDir = Path.GetDirectoryName(LogFilePath);
        if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
        {
            Directory.CreateDirectory(logDir);
        }
    }

    public static void Log(string tag, string message)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var logMessage = $"[{timestamp}] [{tag}] {message}";
        
        // Output to Visual Studio Output window
        Debug.WriteLine(logMessage);
        
        // Also output to console (for command line runs)
        Console.WriteLine(logMessage);
        
        // Also output to Trace (for Avalonia's LogToTrace)
        Trace.WriteLine(logMessage);
        
        // Also write to file for guaranteed visibility
        try
        {
            File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
        }
        catch
        {
            // Ignore file write errors
        }
    }
}

