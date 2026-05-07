namespace ToolLendingPlatform.Api.Dtos
{
    public class ErrorResponseDto
    {
        public string Error { get; set; } = null!;
        public object? Details { get; set; }
    }
}
