using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using ReactiveUI;
using ChatApp.Client.DTOs;
using ChatApp.Client.Services;
using ChatApp.Client.Helpers;
using System.Threading.Tasks;

namespace ChatApp.Client.ViewModels;

public class EditProfileViewModel : ReactiveObject
{
    private readonly IChatService _chatService;
    private readonly Guid _userId;

    public EditProfileViewModel()
    {
        _chatService = null!;
        _userId = Guid.Empty;
        SystemAvatars = Array.Empty<SystemAvatarOption>();
        UseSystemAvatarCommand = ReactiveCommand.Create<string>(_ => { });
        PickAvatarCommand = ReactiveCommand.Create(() => { });
        SaveCommand = ReactiveCommand.Create(() => { });
        CancelCommand = ReactiveCommand.Create(() => { });
    }

    public EditProfileViewModel(Guid userId, string displayName, string? bio, IChatService chatService)
    {
        _userId = userId;
        _chatService = chatService;
        DisplayName = displayName;
        Bio = bio;
        _ = LoadCachedAvatarAsync(userId);
        SystemAvatars = LoadSystemAvatars();

        PickAvatarCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var topLevel = Avalonia.Application.Current?.ApplicationLifetime switch
            {
                Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime d => d.MainWindow,
                _ => null
            } as Avalonia.Controls.Window;

            if (topLevel == null) return;
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                AllowMultiple = false,
                FileTypeFilter = new[] { new FilePickerFileType("Images") { Patterns = new[] { "*.png", "*.jpg", "*.jpeg" } } }
            });
            var file = files.Count > 0 ? files[0] : null;
            if (file != null)
            {
                await using var stream = await file.OpenReadAsync();
                using var mem = new MemoryStream();
                await stream.CopyToAsync(mem);
                var bytes = mem.ToArray();
                AvatarBase64 = Convert.ToBase64String(bytes);
                mem.Position = 0;
                AvatarPreview = new Bitmap(new MemoryStream(bytes));
            }
        });

        UseSystemAvatarCommand = ReactiveCommand.Create<string>(asset =>
        {
            if (string.IsNullOrWhiteSpace(asset))
            {
                return;
            }

            var bytes = SystemImageProvider.LoadAssetBytes(asset);
            if (bytes == null)
            {
                return;
            }

            AvatarBase64 = Convert.ToBase64String(bytes);
            AvatarPreview = new Bitmap(new MemoryStream(bytes));
        });

        SaveCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var update = new UpdateProfileDto { Username = DisplayName, DisplayName = DisplayName, Bio = Bio, AvatarBase64 = AvatarBase64 };
            
            var result = await _chatService.UpdateProfile(_userId, update);
            
            if (result == null)
            {
                return;
            }
            
            if (!string.IsNullOrEmpty(AvatarBase64))
            {
                try { await AvatarCache.SaveAsync(_userId, Convert.FromBase64String(AvatarBase64)); } catch { }
            }
            
            // Trigger profile update event so other views can refresh
            ProfileEvents.RaiseProfileUpdated(_userId, DisplayName);
            
            CloseRequested?.Invoke(true);
        });

        CancelCommand = ReactiveCommand.Create(() => CloseRequested?.Invoke(false));
    }

    private async Task LoadCachedAvatarAsync(Guid userId)
    {
        try
        {
            var bytes = await AvatarCache.TryLoadAsync(userId);
            if (bytes != null && bytes.Length > 0)
            {
                AvatarPreview = new Bitmap(new MemoryStream(bytes));
            }
        }
        catch { }
    }

    private string _displayName = string.Empty;
    public string DisplayName { get => _displayName; set => this.RaiseAndSetIfChanged(ref _displayName, value); }

    public IReadOnlyList<SystemAvatarOption> SystemAvatars { get; }

    private string? _bio;
    public string? Bio { get => _bio; set => this.RaiseAndSetIfChanged(ref _bio, value); }

    private string? _avatarBase64;
    public string? AvatarBase64 { get => _avatarBase64; set => this.RaiseAndSetIfChanged(ref _avatarBase64, value); }

    private Bitmap? _avatarPreview;
    public Bitmap? AvatarPreview { get => _avatarPreview; set => this.RaiseAndSetIfChanged(ref _avatarPreview, value); }

    public ReactiveCommand<Unit, Unit> PickAvatarCommand { get; }
    public ReactiveCommand<string, Unit> UseSystemAvatarCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public event Action<bool>? CloseRequested;

    private static IReadOnlyList<SystemAvatarOption> LoadSystemAvatars()
    {
        var list = new List<SystemAvatarOption>();
        foreach (var asset in SystemImageProvider.Avatars)
        {
            var bitmap = SystemImageProvider.LoadAssetBitmap(asset);
            if (bitmap != null)
            {
                list.Add(new SystemAvatarOption(asset, bitmap));
            }
        }

        return list;
    }
}

public sealed class SystemAvatarOption
{
    public SystemAvatarOption(string assetName, Bitmap preview)
    {
        AssetName = assetName;
        Preview = preview;
    }

    public string AssetName { get; }
    public Bitmap Preview { get; }
}
