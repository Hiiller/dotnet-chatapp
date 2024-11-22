//ChatApp.Client/Program.cs
using Avalonia;
using System;

namespace ChatApp.Client
{
    internal class Program
    {
        // 应用程序入口点
        [STAThread]
        public static void Main(string[] args)
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }

        // 配置 Avalonia 应用程序
        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
        }
    }
}
