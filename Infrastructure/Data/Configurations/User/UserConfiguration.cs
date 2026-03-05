namespace Infrastructure;

public class UserConfiguration : BaseEntityConfiguration<User, Guid>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);

        builder.ToTable("Users");

        builder.Property(u => u.FullName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(u => u.Username)
            .HasMaxLength(50)
            .IsUnicode(false) 
            .IsRequired();

        builder.Property(u => u.Email)
            .HasMaxLength(255)
            .IsUnicode(false) 
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(u => u.AvatarUrl)
            .HasMaxLength(500)
            .IsUnicode(false); 

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20)
            .IsUnicode(false);

        builder.Property(u => u.Major)
            .HasMaxLength(200);

        builder.Property(u => u.Address)
            .HasMaxLength(255);

        builder.Property(u => u.SchoolName)
            .HasMaxLength(200);

        builder.Property(u => u.StudentId)
            .HasMaxLength(50) 
            .IsUnicode(false);

        // --- 4. Enum & Bool ---
        builder.Property(u => u.Gender)
            .HasConversion<int>(); 

        builder.Property(u => u.IsVerified)
            .HasDefaultValue(false);

        builder.Property(u => u.IsDeleted)
            .HasDefaultValue(false);

        // --- 5. Indexes (Quan trọng cho hiệu năng) ---
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.Username).IsUnique();
        builder.HasIndex(u => u.StudentId);
        builder.HasIndex(u => u.IsDeleted);
    }
}
