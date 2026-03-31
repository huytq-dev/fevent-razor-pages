namespace Infrastructure;

public partial class ApplicationDbContext
{
    // User entities
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<UserRole> UserRoles { get; set; } = null!;
    public DbSet<SocialLink> SocialLinks { get; set; } = null!;

    // Event entities
    public DbSet<Event> Events { get; set; } = null!;
    public DbSet<EventRegistration> EventRegistrations { get; set; } = null!;
    public DbSet<EventCollaborator> EventCollaborators { get; set; } = null!;
    public DbSet<EventImage> EventImages { get; set; } = null!;
    public DbSet<EventReview> EventReviews { get; set; } = null!;
    public DbSet<TicketType> TicketTypes { get; set; } = null!;

    // Transaction entities
    public DbSet<Transaction> Transactions { get; set; } = null!;

    // Reference entities
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Location> Locations { get; set; } = null!;
    public DbSet<Club> Clubs {  get; set; } = null!;
    public DbSet<ClubMember> ClubMembers { get; set; } = null!;
}
