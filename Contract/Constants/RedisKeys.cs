namespace Contract;

public static class RedisKeys
{
    private const string AuthNamespace = "auth";
    public static string UserAccessToken(string userId) => $"{AuthNamespace}:session:{userId}:access";
    public static string UserRefreshToken(string userId) => $"{AuthNamespace}:session:{userId}:refresh";

    private const string VerifyNamespace = "verify";
    public static string EmailVerifyToken(string token) => $"{AuthNamespace}:{VerifyNamespace}:token:{token}";
    public static string EmailVerifyUser(string email) => $"{AuthNamespace}:{VerifyNamespace}:user:{email}";

    private const string ResetNamespace = "reset";
    public static string PasswordResetToken(string token) => $"{AuthNamespace}:{ResetNamespace}:token:{token}";
    public static string PasswordResetUser(string email) => $"{AuthNamespace}:{ResetNamespace}:user:{email}";

}