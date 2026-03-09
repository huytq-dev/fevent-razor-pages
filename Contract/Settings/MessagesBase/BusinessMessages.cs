namespace Contract;

public static class BusinessMessages
{
    // Success Messages
    public static string CreatedSuccessfully(string entityName) => $"Tạo {entityName} thành công";
    public static string UpdatedSuccessfully(string entityName) => $"Cập nhật {entityName} thành công";
    public static string DeletedSuccessfully(string entityName) => $"Xóa {entityName} thành công";
    public static string FoundSuccessfully(string entityName) => $"Tìm thấy {entityName}";

    // Failure Messages
    public static string CreateFailure(string entityName) => $"Tạo {entityName} thất bại";
    public static string UpdateFailure(string entityName) => $"Cập nhật {entityName} thất bại";
    public static string DeleteFailure(string entityName) => $"Xóa {entityName} thất bại";
    public static string GetFailure(string entityName) => $"Không thể lấy {entityName}";
}