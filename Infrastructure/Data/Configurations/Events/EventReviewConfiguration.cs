namespace Infrastructure;

public class EventReviewConfiguration : BaseEntityConfiguration<EventReview, Guid>
{
    public override void Configure(EntityTypeBuilder<EventReview> builder)
    {
        base.Configure(builder);

        builder.ToTable("EventReviews");

        // --- Properties ---
        builder.Property(e => e.Content)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(e => e.Rating)
            .IsRequired();

        builder.Property(e => e.IsDeleted)
            .HasDefaultValue(false);

        // --- Relationships ---
        // Restrict để tránh multiple cascade paths và cycle từ User, Event và self-reference
        builder.HasOne(e => e.Event)
            .WithMany(e => e.Reviews)
            .HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Self-referencing relationship for replies - Restrict để tránh cycle
        builder.HasOne(e => e.Parent)
            .WithMany(e => e.Replies)
            .HasForeignKey(e => e.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- Indexes ---
        builder.HasIndex(e => e.EventId);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.ParentId);
        builder.HasIndex(e => e.Rating);
        builder.HasIndex(e => e.IsDeleted);
    }
}
