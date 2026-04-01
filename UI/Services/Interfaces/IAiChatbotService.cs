namespace UI;

public interface IAiChatbotService
{
    Task<AiChatbotReply> GetReplyAsync(string userMessage, CancellationToken ct);
}

public sealed class AiChatbotReply
{
    public string? Reply { get; set; }
    public string? ErrorDetail { get; set; }
}
