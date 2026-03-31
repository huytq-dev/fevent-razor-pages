namespace Infrastructure;

public class MajorConfiguration : BaseEntityConfiguration<Major, Guid>
{
    public override void Configure(EntityTypeBuilder<Major> builder)
    {
        base.Configure(builder);

        builder.ToTable("Majors");

        builder.Property(m => m.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(m => m.Code)
            .HasMaxLength(20);

        builder.HasIndex(m => m.Code).IsUnique().HasFilter("[Code] IS NOT NULL");
    }
}
