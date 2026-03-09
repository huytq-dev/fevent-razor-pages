namespace UI;

public class VerifyEmailModel(IAccountSecurityService accountSecurityService) : PageModel
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(CancellationToken ct = default)
    {
        var token = Request.Query["token"].ToString();
        if (string.IsNullOrWhiteSpace(token))
        {
            IsSuccess = false;
            Message = "Link xác nhận không hợp lệ. Vui lòng kiểm tra lại email.";
            return Page();
        }

        var result = await accountSecurityService.ConfirmEmailAsync(
            new ConfirmEmailRequest { Token = token }, ct);

        IsSuccess = result.IsSuccess;
        Message = result.IsSuccess ? result.Message ?? "Email đã được xác thực thành công." : result.Message ?? "Có lỗi xảy ra.";

        return Page();
    }
}
