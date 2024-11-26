using Xunit;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using ChatApp.Server.API.Hubs;
using ChatApp.Server.Application.DTOs;
using ChatApp.Server.Application.Interfaces;
using System.Collections.Concurrent;




public class ChatHubTests
{
    private readonly Mock<IChatService> _mockChatService;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IHubCallerClients> _mockClients;
    private readonly Mock<ISingleClientProxy> _mockClientProxy;
    private readonly ChatHub _chatHub;
    
    
    public ChatHubTests()
    {
        _mockChatService = new Mock<IChatService>();
        _mockUserService = new Mock<IUserService>();
        _mockClients = new Mock<IHubCallerClients>();
        _mockClientProxy = new Mock<ISingleClientProxy>();

        // 创建 ChatHub 实例
        _chatHub = new ChatHub(_mockChatService.Object, _mockUserService.Object)
        {
            Clients = _mockClients.Object,
            Context = Mock.Of<HubCallerContext>(c => c.ConnectionId == "test-connection-id")
        };

        // 设置 Mock 返回值
        _mockClients.Setup(c => c.Client(It.IsAny<string>())).Returns(_mockClientProxy.Object);
    }
    
    
    [Fact]
    public void RegisterUser_ShouldAddUserToOnlineUsers()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        _chatHub.RegisterUser(userId);

        // Assert
        var onlineUsers = typeof(ChatHub)
            .GetField("_onlineUsers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?.GetValue(null) as ConcurrentDictionary<Guid, string>;

        Assert.NotNull(onlineUsers);
        Assert.True(onlineUsers.ContainsKey(userId));
        Assert.Equal("test-connection-id", onlineUsers[userId]);
    }
    
    [Fact]
    public async Task OnDisconnectedAsync_ShouldRemoveUserFromOnlineUsers()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var onlineUsers = typeof(ChatHub)
            .GetField("_onlineUsers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?.GetValue(null) as ConcurrentDictionary<Guid, string>;

        Assert.NotNull(onlineUsers);
        onlineUsers[userId] = "test-connection-id";

        // Act
        await _chatHub.OnDisconnectedAsync(null);

        // Assert
        Assert.False(onlineUsers.ContainsKey(userId));
    }
    
    [Fact]
    public async Task SendMessage_ShouldCallClientWithMessage()
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var receiverId = Guid.NewGuid();
        var messageDto = new MessageDto
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Content = "Hello"
        };

        // 模拟在线用户和连接 ID
        var onlineUsers = typeof(ChatHub)
            .GetField("_onlineUsers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?.GetValue(null) as ConcurrentDictionary<Guid, string>;

        Assert.NotNull(onlineUsers);
        onlineUsers[receiverId] = "receiver-connection-id";

        // Mock Client 方法返回值
        _mockClients.Setup(c => c.Client("receiver-connection-id")).Returns(_mockClientProxy.Object);

        // 模拟 SendCoreAsync 行为
        _mockClientProxy.Setup(proxy => proxy.SendCoreAsync(
                It.Is<string>(method => method == "ReceiveMessage"),
                It.Is<object[]>(args => args[0] == messageDto),
                default))
            .Returns(Task.CompletedTask);

        // Act
        await _chatHub.SendMessage(messageDto);

        // Assert
        _mockClientProxy.Verify(
            proxy => proxy.SendCoreAsync("ReceiveMessage", It.Is<object[]>(o => o[0] == messageDto), default),
            Times.Once
        );
    }

}