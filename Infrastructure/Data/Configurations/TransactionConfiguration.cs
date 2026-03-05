namespace Infrastructure;

public class TransactionConfiguration : BaseEntityConfiguration<Transaction, Guid>
{
    public override void Configure(EntityTypeBuilder<Transaction> builder)
    {
        base.Configure(builder);

        builder.ToTable("Transactions");

        // --- Properties ---
        builder.Property(e => e.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(e => e.TransactionNo)
            .HasMaxLength(50);

        builder.Property(e => e.BankCode)
            .HasMaxLength(10);

        builder.Property(e => e.OrderInfo)
            .HasMaxLength(255);

        builder.Property(e => e.Status)
            .HasConversion<int>()
            .IsRequired();

        // --- Relationships ---
        builder.HasOne(e => e.EventRegistration)
            .WithMany(er => er.Transactions)
            .HasForeignKey(e => e.EventRegistrationId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- Indexes ---
        builder.HasIndex(e => e.EventRegistrationId);
        builder.HasIndex(e => e.TransactionNo).IsUnique().HasFilter("TransactionNo IS NOT NULL");
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.CreatedAt);
    }
}