namespace Infrastructure;

public class ClubMemberConfiguration : BaseEntityConfiguration<ClubMember, Guid>
{
    public override void Configure(EntityTypeBuilder<ClubMember> builder)
    {
        base.Configure(builder);
        builder.ToTable("ClubMembers");

        builder.Property(cm => cm.Role)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(cm => cm.Status)
            .HasConversion<int>()
            .IsRequired();

        // Mối quan hệ: Club (1) -> Members (N)
        // Khi xóa Club -> Xóa luôn Member (Cascade)
        builder.HasOne(cm => cm.Club)
            .WithMany(c => c.Members) 
            .HasForeignKey(cm => cm.ClubId)
            .OnDelete(DeleteBehavior.Cascade);

        // Mối quan hệ: User (1) -> Members (N)
        // Khi xóa User -> Xóa luôn record Membership (Cascade)
        builder.HasOne(cm => cm.User)
            .WithMany() 
            .HasForeignKey(cm => cm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // --- Indexes ---
        builder.HasIndex(cm => new { cm.ClubId, cm.UserId }).IsUnique();
    }
}