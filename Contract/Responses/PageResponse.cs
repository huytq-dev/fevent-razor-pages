namespace Contract;

/// <summary>
/// Dùng trong Razor Pages thay cho ApiResponse
/// </summary>
public class PageResponse<T>
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public IDictionary<string, string[]>? Errors { get; set; }

    // ── Factory methods ───────────────────────────────────

    public static PageResponse<T> Ok(T data, string message = "Thành công") =>
        new() { IsSuccess = true, Message = message, Data = data };

    public static PageResponse<T> Ok(string message = "Thành công") =>
        new() { IsSuccess = true, Message = message, Data = default };

    public static PageResponse<T> Fail(string message) =>
        new() { IsSuccess = false, Message = message };

    public static PageResponse<T> Fail(string message, IDictionary<string, string[]> errors) =>
        new() { IsSuccess = false, Message = message, Errors = errors };
}

/// <summary>
/// Dùng khi không cần Data (chỉ cần Success/Fail)
/// Ví dụ: Login, Logout, Delete
/// </summary>
public class PageResponse : PageResponse<object>
{
    public static PageResponse Ok(string message = "Thành công") =>
        new() { IsSuccess = true, Message = message };

    public static PageResponse Fail(string message) =>
        new() { IsSuccess = false, Message = message };

    public static PageResponse Fail(string message, IDictionary<string, string[]> errors) =>
        new() { IsSuccess = false, Message = message, Errors = errors };
}