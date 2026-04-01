namespace UI;

public class ScanQRModel : PageModel
{
    private readonly ApplicationDbContext _db;

    [TempData]
    public string? SuccessMessage { get; set; }
    [TempData]
    public string? ErrorMessage { get; set; }
    [TempData]
    public string? CheckedInUserName { get; set; }
    [TempData]
    public string? CheckedInUserAvatar { get; set; }
    [TempData]
    public string? CheckedInTicketCode { get; set; }

    public ScanQRModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public IActionResult OnGet()
    {
        // Verify if user is an Organizer or Admin
        var roleName = HttpContext.Session.GetString("RoleName");
        if (roleName != "Admin" && roleName != "Organizer")
        {
            return RedirectToPage("/Authentications/Login");
        }
        return Page();
    }

    public async Task<IActionResult> OnPostCheckInAsync(string ticketCode)
    {
        var roleName = HttpContext.Session.GetString("RoleName");
        if (roleName != "Admin" && roleName != "Organizer")
        {
            return RedirectToPage("/Authentications/Login");
        }
        
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var currentUserId))
        {
            return RedirectToPage("/Authentications/Login");
        }

        if (string.IsNullOrEmpty(ticketCode))
        {
            ErrorMessage = "Mã vé không hợp lệ.";
            return Page();
        }

        var registration = await _db.EventRegistrations
            .Include(r => r.User)
            .Include(r => r.Event)
                .ThenInclude(e => e.Collaborators)
            .FirstOrDefaultAsync(r => r.TicketCode == ticketCode);

        if (registration == null)
        {
            ErrorMessage = "Không tìm thấy thông tin đăng ký cho mã vé vừa quét.";
            return Page();
        }

        if (roleName != "Admin")
        {
            bool isAuthorized = registration.Event.OrganizerId == currentUserId 
                || registration.Event.Collaborators.Any(c => c.UserId == currentUserId);
            
            if (!isAuthorized)
            {
                ErrorMessage = "Bạn không có quyền điểm danh cho sự kiện này.";
                return Page();
            }
        }

        if (registration.Status == RegistrationStatus.Cancelled)
        {
            ErrorMessage = $"Vé này đã bị hủy. Khách hàng: {registration.User.FullName}";
            return Page();
        }
        
        if (registration.Status == RegistrationStatus.CheckedIn)
        {
            ErrorMessage = $"Vé đã được check-in trước đó vào lúc {registration.CheckInTime?.ToString("HH:mm dd/MM/yyyy")}.";
            return Page();
        }

        // Perform Check-in
        registration.Status = RegistrationStatus.CheckedIn;
        registration.CheckInTime = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();

        SuccessMessage = $"Khách hàng {registration.User.FullName} đã check-in thành công cho sự kiện '{registration.Event.Title}'.";
        CheckedInUserName = registration.User.FullName;
        CheckedInUserAvatar = registration.User.AvatarUrl;
        CheckedInTicketCode = registration.TicketCode;
        return Page();
    }
}
