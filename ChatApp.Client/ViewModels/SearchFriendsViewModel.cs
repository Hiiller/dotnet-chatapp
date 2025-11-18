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
    private GroupDto? _groupSearchResult;
    private string _groupStatusMessage = string.Empty;
    private bool _isSubmittingRequest;

    public SearchFriendsViewModel(Guid currentUserId, IChatService chatService)
    {
        Log("SearchFriendsViewModel", "Constructor: Starting initialization");
        _currentUserId = currentUserId;
        _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
        
        SearchResults = new ObservableCollection<SearchUserResultDto>();
        
        SearchCommand = ReactiveCommand.CreateFromTask(SearchAsync,
            this.WhenAnyValue(x => x.SearchTerm, term => !string.IsNullOrWhiteSpace(term)));
        SendFriendRequestCommand = ReactiveCommand.CreateFromTask<SearchUserResultDto>(SendFriendRequestAsync);
        
        var canJoin = this.WhenAnyValue(x => x.GroupSearchResult, x => x.IsSubmittingRequest,
            (group, busy) => group != null && !group.IsMember && !group.HasPendingRequest && !busy);
        RequestJoinGroupCommand = ReactiveCommand.CreateFromTask(RequestJoinGroupAsync, canJoin);
        
        Log("SearchFriendsViewModel", "Constructor: Completed successfully");
    }

    public string SearchTerm
    {
        get => _searchTerm;
        set => this.RaiseAndSetIfChanged(ref _searchTerm, value);
    }

    public bool IsSearching
    {
        get => _isSearching;
        set
        {
            this.RaiseAndSetIfChanged(ref _isSearching, value);
            this.RaisePropertyChanged(nameof(CanStartSearch));
        }
    }
    
    public bool CanStartSearch => !IsSearching;

    public GroupDto? GroupSearchResult
    {
        get => _groupSearchResult;
        set
        {
            this.RaiseAndSetIfChanged(ref _groupSearchResult, value);
            this.RaisePropertyChanged(nameof(CanRequestJoin));
        }
    }

    public bool CanRequestJoin => GroupSearchResult != null && !GroupSearchResult.IsMember && !GroupSearchResult.HasPendingRequest;

    public string GroupStatusMessage
    {
        get => _groupStatusMessage;
        set => this.RaiseAndSetIfChanged(ref _groupStatusMessage, value);
    }

    public bool IsSubmittingRequest
    {
        get => _isSubmittingRequest;
        set => this.RaiseAndSetIfChanged(ref _isSubmittingRequest, value);
    }

    public ObservableCollection<SearchUserResultDto> SearchResults { get; }

    public ReactiveCommand<Unit, Unit> SearchCommand { get; }
    public ReactiveCommand<SearchUserResultDto, Unit> SendFriendRequestCommand { get; }
    public ReactiveCommand<Unit, Unit> RequestJoinGroupCommand { get; }

    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
            return;

        IsSearching = true;
        GroupStatusMessage = string.Empty;
        GroupSearchResult = null;
        SearchResults.Clear();

        try
        {
            var userTask = _chatService.SearchUsersAsync(SearchTerm);
            var groupTask = _chatService.SearchGroupByCodeAsync(SearchTerm, _currentUserId);

            var users = await userTask;
            foreach (var result in users)
            {
                if (result.Id != _currentUserId)
                {
                    SearchResults.Add(result);
                }
            }

            GroupSearchResult = await groupTask;
            if (GroupSearchResult == null)
            {
                GroupStatusMessage = "未找到匹配的群组";
            }
            else if (GroupSearchResult.IsMember)
            {
                GroupStatusMessage = "你已经加入该群";
            }
            else if (GroupSearchResult.HasPendingRequest)
            {
                GroupStatusMessage = "已发送加群请求，等待审批";
            }
            else
            {
                GroupStatusMessage = "找到可加入的群，点击申请加入";
            }
        }
        catch (Exception ex)
        {
            GroupStatusMessage = $"搜索失败：{ex.Message}";
            Log("SearchFriendsViewModel", $"Search error: {ex.Message}");
        }
        finally
        {
            IsSearching = false;
        }
    }

    private async Task RequestJoinGroupAsync()
    {
        if (GroupSearchResult == null)
        {
            return;
        }

        try
        {
            IsSubmittingRequest = true;
            var response = await _chatService.RequestToJoinGroupAsync(GroupSearchResult.Id, _currentUserId);
            if (response != null)
            {
                GroupSearchResult.HasPendingRequest = true;
                GroupStatusMessage = "加群请求已发送";
                this.RaisePropertyChanged(nameof(GroupSearchResult));
                this.RaisePropertyChanged(nameof(CanRequestJoin));
            }
            else
            {
                GroupStatusMessage = "加群请求发送失败";
            }
        }
        catch (Exception ex)
        {
            GroupStatusMessage = $"加群失败：{ex.Message}";
        }
        finally
        {
            IsSubmittingRequest = false;
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
            }
            else
            {
                Log("SearchFriendsViewModel", $"Failed to send friend request to {user.Username}");
            }
        }
        catch (Exception ex)
        {
            Log("SearchFriendsViewModel", $"SendFriendRequest error: {ex.Message}");
        }
    }
}
