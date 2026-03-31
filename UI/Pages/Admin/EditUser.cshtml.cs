using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Domain;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace UI.Pages.Admin;

public class EditUserModel(ApplicationDbContext db, IWebHostEnvironment hostEnvironment) : PageModel
{
    [BindProperty]
    public EditUserInput Input { get; set; } = new();

    [BindProperty]
    public IFormFile? AvatarFile { get; set; }

    public IList<Role> AvailableRoles { get; private set; } = new List<Role>();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var gate = EnsureAdmin();
        if (gate is not null)
        {
            return gate;
        }

        var user = await db.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
            return RedirectToPage("/Admin/UserManager");
        }

        AvailableRoles = await db.Roles.OrderBy(r => r.RoleName).ToListAsync();

        Input = new EditUserInput
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            StudentId = user.StudentId,
            Major = user.Major,
            PhoneNumber = user.PhoneNumber,
            DOB = user.DOB,
            Address = user.Address,
            AvatarUrl = user.AvatarUrl,
            IsDeleted = user.IsDeleted,
            RoleId = user.UserRoles.FirstOrDefault()?.RoleId
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        var gate = EnsureAdmin();
        if (gate is not null)
        {
            return gate;
        }

        AvailableRoles = await db.Roles.OrderBy(r => r.RoleName).ToListAsync(ct);

        if (!string.IsNullOrWhiteSpace(Input.FullName) && !Regex.IsMatch(Input.FullName, @"^[A-Za-zÀ-ỹ\s]{2,60}$"))
        {
            ModelState.AddModelError("Input.FullName", "Họ tên chỉ được chứa chữ và khoảng trắng, 2-60 ký tự.");
        }

        if (!string.IsNullOrWhiteSpace(Input.PhoneNumber) && !Regex.IsMatch(Input.PhoneNumber, "^0[0-9]{9}$"))
        {
            ModelState.AddModelError("Input.PhoneNumber", "Số điện thoại phải gồm 10 chữ số và bắt đầu bằng 0.");
        }

        if (Input.DOB.HasValue)
        {
            var dob = Input.DOB.Value.Date;
            if (dob > DateTime.Today)
            {
                ModelState.AddModelError("Input.DOB", "Ngày sinh không được lớn hơn ngày hiện tại.");
            }
            else
            {
                var age = DateTime.Today.Year - dob.Year;
                if (dob > DateTime.Today.AddYears(-age))
                {
                    age--;
                }

                if (age < 16)
                {
                    ModelState.AddModelError("Input.DOB", "Người dùng phải từ 16 tuổi trở lên.");
                }
            }
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await db.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == Input.Id, ct);
        if (user is null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
            return RedirectToPage("/Admin/UserManager");
        }

        var emailExists = await db.Users.AnyAsync(u => u.Id != user.Id && u.Email == Input.Email, ct);
        if (emailExists)
        {
            ModelState.AddModelError("Input.Email", "Email đã tồn tại trong hệ thống.");
            return Page();
        }

        user.FullName = Input.FullName.Trim();
        user.Email = Input.Email.Trim();
        user.StudentId = string.IsNullOrWhiteSpace(Input.StudentId) ? null : Input.StudentId.Trim();
        user.Major = string.IsNullOrWhiteSpace(Input.Major) ? null : Input.Major.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(Input.PhoneNumber) ? null : Input.PhoneNumber.Trim();
        user.DOB = Input.DOB;
        user.Address = string.IsNullOrWhiteSpace(Input.Address) ? null : Input.Address.Trim();
        user.IsDeleted = Input.IsDeleted;

        if (Input.RoleId.HasValue)
        {
            var role = await db.Roles.FirstOrDefaultAsync(r => r.Id == Input.RoleId.Value, ct);
            if (role is not null)
            {
                if (user.UserRoles.Any())
                {
                    db.UserRoles.RemoveRange(user.UserRoles);
                }

                db.UserRoles.Add(new UserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id
                });
            }
        }

        if (AvatarFile is not null)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(AvatarFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("AvatarFile", "Avatar chỉ hỗ trợ định dạng .jpg, .jpeg, .png, .webp.");
                return Page();
            }

            if (AvatarFile.Length > 2 * 1024 * 1024)
            {
                ModelState.AddModelError("AvatarFile", "Avatar không được vượt quá 2MB.");
                return Page();
            }

            // Lưu vào ContentRoot/uploads/avatars để app phục vụ qua đường dẫn /uploads
            var uploadsFolder = Path.Combine(hostEnvironment.ContentRootPath, "uploads", "avatars");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"admin_{user.Id:N}_{DateTime.UtcNow.Ticks}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            await using var fileStream = new FileStream(filePath, FileMode.Create);
            await AvatarFile.CopyToAsync(fileStream, ct);

            user.AvatarUrl = $"/uploads/avatars/{uniqueFileName}";
        }

        await db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = $"Đã cập nhật thông tin của {user.FullName}.";
        // Redirect về lại trang EditUser để thấy avatar mới, truyền id
        return RedirectToPage(new { id = user.Id });
    }

    private IActionResult? EnsureAdmin()
    {
        var sessionUserId = HttpContext.Session.GetString("UserId");
        var roleName = HttpContext.Session.GetString("RoleName");
        if (string.IsNullOrEmpty(sessionUserId) || string.IsNullOrEmpty(roleName) || roleName != "Admin")
        {
            return RedirectToPage("/Home/Index");
        }

        return null;
    }

    public sealed class EditUserInput
    {
        [Required]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc.")]
        [StringLength(60, MinimumLength = 2, ErrorMessage = "Họ tên từ 2 đến 60 ký tự.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        public string Email { get; set; } = string.Empty;

        [StringLength(30)]
        public string? StudentId { get; set; }

        [StringLength(100)]
        public string? Major { get; set; }

        [StringLength(10)]
        public string? PhoneNumber { get; set; }

        public DateTime? DOB { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        public string? AvatarUrl { get; set; }

        public bool IsDeleted { get; set; }

        public Guid? RoleId { get; set; }
    }
}
