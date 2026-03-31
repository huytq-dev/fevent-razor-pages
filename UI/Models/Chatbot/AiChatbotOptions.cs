namespace UI.Models.Chatbot;

public sealed class AiChatbotOptions
{
    public string Endpoint { get; set; } = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-2.0-flash";
    public string SystemPrompt { get; set; } = "Bạn là trợ lý FEvents. Luôn trả lời ngắn gọn, hữu ích, bằng tiếng Việt có dấu.";
    public int MaxTokens { get; set; } = 300;
    public double Temperature { get; set; } = 0.3;
}
