using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using ChatApp.Client.Services;
using ReactiveUI;

namespace ChatApp.Client.ViewModels;

public class ThemeOption
{
    public ThemeOption(ThemeSelection selection, string name, string description)
    {
        Selection = selection;
        Name = name;
        Description = description;
    }

    public ThemeSelection Selection { get; }
    public string Name { get; }
    public string Description { get; }
}

public class AppearanceSettingsViewModel : ReactiveObject
{
    private readonly Action _closeAction;
    private ThemeOption? _selectedTheme;

    public AppearanceSettingsViewModel(Action closeAction)
    {
        _closeAction = closeAction;
        Themes = new[]
        {
            new ThemeOption(ThemeSelection.ClassicDark, "Classic Dark", "Deep night palette with Nunito font"),
            new ThemeOption(ThemeSelection.BreezeLight, "Breeze Light", "Bright daylight palette with Segoe UI")
        };

        var current = ThemeManager.Current;
        SelectedTheme = Themes.FirstOrDefault(t => t.Selection == current) ?? Themes[0];

        CloseCommand = ReactiveCommand.Create(_closeAction);
    }

    public IReadOnlyList<ThemeOption> Themes { get; }

    public ThemeOption? SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedTheme, value);
            if (value != null)
            {
                ThemeManager.ApplyTheme(value.Selection);
            }
        }
    }

    public ReactiveCommand<Unit, Unit> CloseCommand { get; }
}
