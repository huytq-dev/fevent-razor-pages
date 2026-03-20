
using Microsoft.AspNetCore.Mvc.RazorPages;
using Infrastructure;
using Domain;

namespace UI.Pages.Profile
{
    public class ViewProfileModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public User? Student { get; set; }

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
            }
        }
    }
}
