namespace Contract;

public static class ExceptionMessages
{
    // Không có tham số
    public const string Forbidden = "Bạn không có quyền truy cập tài nguyên này";
    public const string DatabaseOperationFailed = "Thao tác cơ sở dữ liệu thất bại";
    public const string InternalError = "Đã xảy ra lỗi không mong muốn";

    // --- NotFound ---
    public static string NotFound(string entityName, object id)
        => $"Không tìm thấy {entityName} với ID {id}";

    public static string NotFoundField(string entityName, string fieldName, object value)
        => $"Không tìm thấy {entityName} với {fieldName} '{value}'";

    public static string NotExists(string entityName, object value)
        => $"{entityName} với giá trị '{value}' không tồn tại";

    // --- Conflict / Validation Logic ---
    public static string AlreadyExists(string entityName, object value)
        => $"{entityName} với giá trị '{value}' đã tồn tại";

    public static string Invalid(string entityName)
        => $"{entityName} không hợp lệ";
}