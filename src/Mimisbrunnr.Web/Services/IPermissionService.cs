using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Services;

/// <summary>
/// Service for enforcing access permissions
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Ensures that anonymous access is allowed for the specified user
    /// </summary>
    /// <param name="userInfo">User requesting access</param>
    Task EnsureAnonymousAllowed(UserInfo userInfo);

    /// <summary>
    /// Ensures the user has permission to view the specified space
    /// </summary>
    /// <param name="spaceKey">Key of the space being accessed</param>
    /// <param name="userInfo">User requesting access</param>
    Task EnsureViewPermission(string spaceKey, UserInfo userInfo);

    /// <summary>
    /// Ensures the user has permission to edit the specified space
    /// </summary>
    /// <param name="spaceKey">Key of the space being edited</param>
    /// <param name="userInfo">User requesting access</param>
    Task EnsureEditPermission(string spaceKey, UserInfo userInfo);

    /// <summary>
    /// Ensures the user has permission to remove the specified space
    /// </summary>
    /// <param name="spaceKey">Key of the space being removed</param>
    /// <param name="userInfo">User requesting access</param>
    Task EnsureRemovePermission(string spaceKey, UserInfo userInfo);

    /// <summary>
    /// Ensures the user has administrative permission for the specified space
    /// </summary>
    /// <param name="spaceKey">Key of the space being administered</param>
    /// <param name="userInfo">User requesting access</param>
    Task EnsureAdminPermission(string spaceKey, UserInfo userInfo);
}