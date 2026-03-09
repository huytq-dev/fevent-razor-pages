namespace Application;

[RegisterService(typeof(IAccountSecurityService))]
public class AccountSecurityService(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IJwtTokensService jwtTokensService,
    IRedisCacheService redisCache,
    IBackgroundJobService backgroundJobService
    ) : IAccountSecurityService
{
    public async Task<PageResponse<SendEmailVerificationResponse>> SendEmailVerificationAsync(SendEmailVerificationRequest request, CancellationToken ct = default)
    {
        var emailData = await unitOfWork.Users.GetEmailStatusAsync(request.Email);
        if (emailData.IsVerified)
            return PageResponse<SendEmailVerificationResponse>.Fail("Tài khoản này đã được xác minh");

        await GenerateVerificationTokenAndSendAsync(emailData.Email);

        return PageResponse<SendEmailVerificationResponse>.Ok(new SendEmailVerificationResponse
        {
            Message = "Verification email sent."
        });
    }

    public async Task SendVerificationEmailToUserAsync(string email, CancellationToken ct = default)
    {
        await GenerateVerificationTokenAndSendAsync(email);
    }

    public async Task<PageResponse<ConfirmEmailResponse>> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken ct = default)
    {


        var token = Uri.UnescapeDataString(request.Token.Trim());
        var email = await redisCache.GetRecordAsync<string>(RedisKeys.EmailVerifyToken(token));

        if (string.IsNullOrEmpty(email))
            return PageResponse<ConfirmEmailResponse>.Fail("Token xác nhận email hết hạn hoặc không hợp lệ");

        var userEntity = await unitOfWork.Users.GetUserByEmailAsync(email);
        userEntity.IsVerified = true;

        if (await unitOfWork.SaveChangesAsync(ct) <= 0)
            return PageResponse<ConfirmEmailResponse>.Fail("Có lỗi khi lưu vào database");

        await redisCache.RemoveManyAsync(
            RedisKeys.EmailVerifyToken(token),
            RedisKeys.EmailVerifyUser(email),
            RedisKeys.UserAccessToken(userEntity.Id.ToString()),
            RedisKeys.UserRefreshToken(userEntity.Id.ToString())
        );

        return PageResponse<ConfirmEmailResponse>.Ok(new ConfirmEmailResponse
        {
            Message = "Email verified successfully."
        });
    }

    public async Task<PageResponse<ForgotPasswordResponse>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var emailStatus = await unitOfWork.Users.GetEmailStatusAsync(email);
        if (!emailStatus.IsVerified)
            return PageResponse<ForgotPasswordResponse>.Fail("Không có tài khoản khớp với email này");

        var oldToken = await redisCache.GetRecordAsync<string>(RedisKeys.PasswordResetUser(email));

        if (!string.IsNullOrEmpty(oldToken))
        {
            await redisCache.RemoveManyAsync(
                RedisKeys.PasswordResetToken(oldToken),
                RedisKeys.PasswordResetUser(email)
            );
        }

        if (!await unitOfWork.Users.IsEmailExistAsync(email))
            return PageResponse<ForgotPasswordResponse>.Fail(ExceptionMessages.NotFoundField("User", "Email", email));

        var token = TokenHelper.GenerateToken();
        await redisCache.SetRecordAsync(RedisKeys.PasswordResetToken(token), email, TimeSpan.FromMinutes(15));
        await redisCache.SetRecordAsync(RedisKeys.PasswordResetUser(email), token, TimeSpan.FromMinutes(15));

        var link = $"{AppConstants.BaseUrl}/reset-password?token={Uri.EscapeDataString(token)}";

        _ = backgroundJobService.EnqueueEmailAsync(new EmailContent
        {
            Receiver = email,
            Subject = "Reset your password",
            Body = $"Click <a href='{link}'>here</a> to reset your password (expires in 15 minutes)."
        });

        return PageResponse<ForgotPasswordResponse>.Ok(new ForgotPasswordResponse
        {
            Message = "If the email exists, a reset link has been sent."
        });
    }

    public async Task<PageResponse<ResetPasswordResponse>> ConfirmPasswordResetAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        if (request.NewPassword != request.ConfirmPassword)
            return PageResponse<ResetPasswordResponse>.Fail("Mật khẩu không khớp");

        var token = Uri.UnescapeDataString(request.Token.Trim());
        var email = await redisCache.GetRecordAsync<string>(RedisKeys.PasswordResetToken(token));

        if (string.IsNullOrEmpty(email))
            return PageResponse<ResetPasswordResponse>.Fail("Reset Password token hết hạn hoặc không hợp lệ");

        var hashedPassword = passwordHasher.Hash(request.NewPassword);
        var user = await unitOfWork.Users.GetUserByEmailAsync(email);
        user.PasswordHash = hashedPassword;

        if (await unitOfWork.SaveChangesAsync(ct) <= 0)
            return PageResponse<ResetPasswordResponse>.Fail("Lỗi khi lưu mật khẩu mới vào database");


        await redisCache.RemoveManyAsync(
            RedisKeys.PasswordResetToken(token),
            RedisKeys.PasswordResetUser(email),
            RedisKeys.UserAccessToken(user.Id.ToString()),
            RedisKeys.UserRefreshToken(user.Id.ToString())
        );

        return PageResponse<ResetPasswordResponse>.Ok("Đổi mật khẩu thành công");
    }

    public async Task<PageResponse<ChangePasswordResponse>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        if (request.NewPassword != request.ConfirmPassword)
            return PageResponse<ChangePasswordResponse>.Fail("Mật khẩu không khớp");

        var user = await unitOfWork.Users.GetUserForPasswordChangeAsync(userId, ct);

        if (string.IsNullOrWhiteSpace(user.PasswordHash) ||
            !passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            return PageResponse<ChangePasswordResponse>.Fail("Mật khẩu hiện tại không đúng");

        user.PasswordHash = passwordHasher.Hash(request.NewPassword);

        if (await unitOfWork.SaveChangesAsync(ct) <= 0)
            return PageResponse<ChangePasswordResponse>.Fail("Lỗi khi lưu mật khẩu mới vào database");

        return PageResponse<ChangePasswordResponse>.Ok(new ChangePasswordResponse
        {
            Message = "Đổi mật khẩu thành công."
        });
    }

    #region Helper Methods

    private async Task GenerateVerificationTokenAndSendAsync(string email)
    {
        var oldToken = await redisCache.GetRecordAsync<string>(RedisKeys.EmailVerifyUser(email));
        if (!string.IsNullOrEmpty(oldToken))
        {
            await redisCache.RemoveManyAsync(
                RedisKeys.EmailVerifyToken(oldToken),
                RedisKeys.EmailVerifyUser(email)
            );
        }

        var token = jwtTokensService.GenerateEmailVerifyToken();
        await redisCache.SetRecordAsync(RedisKeys.EmailVerifyToken(token), email, TimeSpan.FromMinutes(15));
        await redisCache.SetRecordAsync(RedisKeys.EmailVerifyUser(email), token, TimeSpan.FromMinutes(15));

        // Nếu muón test đoạn này thì thay lại link trong ClientAppSettings
        var link = $"{AppConstants.BaseUrl}/verify-email?token={Uri.EscapeDataString(token)}";

        _ = backgroundJobService.EnqueueEmailAsync(new EmailContent
        {
            Receiver = email,
            Subject = "Verify your email",
            Body = $"Please verify your email by clicking <a href='{link}'>here</a>."
        });
    }

    #endregion
}
