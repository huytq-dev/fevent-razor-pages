namespace UI;

public class SignUpModel(IAuthServices authServices) : PageModel
{
    [BindProperty]
    public SignUpRequest Input { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct = default)
    {
        Input.Username = Input.Email;
        Input.Role = RoleType.Participant.ToString();

        var result = await authServices.SignUpAsync(Input, ct);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return Page();
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToPage("/Authentications/Login");
    }
}