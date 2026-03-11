namespace UI;

public class LoginModel(IAuthServices authServices) : PageModel
{
    [BindProperty]
    public SignInRequest Input { get; set; }

    public string? ErrorMessage { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public IActionResult OnGet()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (!string.IsNullOrWhiteSpace(userId))
        {
            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                return Redirect(ReturnUrl);
            return RedirectToPage("/Home/Index");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid) return Page();

        var result = await authServices.SignInAsync(Input, ct);

        if (!result.IsSuccess)
        {
            ErrorMessage = result.Message;
            return Page();
        }

        var auth = result.Data!;

        // Lưu vào Session
        HttpContext.Session.SetString("UserId", auth.Id.ToString());
        HttpContext.Session.SetString("FullName", auth.FullName);
        HttpContext.Session.SetString("Email", auth.Email);
        HttpContext.Session.SetString("AvatarUrl", auth.AvatarUrl ?? "");
        //HttpContext.Session.SetString("Role", auth.Role); đang bị lỗi

        if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            return Redirect(ReturnUrl);
        return RedirectToPage("/Home/Index");
    }
}
