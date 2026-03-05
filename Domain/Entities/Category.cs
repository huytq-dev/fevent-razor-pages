namespace Domain;

public class Category : EntityBase<Guid>
{
    public string Name { get; set; } = null!;// VD: Workshop, Seminar, Music
    public string? Description { get; set; }

    // Navigation
    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}