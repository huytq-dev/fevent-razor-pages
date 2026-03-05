namespace Infrastructure;

public class SocialLinkConfiguration : BaseEntityConfiguration<SocialLink, Guid>
{
    public override void Configure(EntityTypeBuilder<SocialLink> builder)
    {
        base.Configure(builder);

        builder.ToTable("SocialLinks");

        builder.Property(x => x.Platform)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Url)
            .HasMaxLength(500)
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithMany(u => u.SocialLinks)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.Platform });
    }
}
