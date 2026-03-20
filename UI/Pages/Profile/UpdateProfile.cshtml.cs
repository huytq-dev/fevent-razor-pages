using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Domain;
using Infrastructure;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace UI.Pages.Profile
{
    public class UpdateProfileModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        [BindProperty]
        public User? AppUser { get; set; }
        [TempData]
        public string? SuccessMessage { get; set; }
        [TempData]
        public string? ErrorMessage { get; set; }

        public UpdateProfileModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return RedirectToPage("/Authentications/Login");
            }
            AppUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (AppUser == null)
            {
                ErrorMessage = "Không tìm thấy thông tin người dùng.";
                return RedirectToPage("/Profile/ViewProfile");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validate FullName: chỉ chữ, khoảng trắng, 2-50 ký tự
            if (string.IsNullOrWhiteSpace(AppUser?.FullName) || !System.Text.RegularExpressions.Regex.IsMatch(AppUser.FullName, @"^[A-Za-zÀ-ỹ\s]{2,50}$"))
            {
                ModelState.AddModelError("AppUser.FullName", "Tên chỉ được chứa chữ và khoảng trắng, từ 2-50 ký tự.");
            }
            // Validate Phone: 10 số, bắt đầu bằng 0
            if (!string.IsNullOrWhiteSpace(AppUser?.PhoneNumber) && !System.Text.RegularExpressions.Regex.IsMatch(AppUser.PhoneNumber, "^0[0-9]{9}$"))
            {
                ModelState.AddModelError("AppUser.PhoneNumber", "Số điện thoại phải đủ 10 số, bắt đầu bằng 0.");
            }
            // Validate DOB >= 18 tuổi
            if (AppUser?.DOB != null)
            {
                var today = DateTime.Today;
                var age = today.Year - AppUser.DOB.Value.Year;
                if (AppUser.DOB.Value.Date > today.AddYears(-age)) age--;
                if (age < 18)
                {
                    ModelState.AddModelError("AppUser.DOB", "Bạn phải đủ 18 tuổi trở lên.");
                }
            }
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Dữ liệu không hợp lệ.";
                return Page();
            }
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return RedirectToPage("/Authentications/Login");
            }
            var userInDb = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (userInDb == null)
            {
                ErrorMessage = "Không tìm thấy thông tin người dùng.";
                return RedirectToPage("/Profile/ViewProfile");
            }
            // Cập nhật các trường cho phép sửa
            userInDb.FullName = AppUser?.FullName ?? userInDb.FullName;
            userInDb.Major = AppUser?.Major ?? userInDb.Major;
            userInDb.PhoneNumber = AppUser?.PhoneNumber ?? userInDb.PhoneNumber;
            userInDb.DOB = AppUser?.DOB ?? userInDb.DOB;
            // Nếu có thêm trường nào cho phép sửa thì bổ sung ở đây
            await _db.SaveChangesAsync();
            SuccessMessage = "Cập nhật thông tin thành công!";
            return RedirectToPage("/Profile/ViewProfile");
        }
    }
}
