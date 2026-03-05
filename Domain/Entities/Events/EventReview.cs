namespace Domain;
public class EventReview : EntityBase<Guid>, ISoftDeletable
{
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public Guid? ParentId { get; set; } // Để reply comment

    public string Content { get; set; } = string.Empty;
    public int Rating { get; set; } // 1-5 sao

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public virtual Event Event { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual EventReview? Parent { get; set; }
    public virtual ICollection<EventReview> Replies { get; set; } = new List<EventReview>();
}