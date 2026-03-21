namespace Infrastructure;

[RegisterService(typeof(IUsersRepository))]
public class UsersRepository : GenericRepository<User>, IUsersRepository
{
    public UsersRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> IsEmailExistAsync(string email)
        => await _set
            .AsNoTracking()
            .AnyAsync(u => u.Email == email && !u.IsDeleted);

    public async Task<bool> IsUsernameExistAsync(string username)
        => await _set
            .AsNoTracking()
            .AnyAsync(u => u.Username == username && !u.IsDeleted);

    public async Task<bool> IsStudentIdExistAsync(string studentId)
        => await _set
            .AsNoTracking()
            .AnyAsync(u => u.StudentId == studentId && !u.IsDeleted);

    public async Task<User?> GetUserByEmailAsync(string email)
        => await _set
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
            // ?? throw new NotFoundException(ExceptionMessages.NotFoundField("User", "Email", email));

    public async Task<User?> LoginAsync(SignInRequest request)
        => await _set
            .AsNoTracking()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u =>
                (u.Email == request.Email)
                && !u.IsDeleted);

    public async Task<UserCredentials?> GetCredentialsForLoginAsync(string username)
        => await _set
            .AsNoTracking()
            .Where(u => (u.Username == username || u.Email == username) && !u.IsDeleted)
            .Select(u => new UserCredentials
            {
                Id = u.Id,
                Email = u.Email,
                Password = u.PasswordHash ?? string.Empty,
                FullName = u.FullName,
                AvatarURL = u.AvatarUrl ?? string.Empty,
                IsVerified = u.IsVerified,
                Role = u.UserRoles.Select(ur => ur.Role.RoleName).FirstOrDefault() ?? "Participant"
            })
            .FirstOrDefaultAsync();
            // ?? throw new NotFoundException(ExceptionMessages.NotFoundField("User", "Username", username));

    public async Task<UserCredentials?> GetCredentialsForRefreshTokenAsync(Guid userId)
        => await _set
            .AsNoTracking()
            .Where(u => u.Id == userId && !u.IsDeleted)
            .Select(u => new UserCredentials
            {
                Id = u.Id,
                Email = u.Email,
                Password = string.Empty,
                FullName = u.FullName,
                AvatarURL = u.AvatarUrl ?? string.Empty,
                IsVerified = u.IsVerified,
                Role = u.UserRoles.Select(ur => ur.Role.RoleName).FirstOrDefault() ?? "Participant"
            })
            .FirstOrDefaultAsync();
            // ?? throw new NotFoundException(ExceptionMessages.NotFound("User", userId));

    public async Task<UserCredentials?> GetByIdForRefreshTokenAsync(Guid id)
        => await GetCredentialsForRefreshTokenAsync(id);

    public async Task<EmailStatus> GetEmailStatusAsync(string email)
        => await _set
            .AsNoTracking()
            .Where(u => u.Email == email && !u.IsDeleted)
            .Select(u => new EmailStatus
            {
                Email = u.Email,
                IsVerified = u.IsVerified
            })
            .FirstOrDefaultAsync()
            ?? new EmailStatus { Email = email, IsVerified = false };

    public async Task UpdateUserAvatarAsync(Guid userId, string avatarUrl)
    {
        var user = await _set
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
        // ?? throw new NotFoundException(ExceptionMessages.NotFound("User", userId));
        if (user is null) return;

        user.AvatarUrl = avatarUrl;
    }

    public async Task UpdateRefreshTokenAsync(Guid id, string refreshToken)
    {
        var user = await _set
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        // ?? throw new NotFoundException(ExceptionMessages.NotFound("User", id));
        if (user is null) return;

        user.RefreshToken = refreshToken ?? string.Empty;
        _set.Update(user);
    }

    public async Task<User?> GetProfileAsync(Guid id, CancellationToken ct = default)
        => await _set
            .AsNoTracking()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.SocialLinks)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, ct);
        // ?? throw new NotFoundException(ExceptionMessages.NotFound("User", id));

    public async Task<User?> GetUserForProfileUpdateAsync(Guid id, CancellationToken ct = default)
        => await _set
            .Include(u => u.SocialLinks)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, ct);
        // ?? throw new NotFoundException(ExceptionMessages.NotFound("User", id));

    public async Task<User?> GetUserForPasswordChangeAsync(Guid id, CancellationToken ct = default)
        => await _set
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, ct);
        // ?? throw new NotFoundException(ExceptionMessages.NotFound("User", id));
}
