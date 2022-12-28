using System.Security.Claims;
using Mimisbrunnr.Integration.User;
using Mimisbrunnr.Wiki.Contracts;
using Riok.Mapperly.Abstractions;

namespace Mimisbrunnr.Web.Mapping;

[Mapper]
public partial class UserMapper
{
    public static UserMapper Instance { get; } = new UserMapper();

    public partial UserInfo ToInfo(UserModel user);

    public partial UserModel ToModel(UserInfo user);

    public partial UserModel ToModel(Users.User user);

    public partial UserProfileModel ToProfileModel(Users.User user);

    public UserViewModel ToViewModel(Users.User user)
    {
        return new UserViewModel()
        {
            Email = user.Email.ToLower(),
            Name = user.Name,
            AvatarUrl = user.AvatarUrl,
            IsAdmin = user.Role == Users.UserRole.Admin,
            Enable = user.Enable
        };
    }

        public UserInfo ToInfo(ClaimsPrincipal principal)
    {
        var user = new UserInfo
        {
            Email = principal.FindFirst(ClaimTypes.Email)?.Value?.ToLower() ?? principal.FindFirst("email")?.Value?.ToLower(),
            Name = principal.Identity?.Name,
            AvatarUrl = principal.FindFirst("picture")?.Value
        };
        if (string.IsNullOrEmpty(user.Email))
            return null;
        return user;
    }

}