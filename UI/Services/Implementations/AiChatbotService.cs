using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace UI;

[RegisterService(typeof(IAiChatbotService))]
public sealed class AiChatbotService : IAiChatbotService
{
    private readonly AiChatbotOptions _options;
    private readonly string _faqPath;
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;
    private readonly ILogger<AiChatbotService> _logger;
    private static readonly string[] GeminiModelFallbacks =
    [
        "gemini-1.5-flash",
        "gemini-1.5-flash-latest",
        "gemini-2.0-flash",
        "gemini-2.0-flash-lite",
        "gemini-pro"
    ];

    public AiChatbotService(
        HttpClient httpClient,
        IOptions<AiChatbotOptions> options,
        IWebHostEnvironment environment,
        IConfiguration configuration,
        ILogger<AiChatbotService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
        _faqPath = Path.Combine(environment.ContentRootPath, "ChatbotKnowledge", "faq_vi.md");
        _apiKey =
            configuration["AiChatbot:ApiKey"]
            ?? Environment.GetEnvironmentVariable("GROQ_API_KEY")
            ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY")
            ?? Environment.GetEnvironmentVariable("GOOGLE_API_KEY")
            ?? options.Value.ApiKey;

        _logger.LogInformation("[FEvents AI] API key loaded: {MaskedKey}", MaskSecret(_apiKey));
    }


    private static string? _faqContent;
    private static readonly object FaqLock = new();

    private static readonly string[] FaqKeywords =
    [
        "ve", "hoan ve", "chinh sach", "check-in", "checkin", "faq", "dang ky", "ticket", "qr"
    ];

    private static readonly string[] GreetingKeywords =
    [
        "alo", "xin chao", "chao", "hello", "hi", "hey"
    ];

    public async Task<AiChatbotReply> GetReplyAsync(string userMessage, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
        {
            return new AiChatbotReply { Reply = "Bạn hãy nhập nội dung cần hỏi." };
        }

        var localReply = TryAnswerFromLocalIntents(userMessage);
        if (!string.IsNullOrWhiteSpace(localReply))
        {
            return new AiChatbotReply { Reply = localReply };
        }

        var faqReply = TryAnswerFromFaq(userMessage);
        if (!string.IsNullOrWhiteSpace(faqReply))
        {
            return new AiChatbotReply { Reply = faqReply };
        }

        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            _logger.LogError("[FEvents AI] API key is missing or empty at runtime!");
            return new AiChatbotReply { Reply = "Chatbot chưa được cấu hình API key. Vui lòng cập nhật mục AiChatbot:ApiKey hoặc biến môi trường GROQ_API_KEY." };
        }

        var isGemini = _options.Endpoint.Contains("generativelanguage.googleapis.com", StringComparison.OrdinalIgnoreCase);
        if (isGemini)
        {
            var (reply, errorDetail) = await CallGeminiWithErrorAsync(userMessage, ct);
            if (!string.IsNullOrWhiteSpace(reply))
            {
                return new AiChatbotReply { Reply = reply };
            }
            // fallback to FAQ if Gemini fails
            var fallback = TryAnswerFromFaq(userMessage);
            if (!string.IsNullOrWhiteSpace(fallback))
            {
                return new AiChatbotReply { Reply = fallback, ErrorDetail = errorDetail };
            }
            return new AiChatbotReply { Reply = "Chatbot Gemini đang bận hoặc chưa kết nối được. Vui lòng thử lại sau.", ErrorDetail = errorDetail };
        }
        else
        {
            var aiReply = await CallOpenAiCompatibleAsync(userMessage, ct);
            if (!string.IsNullOrWhiteSpace(aiReply))
            {
                return new AiChatbotReply { Reply = aiReply };
            }
            var fallback = TryAnswerFromFaq(userMessage);
            if (!string.IsNullOrWhiteSpace(fallback))
            {
                return new AiChatbotReply { Reply = fallback };
            }
            return new AiChatbotReply { Reply = "Chatbot AI đang bận hoặc chưa kết nối được. Vui lòng thử lại sau." };
        }
    }

    private async Task<(string? reply, string? errorDetail)> CallGeminiWithErrorAsync(string userMessage, CancellationToken ct)
    {
        var prompt = string.IsNullOrWhiteSpace(_options.SystemPrompt)
            ? userMessage
            : $"{_options.SystemPrompt}\n\nNgười dùng: {userMessage}";

        var payload = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = prompt } }
                }
            },
            generationConfig = new
            {
                temperature = _options.Temperature,
                maxOutputTokens = _options.MaxTokens
            }
        };

        var payloadJson = JsonSerializer.Serialize(payload);
        var candidateEndpoints = BuildGeminiCandidateEndpoints(_options.Endpoint, _options.Model);

        string? lastError = null;
        foreach (var endpoint in candidateEndpoints)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(payloadJson, Encoding.UTF8, "application/json")
            };
            request.Headers.TryAddWithoutValidation("x-goog-api-key", _apiKey);

            using var response = await _httpClient.SendAsync(request, ct);
            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                lastError = $"Gemini API error: {response.StatusCode} - {body}";
                _logger.LogWarning("Gemini request failed at endpoint {Endpoint}: StatusCode={StatusCode}", endpoint, response.StatusCode);

                var isNotFound = (int)response.StatusCode == 404
                    || body.Contains("NOT_FOUND", StringComparison.OrdinalIgnoreCase)
                    || body.Contains("not found", StringComparison.OrdinalIgnoreCase)
                    || body.Contains("not supported for generateContent", StringComparison.OrdinalIgnoreCase);

                if (isNotFound)
                {
                    continue;
                }

                return (null, lastError);
            }

            var parsedReply = TryParseGeminiText(body, out var parseError);
            if (!string.IsNullOrWhiteSpace(parsedReply))
            {
                return (parsedReply, null);
            }

            lastError = parseError;
        }

        return (null, lastError ?? "Gemini request failed on all known model endpoints.");
    }

    private string? TryParseGeminiText(string json, out string? error)
    {
        error = null;

        using var doc = JsonDocument.Parse(json);

        try
        {
            if (!doc.RootElement.TryGetProperty("candidates", out var candidates))
            {
                _logger.LogWarning("Gemini response has no candidates. Body={Body}", json);
                error = "Gemini response has no candidates";
                return null;
            }

            if (candidates.GetArrayLength() == 0)
            {
                error = "Gemini response candidates array is empty";
                return null;
            }

            var firstCandidate = candidates[0];
            if (!firstCandidate.TryGetProperty("content", out var content)
                || !content.TryGetProperty("parts", out var parts)
                || parts.GetArrayLength() == 0)
            {
                _logger.LogWarning("Gemini response candidate missing content/parts. Body={Body}", json);
                error = "Gemini response candidate missing content/parts";
                return null;
            }

            foreach (var part in parts.EnumerateArray())
            {
                if (part.TryGetProperty("text", out var textElement))
                {
                    var text = textElement.GetString()?.Trim();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        return text;
                    }
                }
            }

            error = "Gemini response parts missing text";
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Gemini response parse failed.");
            error = $"Gemini response parse failed: {ex.Message}";
            return null;
        }
    }

    private static IEnumerable<string> BuildGeminiCandidateEndpoints(string configuredEndpoint, string? configuredModel)
    {
        var endpoints = new List<string>();

        if (!string.IsNullOrWhiteSpace(configuredEndpoint))
        {
            endpoints.Add(configuredEndpoint);
        }

        var apiVersion = !string.IsNullOrWhiteSpace(configuredEndpoint)
            && configuredEndpoint.Contains("/v1/", StringComparison.OrdinalIgnoreCase)
            ? "v1"
            : "v1beta";
        var models = new List<string>();

        if (!string.IsNullOrWhiteSpace(configuredModel))
        {
            models.Add(configuredModel);
        }

        foreach (var fallback in GeminiModelFallbacks)
        {
            if (!models.Contains(fallback, StringComparer.OrdinalIgnoreCase))
            {
                models.Add(fallback);
            }
        }

        foreach (var model in models)
        {
            endpoints.Add($"https://generativelanguage.googleapis.com/{apiVersion}/models/{model}:generateContent");
        }

        var oppositeVersion = apiVersion == "v1" ? "v1beta" : "v1";
        foreach (var model in models)
        {
            endpoints.Add($"https://generativelanguage.googleapis.com/{oppositeVersion}/models/{model}:generateContent");
        }

        return endpoints
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private static string MaskSecret(string? secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
        {
            return "(empty)";
        }

        if (secret.Length <= 8)
        {
            return "***";
        }

        return $"{secret[..4]}...{secret[^4..]}";
    }

    private static string? TryAnswerFromLocalIntents(string userMessage)
    {
        var lower = userMessage.Trim().ToLowerInvariant();

        if (GreetingKeywords.Any(k => lower.Contains(k)))
        {
            return "Xin chào bạn. Mình có thể hỗ trợ đăng ký sự kiện, check-in, vé, hoàn vé và các vấn đề tài khoản trên FEvents.";
        }

        if (lower.Contains("dang ky") || lower.Contains("đăng ký"))
        {
            return "Để đăng ký sự kiện: mở trang chi tiết sự kiện, bấm Đăng ký, sau đó kiểm tra vé/QR trong hồ sơ của bạn.";
        }

        if (lower.Contains("check-in") || lower.Contains("checkin") || lower.Contains("check in"))
        {
            return "Bạn có thể check-in bằng mã QR tại sự kiện. Nếu QR lỗi, vui lòng mở lại trang vé và làm mới mã trước khi quét.";
        }

        if (lower.Contains("hoan ve") || lower.Contains("hoàn vé") || lower.Contains("refund"))
        {
            return "Bạn vui lòng vào mục vé đã mua, chọn sự kiện cần hoàn và gửi yêu cầu hoàn vé theo chính sách của sự kiện.";
        }

        return null;
    }

    private string? TryAnswerFromFaq(string userMessage)
    {
        var faq = GetFaqContent();
        if (string.IsNullOrWhiteSpace(faq))
        {
            return null;
        }

        var lowerMsg = userMessage.ToLowerInvariant();
        if (!FaqKeywords.Any(k => lowerMsg.Contains(k)))
        {
            return null;
        }

        var lines = faq.Split('\n');
        var matches = lines
            .Where(l => FaqKeywords.Any(k => l.Contains(k, StringComparison.OrdinalIgnoreCase)))
            .Take(8)
            .ToList();

        if (matches.Count == 0)
        {
            return faq.Length > 600 ? faq[..600] + "..." : faq;
        }

        return string.Join("\n", matches).Trim();
    }

    private string? GetFaqContent()
    {
        if (_faqContent != null)
        {
            return _faqContent;
        }

        lock (FaqLock)
        {
            if (_faqContent != null)
            {
                return _faqContent;
            }

            if (File.Exists(_faqPath))
            {
                _faqContent = File.ReadAllText(_faqPath);
            }

            return _faqContent;
        }
    }

    private async Task<string?> CallGeminiAsync(string userMessage, CancellationToken ct)
    {
        var prompt = string.IsNullOrWhiteSpace(_options.SystemPrompt)
            ? userMessage
            : $"{_options.SystemPrompt}\n\nNgười dùng: {userMessage}";

        var payload = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = prompt } }
                }
            },
            generationConfig = new
            {
                temperature = _options.Temperature,
                maxOutputTokens = _options.MaxTokens
            }
        };

        var endpointWithKey = BuildGeminiEndpointWithKey(_options.Endpoint, _apiKey);
        using var request = new HttpRequestMessage(HttpMethod.Post, endpointWithKey)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };
        request.Headers.TryAddWithoutValidation("x-goog-api-key", _apiKey);

        using var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            _logger.LogWarning("Gemini request failed: StatusCode={StatusCode}, Body={Body}", response.StatusCode, errorBody);
            return null;
        }

        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);

        try
        {
            if (!doc.RootElement.TryGetProperty("candidates", out var candidates))
            {
                _logger.LogWarning("Gemini response has no candidates. Body={Body}", json);
                return null;
            }

            if (candidates.GetArrayLength() == 0)
            {
                return null;
            }

            var firstCandidate = candidates[0];
            if (!firstCandidate.TryGetProperty("content", out var content)
                || !content.TryGetProperty("parts", out var parts)
                || parts.GetArrayLength() == 0)
            {
                _logger.LogWarning("Gemini response candidate missing content/parts. Body={Body}", json);
                return null;
            }

            foreach (var part in parts.EnumerateArray())
            {
                if (part.TryGetProperty("text", out var textElement))
                {
                    var text = textElement.GetString()?.Trim();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        return text;
                    }
                }
            }

            return null;
        }
        catch
        {
            _logger.LogWarning("Gemini response parse failed.");
            return null;
        }
    }

    private static string BuildGeminiEndpointWithKey(string endpoint, string apiKey)
    {
        if (endpoint.Contains("key=", StringComparison.OrdinalIgnoreCase))
        {
            return endpoint;
        }

        return endpoint.Contains('?')
            ? endpoint + "&key=" + apiKey
            : endpoint + "?key=" + apiKey;
    }

    private async Task<string?> CallOpenAiCompatibleAsync(string userMessage, CancellationToken ct)
    {
        var payload = new
        {
            model = string.IsNullOrWhiteSpace(_options.Model) ? "gpt-4.1-mini" : _options.Model,
            messages = new object[]
            {
                new { role = "system", content = _options.SystemPrompt },
                new { role = "user", content = userMessage }
            },
            max_tokens = _options.MaxTokens,
            temperature = _options.Temperature
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.Endpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        using var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("OpenAI-compatible request failed: StatusCode={StatusCode}", response.StatusCode);
            return null;
        }

        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);

        try
        {
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString()
                ?.Trim();
        }
        catch
        {
            _logger.LogWarning("OpenAI-compatible response parse failed.");
            return null;
        }
    }
}
