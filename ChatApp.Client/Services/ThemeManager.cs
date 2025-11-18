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
            var palette = ThemePalettes.For(selection);
            var font = selection switch
            {
                ThemeSelection.BreezeLight => new FontFamily("Segoe UI"),
                _ => new FontFamily("avares://ChatApp.Client/Assets/Fonts#Nunito")
            };

            app.Resources["AppFontFamily"] = font;
            SetBrush(app, "ThemeBrush.WindowBackground", palette.WindowBackground);
            SetBrush(app, "ThemeBrush.SidebarBackground", palette.SidebarBackground);
            SetBrush(app, "ThemeBrush.CardBackground", palette.CardBackground);
            SetBrush(app, "ThemeBrush.InputBackground", palette.InputBackground);
            SetBrush(app, "ThemeBrush.Accent", palette.Accent);
            SetBrush(app, "ThemeBrush.AccentText", palette.AccentText);
            SetBrush(app, "ThemeBrush.AccentSoft", palette.AccentSoft);
            SetBrush(app, "ThemeBrush.PrimaryText", palette.PrimaryText);
            SetBrush(app, "ThemeBrush.MutedText", palette.MutedText);
            SetBrush(app, "ThemeBrush.Border", palette.Border);
            SetBrush(app, "ThemeBrush.SecondaryButton", palette.SecondaryButton);
            SetBrush(app, "ThemeBrush.SecondaryButtonBorder", palette.SecondaryButtonBorder);
            SetBrush(app, "ThemeBrush.Danger", palette.Danger);
            SetBrush(app, "ThemeBrush.Warning", palette.Warning);
            SetBrush(app, "ThemeBrush.Success", palette.Success);
            app.Resources["SystemAccentColor"] = palette.Accent;
            app.Resources["ChatBackground"] = app.Resources["ThemeBrush.WindowBackground"];
            app.Resources["EntryBackground"] = app.Resources["ThemeBrush.InputBackground"];
            app.Resources["SecondaryButtonBackground"] = app.Resources["ThemeBrush.SecondaryButton"];
            app.Resources["SecondaryButtonBorder"] = app.Resources["ThemeBrush.SecondaryButtonBorder"];
            app.Resources["PrimaryTextBrush"] = app.Resources["ThemeBrush.PrimaryText"];
            app.Resources["MutedTextBrush"] = app.Resources["ThemeBrush.MutedText"];
            app.Resources["DangerBrush"] = app.Resources["ThemeBrush.Danger"];
            app.Resources["WarningBrush"] = app.Resources["ThemeBrush.Warning"];
            app.Resources["CardBackgroundBrush"] = app.Resources["ThemeBrush.CardBackground"];
            app.Resources["SidebarBackgroundBrush"] = app.Resources["ThemeBrush.SidebarBackground"];
            app.Resources["AccentBrush"] = app.Resources["ThemeBrush.Accent"];
            app.RequestedThemeVariant = selection == ThemeSelection.BreezeLight
                ? ThemeVariant.Light
                : ThemeVariant.Dark;
        }

        if (persist)
        {
            ThemeSettingsService.Save(selection);
        }
    }

    private static void SetBrush(Application app, string resourceKey, Color color)
    {
        app.Resources[resourceKey] = new SolidColorBrush(color);
    }
}
