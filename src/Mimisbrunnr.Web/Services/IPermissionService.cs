using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Services;

public interface IPermissionService
{
    Task EnsureAnonymousAllowed(UserInfo userInfo);

    Task EnsureViewPermission(string spaceKey, UserInfo userInfo);

    Task EnsureEditPermission(string spaceKey, UserInfo userInfo);

    Task EnsureRemovePermission(string spaceKey, UserInfo userInfo);

    Task EnsureAdminPermission(string spaceKey, UserInfo userInfo);

    Task<IEnumerable<Space>> FindUserVisibleSpaces(UserInfo userInfo);
}