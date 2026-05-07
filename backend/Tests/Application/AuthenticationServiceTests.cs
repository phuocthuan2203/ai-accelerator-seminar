using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using ToolLendingPlatform.Domain;
using ToolLendingPlatform.Domain.Exceptions;
using ToolLendingPlatform.Application.Interfaces;
using ToolLendingPlatform.Application.Services;

namespace ToolLendingPlatform.Tests.Application
{
    public class AuthenticationServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IPasswordHasher> _mockPasswordHasher;
        private readonly AuthenticationService _authService;

        public AuthenticationServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockPasswordHasher = new Mock<IPasswordHasher>();
            _authService = new AuthenticationService(_mockUserRepository.Object, _mockPasswordHasher.Object);
        }

        // Rule #8: Register happy path
        [Fact]
        public async Task RegisterAsync_ValidInput_CreatesAndSavesUser()
        {
            // Arrange
            var username = "newuser";
            var password = "SecurePass123";
            var passwordHash = "$2a$10$...";

            _mockUserRepository.Setup(r => r.ExistsAsync(username)).ReturnsAsync(false);
            _mockPasswordHasher.Setup(p => p.HashAsync(password)).ReturnsAsync(passwordHash);
            _mockUserRepository.Setup(r => r.SaveAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);

            // Act
            var result = await _authService.RegisterAsync(username, password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(username, result.Username);
            Assert.Equal(passwordHash, result.PasswordHash);
            _mockUserRepository.Verify(r => r.SaveAsync(It.IsAny<User>()), Times.Once);
        }

        // Rule #4: Username validation - too short
        [Fact]
        public async Task RegisterAsync_UsernameTooShort_ThrowsInvalidUsernameException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<InvalidUsernameException>(
                () => _authService.RegisterAsync("ab", "SecurePass123"));
        }

        // Rule #4: Username validation - invalid characters
        [Fact]
        public async Task RegisterAsync_UsernameInvalidCharacters_ThrowsInvalidUsernameException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<InvalidUsernameException>(
                () => _authService.RegisterAsync("user@example", "SecurePass123"));
        }

        // Rule #3: Password validation - too short
        [Fact]
        public async Task RegisterAsync_PasswordTooShort_ThrowsInvalidPasswordException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<InvalidPasswordException>(
                () => _authService.RegisterAsync("validuser", "Pass1"));
        }

        // Rule #3: Password validation - no uppercase
        [Fact]
        public async Task RegisterAsync_PasswordNoUppercase_ThrowsInvalidPasswordException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<InvalidPasswordException>(
                () => _authService.RegisterAsync("validuser", "securepass123"));
        }

        // Rule #5: Duplicate username
        [Fact]
        public async Task RegisterAsync_UsernameDuplicate_ThrowsDuplicateUsernameException()
        {
            // Arrange
            _mockUserRepository.Setup(r => r.ExistsAsync("existing")).ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<DuplicateUsernameException>(
                () => _authService.RegisterAsync("existing", "SecurePass123"));
        }

        // Rule #9: Login happy path
        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsUser()
        {
            // Arrange
            var username = "testuser";
            var password = "SecurePass123";
            var passwordHash = "$2a$10$...";
            var user = new User(username, passwordHash);

            _mockUserRepository.Setup(r => r.GetByUsernameAsync(username)).ReturnsAsync(user);
            _mockPasswordHasher.Setup(p => p.VerifyAsync(password, passwordHash)).ReturnsAsync(true);

            // Act
            var result = await _authService.LoginAsync(username, password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(username, result.Username);
        }

        // Rule #6, #7: Login - user not found (generic message)
        [Fact]
        public async Task LoginAsync_UserNotFound_ThrowsAuthenticationException()
        {
            // Arrange
            _mockUserRepository.Setup(r => r.GetByUsernameAsync("nonexistent")).ReturnsAsync((User)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<AuthenticationException>(
                () => _authService.LoginAsync("nonexistent", "SomePass123"));
            Assert.Equal("Invalid username or password", ex.Message); // Rule #7: Generic message
        }

        // Rule #6, #7: Login - wrong password (generic message)
        [Fact]
        public async Task LoginAsync_WrongPassword_ThrowsAuthenticationException()
        {
            // Arrange
            var username = "testuser";
            var passwordHash = "$2a$10$...";
            var user = new User(username, passwordHash);

            _mockUserRepository.Setup(r => r.GetByUsernameAsync(username)).ReturnsAsync(user);
            _mockPasswordHasher.Setup(p => p.VerifyAsync("WrongPass123", passwordHash)).ReturnsAsync(false);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<AuthenticationException>(
                () => _authService.LoginAsync(username, "WrongPass123"));
            Assert.Equal("Invalid username or password", ex.Message); // Rule #7: Generic message
        }
    }
}
