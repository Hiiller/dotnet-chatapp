using ChatApp.Server.Domain.ValueObjects;
using Xunit;
using ChatApp.Server.Infrastructure.Data;
using ChatApp.Server.Domain.Entities;
using ChatApp.Server.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ChatApp.Server.Tests.Repositories
{
    public class UserRepositoryTests
    {
        private readonly AppDbContext _context;

        public UserRepositoryTests()
        {
            _context = TestDbContextFactory.CreateInMemoryDbContext();
        }

        [Fact]
        public async Task AddUser_ValidUser_ShouldAddUserToDatabase()
        {
            // Arrange
            var user = new User("testUser",  "hashedPassword");

            // Act
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.Users.FindAsync(user.Id);
            Assert.NotNull(result);
            Assert.Equal("testUser", result.Username);
        }
    }
}
