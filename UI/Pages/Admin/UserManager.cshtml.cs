using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Infrastructure;
using Domain;

namespace UI.Pages.Admin
{
    public class UserManagerModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public UserManagerModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public IList<User> Users { get; set; } = new List<User>();
        public IList<Role> AvailableRoles { get; set; } = new List<Role>();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public int TotalUsersCount { get; set; }
        public int ActiveOrganizersCount { get; set; }
        public int BannedUsersCount { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var sessionUserId = HttpContext.Session.GetString("UserId");
            var roleName = HttpContext.Session.GetString("RoleName");
            if (string.IsNullOrEmpty(sessionUserId) || string.IsNullOrEmpty(roleName) || roleName != "Admin")
            {
                return RedirectToPage("/Home/Index");
            }

            var query = _db.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                query = query.Where(u => u.FullName.Contains(SearchTerm) || u.Email.Contains(SearchTerm) || (u.StudentId != null && u.StudentId.Contains(SearchTerm)));
            }

            Users = await query.ToListAsync();
            AvailableRoles = await _db.Roles.ToListAsync();

            // Stats
            TotalUsersCount = await _db.Users.CountAsync();
            BannedUsersCount = await _db.Users.CountAsync(u => u.IsDeleted);
            ActiveOrganizersCount = Users.Count(u => u.UserRoles.Any(ur => ur.Role.RoleName == "Organizer") && !u.IsDeleted);

            return Page();
        }

        public async Task<IActionResult> OnPostChangeRoleAsync(Guid userId, Guid newRoleId)
        {
            if (userId == Guid.Empty || newRoleId == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Invalid payload.";
                return RedirectToPage();
            }

            var user = await _db.Users.Include(u => u.UserRoles).FirstOrDefaultAsync(u => u.Id == userId);
            var role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == newRoleId);

            if (user == null || role == null)
            {
                TempData["ErrorMessage"] = "User or Role not found.";
                return RedirectToPage();
            }

            // Xóa role cũ và add role mới (1 user = 1 role chính theo requirement, nếu nhiều role thì sửa logic này)
            if (user.UserRoles.Any())
            {
                _db.UserRoles.RemoveRange(user.UserRoles);
            }

            _db.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id
            });

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Role of {user.FullName} changed to {role.RoleName}.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleBanAsync(Guid userId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToPage();
            }

            user.IsDeleted = !user.IsDeleted;
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = user.IsDeleted 
                ? $"User {user.FullName} has been banned."
                : $"User {user.FullName} has been un-banned.";

            return RedirectToPage();
        }
    }
}
