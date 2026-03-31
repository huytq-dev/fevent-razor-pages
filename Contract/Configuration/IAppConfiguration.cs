namespace Contract;

public interface IAppConfiguration
{
    //JwtOptions GetJwtOptions();
    CloudinaryOptions GetCloudinaryOptions();
    string GetBaseUrl();
    //string? GetSqlServerConnectionString();
    //public string? GetEnvironment();
}
