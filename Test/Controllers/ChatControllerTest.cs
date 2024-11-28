using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using ChatApp.Server.API.Controllers;
using ChatApp.Server.Application.Interfaces;
using ChatApp.Server.Application.DTOs;
using ChatApp.Server.Domain.Entities;
using ChatApp.Server.Domain.ValueObjects;
using System;
using System.Threading.Tasks;

public class ChatControllerTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IChatService> _mockChatService;
    private readonly ChatController _controller;

    public ChatControllerTests()
    {
        _mockUserService = new Mock<IUserService>();
        _mockChatService = new Mock<IChatService>();
        _controller = new ChatController(_mockChatService.Object, _mockUserService.Object);
    }

    #region RegisterUser Tests
    [Fact]
    public async Task RegisterUser_ReturnsOkResult_WhenUserIsSuccessfullyRegistered()
    {
        // Arrange
        var registerDto = new RegisterUserDto
        {
            Username = "testuser",
            DisplayName = "Test User",
            Email = "testuser@example.com",
            Password = "password123"
        };
        
        var user = new User("testuser", new Email("testuser@example.com"), "Test User", "password123");

        _mockUserService.Setup(service => service.RegisterUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                        .ReturnsAsync(user);

        // Act
        var result = await _controller.RegisterUser(registerDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("User registered successfully.", okResult.Value);
    }

    [Fact]
    public async Task RegisterUser_ReturnsBadRequest_WhenUserAlreadyExists()
    {
        // Arrange
        var registerDto = new RegisterUserDto
        {
            Username = "testuser",
            DisplayName = "Test User",
            Email = "testuser@example.com",
            Password = "password123"
        };

        _mockUserService.Setup(service => service.RegisterUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                        .ReturnsAsync((User)null);

        // Act
        var result = await _controller.RegisterUser(registerDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Username or email already exists.", badRequestResult.Value);
    }
    #endregion

    #region LoginUser Tests
    [Fact]
    public async Task LoginUser_ReturnsOkResult_WhenLoginIsSuccessful()
    {
        // Arrange
        var loginDto = new LoginUserDto
        {
            Username = "testuser",
            Password = "password123"
        };

        var user = new User("testuser", new Email("testuser@example.com"), "Test User", "password123");

        _mockUserService.Setup(service => service.LoginUserAsync(It.IsAny<string>(), It.IsAny<string>()))
                        .ReturnsAsync(user);

        // Act
        var result = await _controller.LoginUser(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Login successful.", okResult.Value);
    }

    [Fact]
    public async Task LoginUser_ReturnsUnauthorized_WhenLoginFails()
    {
        // Arrange
        var loginDto = new LoginUserDto
        {
            Username = "testuser",
            Password = "wrongpassword"
        };

        _mockUserService.Setup(service => service.LoginUserAsync(It.IsAny<string>(), It.IsAny<string>()))
                        .ReturnsAsync((User)null);

        // Act
        var result = await _controller.LoginUser(loginDto);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Invalid username or password.", unauthorizedResult.Value);
    }
    #endregion
    #region GetRecentMessages Tests
    [Fact]
    public async Task GetRecentChats_ReturnsOk_WhenNoRecentChatsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockChatService.Setup(service => service.GetRecentChatsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(Enumerable.Empty<PrivateChatDto>());  // Simulate no recent chats

        // Act
        var result = await _controller.GetRecentChats(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<NotFoundResponseDto>(okResult.Value); // Get the actual value from OkObjectResult
        Assert.Equal("empty", response.Status);  // Assert the status
        Assert.Equal("No recent chats found.", response.Message);  // Assert the message
    }

    [Fact]
    public async Task GetRecentChats_ReturnsOk_WhenRecentChatsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var recentChats = new List<PrivateChatDto>
        {
            new PrivateChatDto { UserId = Guid.NewGuid(), DisplayName = "User1", LastMessageContent = "Hi!", LastMessageTimestamp = DateTime.Now }
        };
        _mockChatService.Setup(service => service.GetRecentChatsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(recentChats);

        // Act
        var result = await _controller.GetRecentChats(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<PrivateChatDto>>(okResult.Value);
        Assert.Single(returnValue);
    }
    #endregion

    #region GetPrivateMessages Tests
    [Fact]
    public async Task GetPrivateMessages_ReturnsNotFound_WhenNoMessagesExist()
    {
        // Arrange
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();
        _mockChatService.Setup(service => service.GetPrivateMessagesAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(Enumerable.Empty<MessageDto>());  // Simulate no messages

        // Act
        var result = await _controller.GetPrivateMessages(user1Id, user2Id);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<NotFoundResponseDto>(notFoundResult.Value);  // 强类型断言
        Assert.Equal("empty", response.Status);
        Assert.Equal("No messages found.", response.Message);
    }



    
    [Fact]
    public async Task GetPrivateMessages_ReturnsOk_WhenMessagesExist()
    {
        // Arrange
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();
        var messages = new List<MessageDto>
        {
            new MessageDto { Id = Guid.NewGuid(), SenderId = user1Id, ReceiverId = user2Id, Content = "Hello", Timestamp = DateTime.Now }
        };
        _mockChatService.Setup(service => service.GetPrivateMessagesAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(messages);

        // Act
        var result = await _controller.GetPrivateMessages(user1Id, user2Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<MessageDto>>(okResult.Value);
        Assert.Single(returnValue);
    }
    

    #endregion

    
}
