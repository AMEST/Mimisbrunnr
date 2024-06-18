using System.Security.Claims;
using Mimisbrunnr.Integration.User;
using Mimisbrunnr.Wiki.Contracts;
using Riok.Mapperly.Abstractions;

namespace Mimisbrunnr.Web.Mapping;

[Mapper]
public static partial class UserMapper
{

    public static partial UserInfo ToInfo(this UserModel user);

    public static partial UserModel ToModel(this UserInfo user);

    public static partial UserModel ToModel(this Users.User user);

    public static partial UserProfileModel ToProfileModel(this Users.User user);

    public static UserViewModel ToViewModel(this Users.User user)
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

    public static UserInfo ToInfo(this ClaimsPrincipal principal)
    {
        var user = new UserInfo
        {
            Email = principal.FindFirst(ClaimTypes.Email)?.Value?.ToLower() ??
                    principal.FindFirst("email")?.Value?.ToLower(),
            Name = principal.Identity?.Name ?? principal.FindFirst("name")?.Value,
            AvatarUrl = principal.FindFirst("picture")?.Value
        };
        if (string.IsNullOrEmpty(user.Email))
            return null;
        return user;
    }
}