namespace Contract;

public static class ValidationMessages
{
    // Basic
    public const string Required = "{PropertyName} là bắt buộc";
    public const string InvalidFormat = "{PropertyName} có định dạng không hợp lệ";
    public const string InvalidEnumValue = "{PropertyName} có giá trị không hợp lệ";

    // Format specific
    public const string InvalidEmail = "{PropertyName} không đúng định dạng email";
    public const string InvalidPhoneNumber = "{PropertyName} không đúng định dạng số điện thoại";
    public const string InvalidDate = "{PropertyName} không phải là ngày hợp lệ";
    public const string InvalidPlateNumberFormat = "{PropertyName} chỉ được chứa chữ cái, số và dấu gạch ngang";
    public const string Expired = "{PropertyName} đã hết hạn";

    // Length & Range
    public const string MinLength = "{PropertyName} phải có ít nhất {MinLength} ký tự";
    public const string MaxLength = "{PropertyName} không được vượt quá {MaxLength} ký tự";
    public const string ExactLength = "{PropertyName} phải có đúng {ExactLength} ký tự";
    public const string Range = "{PropertyName} phải nằm trong khoảng {MinLength} đến {MaxLength} ký tự";

    // Comparison
    public const string GreaterThan = "{PropertyName} phải lớn hơn {ComparisonValue}";
    public const string GreaterThanOrEqual = "{PropertyName} phải lớn hơn hoặc bằng {ComparisonValue}";
    public const string LessThan = "{PropertyName} phải nhỏ hơn {ComparisonValue}";
    public const string LessThanOrEqual = "{PropertyName} phải nhỏ hơn hoặc bằng {ComparisonValue}";

    // Content restriction
    public const string OnlyLetters = "{PropertyName} chỉ được chứa chữ cái";
    public const string OnlyNumbers = "{PropertyName} chỉ được chứa số";
    public const string OnlyAlphanumeric = "{PropertyName} chỉ được chứa chữ cái và số";
    public const string NotContainSpaces = "{PropertyName} không được chứa khoảng trắng";
    public const string ProhibitedContent = "Bình luận của bạn chứa nội dung bị cấm";

    // List/Collection
    public const string ListNotEmpty = "{PropertyName} không được để trống";
    public const string ListMinItems = "{PropertyName} phải chứa ít nhất {MinItems} phần tử";
    public const string ListMaxItems = "{PropertyName} không được chứa quá {MaxItems} phần tử";

    // Boolean
    public const string MustBeTrue = "{PropertyName} phải là true";
    public const string MustBeFalse = "{PropertyName} phải là false";

    // Matching
    public const string MustMatch = "{PropertyName} phải trùng với {ComparisonProperty}";
    public const string MustNotMatch = "{PropertyName} không được trùng với {ComparisonProperty}";
}