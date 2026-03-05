namespace Domain;

public class EventCollaborator : EntityBase<Guid>
{
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public EventRole Role { get; set; } // CoHost, Staff...

    public virtual Event Event { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}