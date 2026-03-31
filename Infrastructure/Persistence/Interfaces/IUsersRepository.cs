namespace Infrastructure;

public interface IUsersRepository : IGenericRepository<User>
{
    //Task<UserResponse> GetUserByIdAsync(Guid id);
    Task<bool> IsEmailExistAsync(string email);
    Task<bool> IsUsernameExistAsync(string username);
    Task<bool> IsStudentIdExistAsync(string studentId);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetProfileAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetUserForProfileUpdateAsync(Guid id, CancellationToken ct = default);
    //Task<User> UpdateUserProfileAsync(Guid id, UpdateUserProfileRequest request);

    // Authentication
    Task<UserCredentials?> GetCredentialsForLoginAsync(string username);
    Task<UserCredentials?> GetCredentialsForRefreshTokenAsync(Guid userId);

    Task<User?> LoginAsync(SignInRequest request);

    // Email
    Task<EmailStatus> GetEmailStatusAsync(string email);

    // Avatar
    Task UpdateUserAvatarAsync(Guid userId, string avatarUrl);
    // Token
    Task UpdateRefreshTokenAsync(Guid id, string refreshToken);
    Task<UserCredentials?> GetByIdForRefreshTokenAsync(Guid id);
    Task<User?> GetUserForPasswordChangeAsync(Guid id, CancellationToken ct = default);

}
