namespace Application;

[RegisterService(typeof(IAuthServices))]
public class AuthService(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IAccountSecurityService accountSecurityService) : IAuthServices

{
    public async Task<PageResponse<SignInResponse>> SignInAsync(SignInRequest request, CancellationToken ct = default)
    {
        var user = await unitOfWork.Users.LoginAsync(request);

        if (user == null
            || string.IsNullOrWhiteSpace(user.PasswordHash)
            || !passwordHasher.Verify(request.Password, user.PasswordHash))
            return PageResponse<SignInResponse>.Fail("Sai email hoặc mật khẩu.");

        if (user.IsDeleted) return PageResponse<SignInResponse>.Fail("Tài khoản đã bị chặn.");
        if (!user.IsVerified) return PageResponse<SignInResponse>.Fail("Email chưa được xác thực.");

        var userDto = user.Adapt<SignInResponse>();
        // Kiểm tra role

        return PageResponse<SignInResponse>.Ok(userDto, "Đăng nhập thành công.");

        //var userCreds = rawUser.Adapt<UserCredentials>();
        //userCreds.Password = string.Empty;
        //userCreds.AvatarURL = rawUser.AvatarUrl ?? string.Empty;
        //userCreds.Role = rawUser.UserRoles
        //    .Select(ur => ur.Role.RoleName)
        //    .FirstOrDefault() ?? "Participant";

        //var token = jwtTokensService.GenerateAccessToken(userCreds);
        //var refreshToken = jwtTokensService.GenerateRefreshToken();
        //await UpdateRefreshTokenAsync(rawUser.Id, refreshToken, ct);
    }

    public async Task<PageResponse<SignUpResponse>> SignUpAsync(SignUpRequest request, CancellationToken ct = default)
    {
        if (await unitOfWork.Users.IsUsernameExistAsync(request.Username))
            return PageResponse<SignUpResponse>.Fail($"Tên tài khoản{request.Username} đã được lấy");

        if (await unitOfWork.Users.IsEmailExistAsync(request.Email))
            return PageResponse<SignUpResponse>.Fail($"Email {request.Email} đã tồn tại");

        var user = request.Adapt<User>();
        user.PasswordHash = passwordHasher.Hash(request.Password);
        user.IsVerified = false;

        var roleName = RoleType.Participant.ToString();
        var role = await unitOfWork.Roles.GetByNameAsync(roleName, ct);

        // Add Role mặc định là participant
        user.UserRoles = new List<UserRole>
        {
            new UserRole
            {
                RoleId = role.Id,
                User = user
            }
        };

        await unitOfWork.Users.AddAsync(user);

        if (await unitOfWork.SaveChangesAsync(ct) <= 0)
            return PageResponse<SignUpResponse>.Fail("Lỗi khi lưu tài khoản vào database");

        _ = accountSecurityService.SendVerificationEmailToUserAsync(user.Email, ct);

        return PageResponse<SignUpResponse>.Ok("Đăng kí thành công, Mail xác nhận đã được gửi");
    }
}
