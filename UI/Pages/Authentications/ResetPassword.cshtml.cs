namespace UI;

public class ResetPasswordModel(IAccountSecurityService accountSecurityService) : PageModel
{
    [BindProperty]
    public ResetPasswordRequest Input { get; set; } = null!;

    public string? Token { get; set; }
    public bool TokenValid { get; set; } = true;

    public IActionResult OnGet()
    {
        Token = Request.Query["token"].ToString();
        if (string.IsNullOrWhiteSpace(Token))
        {
            TokenValid = false;
            return Page();
        }
        Input = new ResetPasswordRequest { Token = Token, NewPassword = "", ConfirmPassword = "" };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return Page();

        var result = await accountSecurityService.ConfirmPasswordResetAsync(Input, ct);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return Page();
        }

        TempData["SuccessMessage"] = "Đặt lại mật khẩu thành công. Vui lòng đăng nhập.";
        return RedirectToPage("/Authentications/Login");
    }
}
