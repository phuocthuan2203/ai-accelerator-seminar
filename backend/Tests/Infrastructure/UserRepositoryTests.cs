using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using ToolLendingPlatform.Domain;
using ToolLendingPlatform.Infrastructure.Data;
using ToolLendingPlatform.Infrastructure.Repositories;

namespace ToolLendingPlatform.Tests.Infrastructure
{
    public class UserRepositoryTests
    {
        private ToolLendingDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ToolLendingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ToolLendingDbContext(options);
        }

        [Fact]
        public async Task GetByUsernameAsync_UserExists_ReturnsUser()
        {
            // Arrange
            using var dbContext = CreateInMemoryDbContext();
            var user = new User("john_doe", "hashed_password_123");
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            var repository = new UserRepository(dbContext);

            // Act
            var result = await repository.GetByUsernameAsync("john_doe");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("john_doe", result.Username);
            Assert.Equal("hashed_password_123", result.PasswordHash);
        }

        [Fact]
        public async Task GetByUsernameAsync_UserDoesNotExist_ReturnsNull()
        {
            // Arrange
            using var dbContext = CreateInMemoryDbContext();
            var repository = new UserRepository(dbContext);

            // Act
            var result = await repository.GetByUsernameAsync("nonexistent");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByUsernameAsync_NullOrEmptyUsername_ReturnsNull()
        {
            // Arrange
            using var dbContext = CreateInMemoryDbContext();
            var repository = new UserRepository(dbContext);

            // Act
            var resultNull = await repository.GetByUsernameAsync(null!);
            var resultEmpty = await repository.GetByUsernameAsync("");
            var resultWhitespace = await repository.GetByUsernameAsync("   ");

            // Assert
            Assert.Null(resultNull);
            Assert.Null(resultEmpty);
            Assert.Null(resultWhitespace);
        }

        [Fact]
        public async Task SaveAsync_ValidUser_PersistsAndReturnsUserWithId()
        {
            // Arrange
            using var dbContext = CreateInMemoryDbContext();
            var user = new User("jane_doe", "hashed_password_456");
            var repository = new UserRepository(dbContext);

            // Act
            var savedUser = await repository.SaveAsync(user);

            // Assert
            Assert.NotEqual(0, savedUser.Id);
            Assert.Equal("jane_doe", savedUser.Username);

            // Verify in database
            var retrievedUser = await dbContext.Users.FirstAsync(u => u.Username == "jane_doe");
            Assert.Equal(savedUser.Id, retrievedUser.Id);
        }

        [Fact]
        public async Task SaveAsync_NullUser_ThrowsArgumentNullException()
        {
            // Arrange
            using var dbContext = CreateInMemoryDbContext();
            var repository = new UserRepository(dbContext);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await repository.SaveAsync(null!));
        }

        [Fact]
        public async Task SaveAsync_ValidUser_SetsTimestamps()
        {
            // Arrange
            using var dbContext = CreateInMemoryDbContext();
            var beforeSave = DateTime.UtcNow;
            var user = new User("timestamp_test", "hash");
            var repository = new UserRepository(dbContext);

            // Act
            await repository.SaveAsync(user);
            var afterSave = DateTime.UtcNow;

            // Assert
            Assert.True(user.CreatedAt >= beforeSave && user.CreatedAt <= afterSave);
            Assert.True(user.UpdatedAt >= beforeSave && user.UpdatedAt <= afterSave);
        }

        [Fact]
        public async Task ExistsAsync_UsernameExists_ReturnsTrue()
        {
            // Arrange
            using var dbContext = CreateInMemoryDbContext();
            var user = new User("alice", "hashed_password_789");
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            var repository = new UserRepository(dbContext);

            // Act
            var result = await repository.ExistsAsync("alice");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_UsernameDoesNotExist_ReturnsFalse()
        {
            // Arrange
            using var dbContext = CreateInMemoryDbContext();
            var repository = new UserRepository(dbContext);

            // Act
            var result = await repository.ExistsAsync("nonexistent");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_NullOrEmptyUsername_ReturnsFalse()
        {
            // Arrange
            using var dbContext = CreateInMemoryDbContext();
            var repository = new UserRepository(dbContext);

            // Act
            var resultNull = await repository.ExistsAsync(null!);
            var resultEmpty = await repository.ExistsAsync("");
            var resultWhitespace = await repository.ExistsAsync("   ");

            // Assert
            Assert.False(resultNull);
            Assert.False(resultEmpty);
            Assert.False(resultWhitespace);
        }

        [Fact]
        public async Task SaveAsync_DuplicateUsername_InMemoryDb_AllowsDuplicate()
        {
            // Note: EF Core's in-memory database does not enforce unique constraints by default.
            // This test verifies that the repository code compiles and runs.
            // Actual unique constraint enforcement is tested in SQLite integration tests or verified via database schema.
            // Arrange
            using var dbContext = CreateInMemoryDbContext();
            var user1 = new User("duplicate", "hash1");
            var user2 = new User("duplicate", "hash2");

            dbContext.Users.Add(user1);
            await dbContext.SaveChangesAsync();

            var repository = new UserRepository(dbContext);

            // Act - In-memory database allows duplicates (constraint not enforced)
            var savedUser = await repository.SaveAsync(user2);

            // Assert - Both users exist (constraint not enforced in memory)
            var count = await dbContext.Users.CountAsync(u => u.Username == "duplicate");
            Assert.Equal(2, count);
        }
    }
}
