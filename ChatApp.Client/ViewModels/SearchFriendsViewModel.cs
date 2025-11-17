using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using ChatApp.Client.DTOs;
using ChatApp.Client.Services;
using ReactiveUI;
using static ChatApp.Client.Helpers.DebugLogger;

namespace ChatApp.Client.ViewModels;

public class SearchFriendsViewModel : ReactiveObject
{
    private readonly IChatService _chatService;
    private readonly Guid _currentUserId;
    private string _searchTerm = string.Empty;
    private bool _isSearching;

    public SearchFriendsViewModel(Guid currentUserId, IChatService chatService)
    {
        _currentUserId = currentUserId;
        _chatService = chatService;
        SearchResults = new ObservableCollection<SearchUserResultDto>();
        
        SearchCommand = ReactiveCommand.CreateFromTask(SearchAsync, this.WhenAnyValue(x => x.SearchTerm, term => !string.IsNullOrWhiteSpace(term)));
        SendFriendRequestCommand = ReactiveCommand.CreateFromTask<SearchUserResultDto>(SendFriendRequestAsync);
    }

    public string SearchTerm
    {
        get => _searchTerm;
        set => this.RaiseAndSetIfChanged(ref _searchTerm, value);
    }

    public bool IsSearching
    {
        get => _isSearching;
        set => this.RaiseAndSetIfChanged(ref _isSearching, value);
    }

    public ObservableCollection<SearchUserResultDto> SearchResults { get; }

    public ReactiveCommand<Unit, Unit> SearchCommand { get; }
    public ReactiveCommand<SearchUserResultDto, Unit> SendFriendRequestCommand { get; }

    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
            return;

        IsSearching = true;
        SearchResults.Clear();

        try
        {
            var results = await _chatService.SearchUsersAsync(SearchTerm);
            foreach (var result in results)
            {
                // 排除自己
                if (result.Id != _currentUserId)
                {
                    SearchResults.Add(result);
                }
            }
        }
        catch (Exception ex)
        {
            Log("SearchFriendsViewModel", $"Search error: {ex.Message}");
        }
        finally
        {
            IsSearching = false;
        }
    }

    private async Task SendFriendRequestAsync(SearchUserResultDto user)
    {
        try
        {
            var dto = new SendFriendRequestDto
            {
                RequesterId = _currentUserId,
                ReceiverUsername = user.Username
            };

            var result = await _chatService.SendFriendRequestAsync(dto);
            if (result != null)
            {
                Log("SearchFriendsViewModel", $"Friend request sent to {user.Username}");
                // TODO: 显示成功消息或更新UI
            }
            else
            {
                Log("SearchFriendsViewModel", $"Failed to send friend request to {user.Username}");
                // TODO: 显示错误消息
            }
        }
        catch (Exception ex)
        {
            Log("SearchFriendsViewModel", $"SendFriendRequest error: {ex.Message}");
        }
    }
}

