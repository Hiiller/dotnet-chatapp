using ChatApp.Server.Domain.ValueObjects;
using ChatApp.Server.Infrastructure.Data;
using ChatApp.Server.Domain.Entities;
using ChatApp.Server.Tests.TestUtilities;
using Xunit;

namespace ChatApp.Server.Tests.Repositories
{
    public class MessageRepositoryTests
    {
        private readonly AppDbContext _context;

        public MessageRepositoryTests()
        {
            _context = TestDbContextFactory.CreateInMemoryDbContext();
        }

        [Fact]
        public async Task AddMessage_ValidMessage_ShouldAddMessageToDatabase()
        {
            // Arrange
            var message = new Message(
                senderId: Guid.NewGuid(),
                content: "Hello, World!",
                receiverId: Guid.NewGuid()
            );

            // Act
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.Messages.FindAsync(message.Id);
            Assert.NotNull(result);
            Assert.Equal("Hello, World!", result.Content);
        }
    }
}
