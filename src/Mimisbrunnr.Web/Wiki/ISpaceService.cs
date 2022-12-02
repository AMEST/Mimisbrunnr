using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Integration.Wiki;

namespace Mimisbrunnr.Web.Wiki;

public interface ISpaceService
{
    Task<SpaceModel[]> GetAll(UserInfo requestedBy);

    Task<SpaceModel> GetByKey(string key, UserInfo requestedBy);

    Task<UserPermissionModel> GetPermission(string key, UserInfo requestedBy);

    Task<SpacePermissionModel[]> GetSpacePermissions(string key, UserInfo requestedBy);

    Task<SpacePermissionModel> AddPermission(string key, SpacePermissionModel model, UserInfo addedBy);
    
    Task UpdatePermission(string key, SpacePermissionModel model, UserInfo updatedBy);

    Task RemovePermission(string key,SpacePermissionModel model, UserInfo removedBy);

    Task<SpaceModel> Create(SpaceCreateModel model, UserInfo createdBy);

    Task Update(string key, SpaceUpdateModel model, UserInfo updatedBy);

    Task Archive(string key, UserInfo archivedBy);

    Task UnArchive(string key, UserInfo unArchivedBy);

    Task Remove(string key, UserInfo removedBy);
}