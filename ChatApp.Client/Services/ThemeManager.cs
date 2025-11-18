using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;

namespace ChatApp.Client.Services;

public static class ThemeManager
{
    public static ThemeSelection Current { get; private set; } = ThemeSelection.ClassicDark;

    public static void Initialize()
    {
        var saved = ThemeSettingsService.Load();
        ApplyTheme(saved, persist: false);
    }

    public static void ApplyTheme(ThemeSelection selection, bool persist = true)
    {
        Current = selection;

        var app = Application.Current;
        if (app != null)
        {
            var font = selection switch
            {
                ThemeSelection.BreezeLight => new FontFamily("Segoe UI"),
                _ => new FontFamily("avares://ChatApp.Client/Assets/Fonts#Nunito")
            };

            app.Resources["AppFontFamily"] = font;
            app.RequestedThemeVariant = selection == ThemeSelection.BreezeLight
                ? ThemeVariant.Light
                : ThemeVariant.Dark;
        }

        if (persist)
        {
            ThemeSettingsService.Save(selection);
        }
    }
}
