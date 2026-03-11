namespace UI;

public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (!string.IsNullOrWhiteSpace(userId)) return RedirectToPage("/Home/Index");

        return RedirectToPage("/Authentications/Login");
    }
}
