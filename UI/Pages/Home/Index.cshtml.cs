namespace UI;

public class HomeIndexModel : PageModel
{
    public IActionResult OnGet()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrWhiteSpace(userId)) return RedirectToPage("/Authentications/Login");

        return Page();
    }
}
