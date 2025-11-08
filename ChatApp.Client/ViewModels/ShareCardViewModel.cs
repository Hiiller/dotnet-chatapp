using System;
using System.IO;
using System.Reactive;
using Avalonia.Media.Imaging;
using ReactiveUI;
using ChatApp.Client.Services;
using ChatApp.Client.Helpers;
using System.Threading.Tasks;

namespace ChatApp.Client.ViewModels;

public class ShareCardViewModel : ReactiveObject
{
    private readonly IChatService _chatService;

    public ShareCardViewModel() { _chatService = null!; CloseCommand = ReactiveCommand.Create(() => { }); }

    public ShareCardViewModel(Guid userId, string displayName, string personalCode, IChatService chatService)
    {
        _chatService = chatService;
        UserId = userId.ToString();
        DisplayName = displayName;
        PersonalCode = personalCode;
        CloseCommand = ReactiveCommand.Create(() => CloseRequested?.Invoke());

        _ = LoadAvatarAsync(userId);
    }

    private async Task LoadAvatarAsync(Guid userId)
    {
        try
        {
            // Try cache first
            var bytes = await AvatarCache.TryLoadAsync(userId) ?? await _chatService.GetAvatar(userId);
            if (bytes != null && bytes.Length > 0)
            {
                AvatarPreview = new Bitmap(new MemoryStream(bytes));
                try { await AvatarCache.SaveAsync(userId, bytes); } catch { }
            }
        }
        catch { }
    }

    private string _userId = string.Empty;
    public string UserId { get => _userId; set => this.RaiseAndSetIfChanged(ref _userId, value); }

    private string _displayName = string.Empty;
    public string DisplayName { get => _displayName; set => this.RaiseAndSetIfChanged(ref _displayName, value); }

    private string _personalCode = string.Empty;
    public string PersonalCode { get => _personalCode; set => this.RaiseAndSetIfChanged(ref _personalCode, value); }

    private Bitmap? _avatarPreview;
    public Bitmap? AvatarPreview { get => _avatarPreview; set => this.RaiseAndSetIfChanged(ref _avatarPreview, value); }

    public ReactiveCommand<Unit, Unit> CloseCommand { get; }
    public event Action? CloseRequested;
}
