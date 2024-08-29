namespace api_ai_sport_assistant.Models
{
    public class ChatProviderRequest
    {
        public string? userId { get; set; } = string.Empty;
        public string? sessionId { get; set; } = string.Empty;

        public string? sportCode { get; set; } = string.Empty;
        public string? tenantId { get; set; } = string.Empty;
        public string? type { get; set; } = string.Empty;
        public string? prompt { get; set; } = string.Empty;
        public string? chatName { get; set; } = string.Empty;

    }
}
