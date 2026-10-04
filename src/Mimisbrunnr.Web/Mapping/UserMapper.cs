using System.Security.Claims;
using Mimisbrunnr.Integration.User;
using Mimisbrunnr.Wiki.Contracts;
using Riok.Mapperly.Abstractions;

namespace Mimisbrunnr.Web.Mapping;

/// <summary>
/// Maps user entities to and from their web models
/// </summary>
[Mapper]
public static partial class UserMapper
{

    /// <summary>
    /// Maps a user model to user info
    /// </summary>
    /// <param name="user">User model to map</param>
    /// <returns>The mapped user info</returns>
    public static partial UserInfo ToInfo(this UserModel user);

    /// <summary>
    /// Maps user info to a user model
    /// </summary>
    /// <param name="user">User info to map</param>
    /// <returns>The mapped user model</returns>
    public static partial UserModel ToModel(this UserInfo user);

    /// <summary>
    /// Maps a user entity to a user model
    /// </summary>
    /// <param name="user">User entity to map</param>
    /// <returns>The mapped user model</returns>
    [MapperIgnoreSource(nameof(Users.User.Id))]
    [MapperIgnoreSource(nameof(Users.User.Website))]
    [MapperIgnoreSource(nameof(Users.User.Post))]
    [MapperIgnoreSource(nameof(Users.User.Department))]
    [MapperIgnoreSource(nameof(Users.User.Organization))]
    [MapperIgnoreSource(nameof(Users.User.Location))]
    [MapperIgnoreSource(nameof(Users.User.Enable))]
    [MapperIgnoreSource(nameof(Users.User.Role))]
    public static partial UserModel ToModel(this Users.User user);


    /// <summary>
    /// Maps a user entity to a user profile model
    /// </summary>
    /// <param name="user">User entity to map</param>
    /// <returns>The mapped user profile model</returns>
    [MapperIgnoreSource(nameof(Users.User.Id))]
    [MapperIgnoreSource(nameof(Users.User.Enable))]
    [MapperIgnoreSource(nameof(Users.User.Role))]
    public static partial UserProfileModel ToProfileModel(this Users.User user);

    /// <summary>
    /// Maps a user entity to a user view model
    /// </summary>
    /// <param name="user">User entity to map</param>
    /// <returns>The mapped user view model</returns>
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

    /// <summary>
    /// Extracts user info from the claims principal
    /// </summary>
    /// <param name="principal">Claims principal to extract user info from</param>
    /// <returns>The extracted user info, or null when no email claim is present</returns>
    public static UserInfo ToInfo(this ClaimsPrincipal principal)
    {
        if (principal is null)
            return null;

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