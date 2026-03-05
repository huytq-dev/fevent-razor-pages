namespace Domain;
public class EventImage : EntityBase<Guid>
{
    public Guid EventId { get; set; }
    public required string ImageUrl { get; set; }
    public string? Caption { get; set; }

    public virtual Event Event { get; set; } = null!;
}