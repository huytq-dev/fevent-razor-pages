using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Infrastructure;
using Domain;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using UI.Helpers;

namespace UI.Pages.Admin
{
    public class UserManagerModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IPasswordHasher _passwordHasher;

        public UserManagerModel(ApplicationDbContext db, IPasswordHasher passwordHasher)
        {
            _db = db;
            _passwordHasher = passwordHasher;
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

        public async Task<IActionResult> OnPostCreateUserAsync(string fullName, string email, string password, Guid roleId)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || roleId == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Vui lòng điền đầy đủ thông tin.";
                return RedirectToPage();
            }

            fullName = fullName.Trim();
            email = email.Trim();

            if (!Regex.IsMatch(fullName, @"^[A-Za-zÀ-ỹ\s]{2,60}$"))
            {
                TempData["ErrorMessage"] = "Họ tên chỉ được chứa chữ và khoảng trắng, từ 2-60 ký tự.";
                return RedirectToPage();
            }

            var emailValidator = new EmailAddressAttribute();
            if (!emailValidator.IsValid(email))
            {
                TempData["ErrorMessage"] = "Email không đúng định dạng.";
                return RedirectToPage();
            }

            if (password.Length < 8 || !Regex.IsMatch(password, @"^(?=.*[A-Za-z])(?=.*\d).{8,}$"))
            {
                TempData["ErrorMessage"] = "Mật khẩu phải từ 8 ký tự và chứa cả chữ lẫn số.";
                return RedirectToPage();
            }

            if (await _db.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower()))
            {
                TempData["ErrorMessage"] = "Email đã tồn tại trong hệ thống.";
                return RedirectToPage();
            }

            var role = await _db.Roles.FindAsync(roleId);
            if (role == null)
            {
                TempData["ErrorMessage"] = "Vai trò không hợp lệ.";
                return RedirectToPage();
            }

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                FullName = fullName,
                Email = email,
                Username = email.Split('@')[0], 
                PasswordHash = _passwordHasher.Hash(password),
                IsVerified = true,
                IsDeleted = false
            };

            await _db.Users.AddAsync(newUser);
            
            _db.UserRoles.Add(new UserRole
            {
                UserId = newUser.Id,
                RoleId = role.Id
            });

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã thêm thành công người dùng {newUser.FullName}.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetExportAsync()
        {
            var sessionUserId = HttpContext.Session.GetString("UserId");
            var roleName = HttpContext.Session.GetString("RoleName");
            if (string.IsNullOrEmpty(sessionUserId) || string.IsNullOrEmpty(roleName) || roleName != "Admin")
            {
                return RedirectToPage("/Home/Index");
            }

            var users = await _db.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            var rows = users.Select((u, i) => new[]
            {
                (i + 1).ToString(),
                u.FullName,
                u.Email,
                u.StudentId,
                u.Major,
                u.PhoneNumber,
                u.DOB?.ToString("yyyy-MM-dd"),
                u.UserRoles.FirstOrDefault()?.Role?.RoleName,
                u.IsDeleted ? "Suspended" : "Active",
                u.AvatarUrl
            });

            var workbook = ExcelExportHelper.BuildWorkbook(
                ["No", "Full Name", "Email", "Student ID", "Major", "Phone", "DOB", "Role", "Status", "Avatar URL"],
                rows,
                "Users");

            var fileName = $"users-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
            return File(workbook, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
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
