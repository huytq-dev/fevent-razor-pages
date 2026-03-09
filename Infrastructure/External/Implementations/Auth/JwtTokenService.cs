namespace Infrastructure;

[RegisterService(typeof(IJwtTokensService))]
public class JwtTokenService : IJwtTokensService
{

    public string GenerateEmailVerifyToken()
           => TokenHelper.GenerateToken();

}