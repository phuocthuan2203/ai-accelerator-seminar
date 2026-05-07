namespace ToolLendingPlatform.Api.Dtos
{
    /// Rule #10: Response payload for successful login (200 OK)
    public class LoginResponseDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string Message { get; set; } = null!;
    }
}
