namespace UI;

public class ForgotPasswordModel(IAccountSecurityService accountSecurityService) : PageModel
{
    [BindProperty]
    public ForgotPasswordRequest Input { get; set; } = new() { Email = "" };

    public async Task<IActionResult> OnPostAsync(CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return Page();

        var result = await accountSecurityService.ForgotPasswordAsync(Input, ct);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return Page();
        }

        TempData["SuccessMessage"] = "Hướng dẫn đặt lại mật khẩu đã được gửi đến email của bạn.";
        return RedirectToPage("/Authentications/ForgotPassword");
    }
}
