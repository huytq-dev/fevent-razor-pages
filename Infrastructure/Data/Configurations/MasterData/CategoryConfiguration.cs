namespace Infrastructure;

public class CategoryConfiguration : BaseEntityConfiguration<Category, Guid>
{
    public override void Configure(EntityTypeBuilder<Category> builder)
    {
        base.Configure(builder);

        builder.ToTable("Categories");

        // --- Properties ---
        builder.Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasMaxLength(1000);

        // --- Indexes ---
        builder.HasIndex(c => c.Name).IsUnique();
    }
}
