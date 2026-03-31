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
            var roleName = HttpContext.Session.GetString("RoleName");
            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                return Redirect(ReturnUrl);

            if (roleName == "Admin")
                return RedirectToPage("/Admin/UserManager");
            if (roleName == "Organizer")
                return RedirectToPage("/Organizer/CreateEvent");

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
        HttpContext.Session.SetString("RoleName", auth.RoleName ?? "Participant");

        if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            return Redirect(ReturnUrl);

        if (auth.RoleName == "Admin")
            return RedirectToPage("/Admin/UserManager");
        if (auth.RoleName == "Organizer")
            return RedirectToPage("/Management/CreateEvent");

        return RedirectToPage("/Home/Index");
    }
}
