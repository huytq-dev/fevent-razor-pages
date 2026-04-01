namespace UI;

public class ViewProfileModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public User? Student { get; set; }
    public int EventsAttendedCount { get; set; }
    public int ClubsJoinedCount { get; set; }

    public ViewProfileModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public void OnGet()
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (!string.IsNullOrEmpty(userIdStr) && Guid.TryParse(userIdStr, out var userId))
        {
            Student = _db.Users.FirstOrDefault(u => u.Id == userId);
            EventsAttendedCount = _db.EventRegistrations.Count(er => er.UserId == userId && er.Status == RegistrationStatus.CheckedIn);
            ClubsJoinedCount = _db.ClubMembers.Count(cm => cm.UserId == userId && cm.Status == ClubMemberStatus.Active);
        }
    }
}
