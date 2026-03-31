namespace Domain;

public class Major : EntityBase<Guid>
{
    public string Name { get; set; } = null!;
    public string? Code { get; set; }

    // Navigation
    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}
