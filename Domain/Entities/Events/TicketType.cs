namespace Domain;

public class TicketType : EntityBase<Guid>
{
    public Guid EventId { get; set; }

    public required string Name { get; set; } // VD: VIP, Early Bird
    public decimal Price { get; set; } = 0;
    public int Quantity { get; set; } // Tổng số vé loại này
    public int SoldCount { get; set; } = 0; // Đã bán

    public DateTimeOffset? SaleStartDate { get; set; }
    public DateTimeOffset? SaleEndDate { get; set; }

    public virtual Event Event { get; set; } = null!;
    public virtual ICollection<EventRegistration> Registrations { get; set; } = new List<EventRegistration>();
}