namespace Domain;

public class EventRegistration : EntityBase<Guid>
{
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public Guid TicketTypeId { get; set; }

    // QR Check-in Info
    public required string TicketCode { get; set; } // String unique để tạo QR
    public string? QrCodeUrl { get; set; } // Link ảnh QR (nếu cần)
    public DateTimeOffset? CheckInTime { get; set; }

    // Đếm ngược thời gian thanh toán
    public DateTimeOffset? PaymentExpiryTime { get; set; }

    // Status
    public RegistrationStatus Status { get; set; } = RegistrationStatus.PendingPayment;
    public DateTimeOffset? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }

    // Navigation
    public virtual Event Event { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual TicketType TicketType { get; set; } = null!;
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
