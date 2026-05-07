using System;

namespace ToolLendingPlatform.Api.Dtos
{
    /// Rule #9: Response payload for successful registration (201 Created)
    public class RegisterResponseDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string Message { get; set; } = null!;
    }
}
