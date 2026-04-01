namespace UI;

public static class ChatbotEndpoints
{
    public static IEndpointRouteBuilder MapChatbotEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/chatbot/message", async (
            IAiChatbotService chatbotService,
            ChatbotRequest request,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return Results.BadRequest(new { error = "Message is required." });
            }

            var result = await chatbotService.GetReplyAsync(request.Message, ct);
            return Results.Ok(new { reply = result.Reply, errorDetail = result.ErrorDetail });
        });

        return app;
    }
}

public sealed record ChatbotRequest(string Message);
