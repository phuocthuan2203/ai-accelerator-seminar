using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Session;
using ToolLendingPlatform.Application.Services;
using ToolLendingPlatform.Api.Dtos;
using ToolLendingPlatform.Api.Filters;

namespace ToolLendingPlatform.Api.Controllers
{
    /// <summary>
    /// Authentication controller for user registration and login.
    /// Rule #1, #2: Endpoints POST /api/auth/register and POST /api/auth/login
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(AuthExceptionFilter))] // Rule #12: Apply exception filter
    public class AuthController : ControllerBase
    {
        private readonly AuthenticationService _authService;

        public AuthController(AuthenticationService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        /// <summary>
        /// Rule #1: Register a new user.
        /// Validates input, creates user, sets session cookie.
        /// Returns: 201 Created with userId, username, createdAt
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequestDto request)
        {
            // Rule #3, #8: Validate request (ASP.NET Core model validation via DataAnnotations)
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new ErrorResponseDto
                {
                    Error = "ValidationError",
                    Details = errors
                });
            }

            // Delegate to application service (Rule #1)
            // Service will validate username/password and throw exceptions if invalid
            var user = await _authService.RegisterAsync(request.Username, request.Password);

            // Rule #6: Create session cookie (HTTP-only, secure, 30-min timeout)
            // ASP.NET Core session is configured in Program.cs; we store userId
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);

            // Rule #9: Return 201 Created with user details
            return Created($"/api/auth/{user.Id}", new RegisterResponseDto
            {
                UserId = user.Id,
                Username = user.Username,
                CreatedAt = user.CreatedAt,
                Message = "Registration successful. You are now logged in."
            });
        }

        /// <summary>
        /// Rule #2: Authenticate user and create session.
        /// Returns: 200 OK with userId, username
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequestDto request)
        {
            // Rule #8: Validate request
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new ErrorResponseDto
                {
                    Error = "ValidationError",
                    Details = errors
                });
            }

            // Delegate to application service (Rule #2)
            // Service will throw AuthenticationException if credentials are invalid
            var user = await _authService.LoginAsync(request.Username, request.Password);

            // Rule #7: Create session cookie
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);

            // Rule #10: Return 200 OK with user details (no password)
            return Ok(new LoginResponseDto
            {
                UserId = user.Id,
                Username = user.Username,
                Message = "Login successful"
            });
        }
    }
}
