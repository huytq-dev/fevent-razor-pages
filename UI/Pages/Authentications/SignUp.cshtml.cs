namespace UI;

public class SignUpModel(IAuthServices authServices, ICatalogService catalogService) : PageModel
{
    [BindProperty]
    public SignUpRequest Input { get; set; }

    public IReadOnlyList<MajorFilterItem> Majors { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Majors = await catalogService.GetMajorsAsync();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct = default)
    {
        Input.Username = Input.Email;
        Input.Role = RoleType.Participant.ToString();

        var result = await authServices.SignUpAsync(Input, ct);

        if (!result.IsSuccess)
        {
            Majors = await catalogService.GetMajorsAsync();
            ModelState.AddModelError(string.Empty, result.Message);
            return Page();
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToPage("/Authentications/Login");
    }
}