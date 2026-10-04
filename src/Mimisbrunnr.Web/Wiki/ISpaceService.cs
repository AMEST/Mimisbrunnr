using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Integration.Wiki;

namespace Mimisbrunnr.Web.Wiki;

/// <summary>
/// Service for managing wiki spaces
/// </summary>
public interface ISpaceService
{
    /// <summary>
    /// Get all spaces available to the requesting user
    /// </summary>
    /// <param name="requestedBy">User requesting the spaces</param>
    /// <param name="take">Maximum number of spaces to return</param>
    /// <param name="skip">Number of spaces to skip</param>
    /// <returns>List of spaces</returns>
    Task<SpaceModel[]> GetAll(UserInfo requestedBy, int? take = null, int? skip = null);

    /// <summary>
    /// Get a space by its key
    /// </summary>
    /// <param name="key">Space key</param>
    /// <param name="requestedBy">User requesting the space</param>
    /// <returns>The requested space</returns>
    Task<SpaceModel> GetByKey(string key, UserInfo requestedBy);

    /// <summary>
    /// Get the permissions of the requesting user for a space
    /// </summary>
    /// <param name="key">Space key</param>
    /// <param name="requestedBy">User requesting the permissions</param>
    /// <returns>User permissions for the space</returns>
    Task<UserPermissionModel> GetPermission(string key, UserInfo requestedBy);

    /// <summary>
    /// Get all permissions configured for a space
    /// </summary>
    /// <param name="key">Space key</param>
    /// <param name="requestedBy">User requesting the permissions</param>
    /// <returns>List of space permissions</returns>
    Task<SpacePermissionModel[]> GetSpacePermissions(string key, UserInfo requestedBy);

    /// <summary>
    /// Add a permission to a space
    /// </summary>
    /// <param name="key">Space key</param>
    /// <param name="model">Permission to add</param>
    /// <param name="addedBy">User adding the permission</param>
    /// <returns>The added permission</returns>
    Task<SpacePermissionModel> AddPermission(string key, SpacePermissionModel model, UserInfo addedBy);
    
    /// <summary>
    /// Update a permission of a space
    /// </summary>
    /// <param name="key">Space key</param>
    /// <param name="model">Updated permission</param>
    /// <param name="updatedBy">User updating the permission</param>
    Task UpdatePermission(string key, SpacePermissionModel model, UserInfo updatedBy);

    /// <summary>
    /// Remove a permission from a space
    /// </summary>
    /// <param name="key">Space key</param>
    /// <param name="model">Permission to remove</param>
    /// <param name="removedBy">User removing the permission</param>
    Task RemovePermission(string key,SpacePermissionModel model, UserInfo removedBy);

    /// <summary>
    /// Create a new space
    /// </summary>
    /// <param name="model">Space creation parameters</param>
    /// <param name="createdBy">User creating the space</param>
    /// <returns>The created space</returns>
    Task<SpaceModel> Create(SpaceCreateModel model, UserInfo createdBy);

    /// <summary>
    /// Update an existing space
    /// </summary>
    /// <param name="key">Space key</param>
    /// <param name="model">Space update parameters</param>
    /// <param name="updatedBy">User updating the space</param>
    Task Update(string key, SpaceUpdateModel model, UserInfo updatedBy);

    /// <summary>
    /// Archive a space
    /// </summary>
    /// <param name="key">Space key</param>
    /// <param name="archivedBy">User archiving the space</param>
    Task Archive(string key, UserInfo archivedBy);

    /// <summary>
    /// Unarchive a space
    /// </summary>
    /// <param name="key">Space key</param>
    /// <param name="unArchivedBy">User unarchiving the space</param>
    Task UnArchive(string key, UserInfo unArchivedBy);

    /// <summary>
    /// Remove a space
    /// </summary>
    /// <param name="key">Space key</param>
    /// <param name="removedBy">User removing the space</param>
    Task Remove(string key, UserInfo removedBy);
}