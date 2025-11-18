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
    private bool _isSearchingGroup;

    public SearchFriendsViewModel(Guid currentUserId, IChatService chatService)
    {
        try
        {
            Log("SearchFriendsViewModel", "Constructor: Starting initialization");
            _currentUserId = currentUserId;
            Log("SearchFriendsViewModel", $"Constructor: currentUserId={currentUserId}");
            
            _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService), "chatService cannot be null");
            Log("SearchFriendsViewModel", "Constructor: chatService assigned");
            
            SearchResults = new ObservableCollection<SearchUserResultDto>();
            GroupSearchResult = null;
            Log("SearchFriendsViewModel", "Constructor: SearchResults created");
            
            SearchCommand = ReactiveCommand.CreateFromTask(SearchAsync, this.WhenAnyValue(x => x.SearchTerm, term => !string.IsNullOrWhiteSpace(term)));
            Log("SearchFriendsViewModel", "Constructor: SearchCommand created");
            
            SearchGroupCommand = ReactiveCommand.CreateFromTask(SearchGroupAsync, this.WhenAnyValue(x => x.SearchTerm, term => !string.IsNullOrWhiteSpace(term)));
            Log("SearchFriendsViewModel", "Constructor: SearchGroupCommand created");
            
            SendFriendRequestCommand = ReactiveCommand.CreateFromTask<SearchUserResultDto>(SendFriendRequestAsync);
            Log("SearchFriendsViewModel", "Constructor: SendFriendRequestCommand created");
            
            Log("SearchFriendsViewModel", "Constructor: Completed successfully");
        }
        catch (Exception ex)
        {
            Log("SearchFriendsViewModel", $"Constructor ERROR: {ex.Message}");
            Log("SearchFriendsViewModel", $"StackTrace: {ex.StackTrace}");
            throw; // Re-throw to see the error
        }
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
    
    public bool IsSearchingGroup
    {
        get => _isSearchingGroup;
        set => this.RaiseAndSetIfChanged(ref _isSearchingGroup, value);
    }
    
    private GroupDto? _groupSearchResult;
    public GroupDto? GroupSearchResult
    {
        get => _groupSearchResult;
        set => this.RaiseAndSetIfChanged(ref _groupSearchResult, value);
    }

    public ObservableCollection<SearchUserResultDto> SearchResults { get; }

    public ReactiveCommand<Unit, Unit> SearchCommand { get; }
    public ReactiveCommand<Unit, Unit> SearchGroupCommand { get; }
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
    
    private async Task SearchGroupAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
            return;

        IsSearchingGroup = true;
        GroupSearchResult = null;

        try
        {
            var result = await _chatService.SearchGroupByCodeAsync(SearchTerm);
            GroupSearchResult = result;
            
            if (result != null)
            {
                Log("SearchFriendsViewModel", $"Found group: {result.Name} ({result.GroupCode})");
            }
            else
            {
                Log("SearchFriendsViewModel", "No group found with that code");
            }
        }
        catch (Exception ex)
        {
            Log("SearchFriendsViewModel", $"SearchGroup error: {ex.Message}");
        }
        finally
        {
            IsSearchingGroup = false;
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

