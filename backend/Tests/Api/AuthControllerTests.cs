using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using ToolLendingPlatform.Api.Dtos;

namespace ToolLendingPlatform.Tests.Api
{
    public class AuthControllerTests : IAsyncLifetime
    {
        private WebApplicationFactory<Program> _factory = null!;
        private HttpClient _client = null!;

        public async Task InitializeAsync()
        {
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        config.AddInMemoryCollection(new Dictionary<string, string?>
                        {
                            { "ConnectionStrings:DefaultConnection", "Data Source=:memory:" }
                        });
                    });
                });
            _client = _factory.CreateClient();
            await Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            _client.Dispose();
            await _factory.DisposeAsync();
        }

        // Rule #1, #9: Register happy path
        [Fact]
        public async Task RegisterAsync_ValidInput_Returns201Created()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Username = $"newuser{Guid.NewGuid().ToString().Replace("-", "").Substring(0, 12)}",
                Password = "SecurePass123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", request);

            // Debug: Get response body to understand error
            if (response.StatusCode != HttpStatusCode.Created)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                Assert.Fail($"Expected 201 Created but got {response.StatusCode}. Response: {errorBody}");
            }

            var content = await response.Content.ReadFromJsonAsync<RegisterResponseDto>();

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(request.Username, content.Username);
            Assert.NotEqual(0, content.UserId);
            Assert.NotEqual(default(DateTime), content.CreatedAt);
            Assert.NotNull(content.Message);
        }

        // Rule #4: Register duplicate username
        [Fact]
        public async Task RegisterAsync_DuplicateUsername_Returns400()
        {
            // Arrange
            var uniqueUsername = $"duplicate{Guid.NewGuid()}";
            var request = new RegisterRequestDto
            {
                Username = uniqueUsername,
                Password = "SecurePass123"
            };

            // First registration succeeds
            await _client.PostAsJsonAsync("/api/auth/register", request);

            // Act: Second registration with same username (using same request with same username)
            var response = await _client.PostAsJsonAsync("/api/auth/register", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Rule #3: Register invalid password
        [Fact]
        public async Task RegisterAsync_WeakPassword_Returns400()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Username = $"validuser{Guid.NewGuid()}",
                Password = "weak" // Too short, no uppercase, no digit
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Rule #8: Register missing field
        [Fact]
        public async Task RegisterAsync_MissingUsername_Returns400()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Username = null!,
                Password = "SecurePass123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Rule #2, #10: Login happy path
        [Fact]
        public async Task LoginAsync_ValidCredentials_Returns200Ok()
        {
            // Arrange: First register
            var uniqueUsername = $"testuser{Guid.NewGuid()}";
            var registerRequest = new RegisterRequestDto
            {
                Username = uniqueUsername,
                Password = "SecurePass123"
            };
            await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Act: Login
            var loginRequest = new LoginRequestDto
            {
                Username = uniqueUsername,
                Password = "SecurePass123"
            };
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Debug
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                Assert.Fail($"Expected 200 OK but got {response.StatusCode}. Response: {errorBody}");
            }

            var content = await response.Content.ReadFromJsonAsync<LoginResponseDto>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(uniqueUsername, content.Username);
            Assert.NotEqual(0, content.UserId);
            Assert.NotNull(content.Message);
        }

        // Rule #5: Login wrong password
        [Fact]
        public async Task LoginAsync_WrongPassword_Returns401()
        {
            // Arrange: Register first
            var uniqueUsername = $"testuser{Guid.NewGuid()}";
            var registerRequest = new RegisterRequestDto
            {
                Username = uniqueUsername,
                Password = "SecurePass123"
            };
            await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Act: Login with wrong password
            var loginRequest = new LoginRequestDto
            {
                Username = uniqueUsername,
                Password = "WrongPass123"
            };
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // Rule #5: Login user not found
        [Fact]
        public async Task LoginAsync_UserNotFound_Returns401()
        {
            // Arrange
            var loginRequest = new LoginRequestDto
            {
                Username = $"nonexistent{Guid.NewGuid().ToString().Replace("-", "").Substring(0, 12)}",
                Password = "SomePass123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // Rule #6: Register sets cookie
        [Fact]
        public async Task RegisterAsync_SetsCookie()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Username = $"cookietest{Guid.NewGuid().ToString().Replace("-", "").Substring(0, 12)}",
                Password = "SecurePass123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", request);

            // Assert
            Assert.True(response.Headers.Contains("Set-Cookie"), "Set-Cookie header should be present");
        }

        // Rule #7: Login sets cookie
        [Fact]
        public async Task LoginAsync_SetsCookie()
        {
            // Arrange: First register
            var uniqueUsername = $"logincookie{Guid.NewGuid().ToString().Replace("-", "").Substring(0, 12)}";
            var registerRequest = new RegisterRequestDto
            {
                Username = uniqueUsername,
                Password = "SecurePass123"
            };
            await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Reset client to clear previous cookies for fresh login test
            var loginClient = _factory.CreateClient();

            // Act: Login
            var loginRequest = new LoginRequestDto
            {
                Username = uniqueUsername,
                Password = "SecurePass123"
            };
            var response = await loginClient.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            Assert.True(response.Headers.Contains("Set-Cookie"), "Set-Cookie header should be present");
            loginClient.Dispose();
        }
    }
}
