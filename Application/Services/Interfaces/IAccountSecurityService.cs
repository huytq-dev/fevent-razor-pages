namespace Application;

public interface IAccountSecurityService
{
    // Email Verification
    Task<PageResponse<SendEmailVerificationResponse>> SendEmailVerificationAsync(
        SendEmailVerificationRequest request,
        CancellationToken ct = default);

    Task SendVerificationEmailToUserAsync(string email, CancellationToken ct = default);

    Task<PageResponse<ConfirmEmailResponse>> ConfirmEmailAsync(
         ConfirmEmailRequest request,
         CancellationToken ct = default);

    // Password Reset
    Task<PageResponse<ForgotPasswordResponse>> ForgotPasswordAsync(
         ForgotPasswordRequest request,
         CancellationToken ct = default);

    Task<PageResponse<ResetPasswordResponse>> ConfirmPasswordResetAsync(
        ResetPasswordRequest request,
        CancellationToken ct = default);

    Task<PageResponse<ChangePasswordResponse>> ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request,
        CancellationToken ct = default);
}
