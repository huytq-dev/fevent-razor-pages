namespace UI;

public class LogoutModel : PageModel
{
    public void OnGet()
    {
        // Không chuyển hướng, chỉ render giao diện xác nhận
    }

    public IActionResult OnPost()
    {
        HttpContext.Session.Clear(); // Xóa toàn bộ session
        return RedirectToPage("/Authentications/Login"); // Chuyển về trang đăng nhập
    }
}
