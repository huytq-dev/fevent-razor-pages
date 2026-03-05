using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure;

public class ClubConfiguration : BaseEntityConfiguration<Club, Guid>
{
    public override void Configure(EntityTypeBuilder<Club> builder)
    {
        // Gọi base để cấu hình Id, CreatedAt, UpdatedAt...
        base.Configure(builder);

        builder.ToTable("Clubs");

        // --- Properties ---
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Code)
            .HasMaxLength(50) // Ví dụ: F-CODE, JS-CLUB
            .IsUnicode(false); // Code thường không cần dấu

        builder.Property(c => c.Description)
            .HasMaxLength(2000); // Cho phép mô tả dài

        builder.Property(c => c.LogoUrl)
            .HasMaxLength(500);

        builder.Property(c => c.SocialLink)
            .HasMaxLength(500);

        // --- Relationships ---

        // 1. Club - Events (1 Club có nhiều Event)
        // Khi xóa Club -> Set Event.ClubId = null (để giữ lại lịch sử Event)
        builder.HasMany(c => c.Events)
            .WithOne(e => e.Club)
            .HasForeignKey(e => e.ClubId)
            .OnDelete(DeleteBehavior.SetNull);

        // 2. Club - Members (1 Club có nhiều Member)
        builder.HasMany(c => c.Members)
            .WithOne(m => m.Club)
            .HasForeignKey(m => m.ClubId)
            .OnDelete(DeleteBehavior.Cascade);

        // --- Indexes ---
        builder.HasIndex(c => c.Name); 
        builder.HasIndex(c => c.Code).IsUnique();
    }
}