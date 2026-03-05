namespace Domain;

public enum RegistrationStatus
{
    PendingPayment = 0, // Chờ thanh toán
    Paid = 1,           // Đã thanh toán (Vé hợp lệ)
    CheckedIn = 2,      // Đã tham gia (Quét QR xong)
    Cancelled = 3,      // Hủy vé/Hoàn tiền
    Confirmed = 4 
}