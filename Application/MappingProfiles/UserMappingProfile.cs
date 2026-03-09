namespace Application;

public class UserMappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {

        config.NewConfig<User, SignInResponse>()
            .Map(dest => dest.RoleName, src => src.UserRoles.FirstOrDefault() != null
                                           ? src.UserRoles.First().Role.RoleName
                                           : "Participant")
            .TwoWays();

        config.NewConfig<SignUpRequest, User>()
            .Map(dest => dest.FullName, src => src.Fullname)
            .Ignore(dest => dest.PasswordHash)
            .TwoWays();
        // Code từ file cũ chưa xoá 

        config.NewConfig<UserCredentials, User>().TwoWays();

        config.NewConfig<SignUpResponse, User>().TwoWays();

        config.NewConfig<User, UserCredentials>()
            .Map(dest => dest.Role, src => src.UserRoles.FirstOrDefault() != null
                ? src.UserRoles.First().Role.RoleName
                : "Participant");

        config.NewConfig<User, UserProfileResponse>()
        .Map(dest => dest.Id, src => src.Id)
        .Map(dest => dest.StudentId, src => src.StudentId)
        .Map(dest => dest.UniversityId, src => src.StudentId)
        .Map(dest => dest.SchoolName, src => src.SchoolName)
        .Map(dest => dest.Major, src => src.Major)
        // 1. Xử lý Role: Lấy role đầu tiên, nếu không có thì null (hoặc xử lý ở DTO default)
        .Map(dest => dest.RoleName, src => src.UserRoles.FirstOrDefault() != null
                                           ? src.UserRoles.First().Role.RoleName
                                           : "Participant")
        // 2. Xử lý Gender: Chuyển Enum sang String
        .Map(dest => dest.Gender, src => src.Gender.HasValue ? src.Gender.Value.ToString() : null);

        config.NewConfig<SocialLink, SocialLinkResponse>()
            .Map(dest => dest.Platform, src => (int)src.Platform)
            .Map(dest => dest.Url, src => src.Url);
    }
}
