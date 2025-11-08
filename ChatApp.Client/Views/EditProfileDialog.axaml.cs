using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ChatApp.Client.ViewModels;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using System;

namespace ChatApp.Client.Views;

public partial class EditProfileDialog : Window
{
    public EditProfileDialog()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async void OnPickAvatarClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not EditProfileViewModel vm) return;
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter = new[] { new FilePickerFileType("Images") { Patterns = new[] { "*.png", "*.jpg", "*.jpeg" } } }
        });
        var file = files.Count > 0 ? files[0] : null;
        if (file == null) return;
        await using var stream = await file.OpenReadAsync();
        using var mem = new System.IO.MemoryStream();
        await stream.CopyToAsync(mem);
        var bytes = mem.ToArray();
        vm.AvatarBase64 = Convert.ToBase64String(bytes);
        mem.Position = 0;
        vm.AvatarPreview = new Bitmap(new System.IO.MemoryStream(bytes));
    }
}
