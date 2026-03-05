namespace Domain;

public class Transaction : EntityBase<Guid>
{
    public Guid EventRegistrationId { get; set; }

    public decimal Amount { get; set; }
    public string? TransactionNo { get; set; } // Mã GD từ Bank/VNPay
    public string? BankCode { get; set; }      // VCB, MOMO...
    public string? OrderInfo { get; set; }

    public TransactionStatus Status { get; set; }

    public virtual EventRegistration EventRegistration { get; set; } = null!;
}