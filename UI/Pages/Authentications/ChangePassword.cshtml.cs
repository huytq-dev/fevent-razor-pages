namespace UI;

public class ChangePasswordModel(IAccountSecurityService accountSecurityService) : PageModel
{
    [BindProperty]
    public ChangePasswordRequest Input { get; set; } = new() { CurrentPassword = "", NewPassword = "", ConfirmPassword = "" };

    public IActionResult OnGet()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out _))
            return RedirectToPage("/Authentications/Login");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct = default)
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return RedirectToPage("/Authentications/Login");

        if (!ModelState.IsValid) return Page();

        var result = await accountSecurityService.ChangePasswordAsync(userId, Input, ct);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return Page();
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToPage("/Authentications/ChangePassword");
    }
}
