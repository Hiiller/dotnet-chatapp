using Avalonia.Media;

namespace ChatApp.Client.Services;

internal record ThemePalette(
    Color WindowBackground,
    Color SidebarBackground,
    Color CardBackground,
    Color InputBackground,
    Color Accent,
    Color AccentText,
    Color AccentSoft,
    Color PrimaryText,
    Color MutedText,
    Color Border,
    Color SecondaryButton,
    Color SecondaryButtonBorder,
    Color Danger,
    Color Warning,
    Color Success);

internal static class ThemePalettes
{
    private static readonly ThemePalette ClassicDark = new(
        Color.Parse("#0B1623"),
        Color.Parse("#111B2A"),
        Color.Parse("#1A2742"),
        Color.Parse("#1E2126"),
        Color.Parse("#2D8EFF"),
        Colors.White,
        Color.Parse("#2A3A5A"),
        Color.Parse("#F5F7FB"),
        Color.Parse("#8EA0C5"),
        Color.Parse("#2A3A5A"),
        Color.Parse("#2A344D"),
        Color.Parse("#2A3A5A"),
        Color.Parse("#E14E4E"),
        Color.Parse("#FFB347"),
        Color.Parse("#2ECC71"));

    private static readonly ThemePalette BreezeLight = new(
        Color.Parse("#F4F7FB"),
        Color.Parse("#FFFFFF"),
        Color.Parse("#E4ECF5"),
        Color.Parse("#FFFFFF"),
        Color.Parse("#4C7EFF"),
        Colors.White,
        Color.Parse("#CBD5F0"),
        Color.Parse("#1F2430"),
        Color.Parse("#5A6378"),
        Color.Parse("#CED4E2"),
        Color.Parse("#E9EDF7"),
        Color.Parse("#CED4E2"),
        Color.Parse("#D9534F"),
        Color.Parse("#E79B36"),
        Color.Parse("#36B37E"));

    public static ThemePalette For(ThemeSelection selection) =>
        selection switch
        {
            ThemeSelection.BreezeLight => BreezeLight,
            _ => ClassicDark
        };
}
