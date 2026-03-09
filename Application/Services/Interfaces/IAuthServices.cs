namespace Application;

public interface IAuthServices
{
    Task<PageResponse<SignInResponse>> SignInAsync(SignInRequest request, CancellationToken ct = default);
    Task<PageResponse<SignUpResponse>> SignUpAsync(SignUpRequest request, CancellationToken ct = default);
}
