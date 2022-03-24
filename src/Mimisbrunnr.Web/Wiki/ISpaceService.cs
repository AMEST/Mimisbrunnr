using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Wiki;

public interface ISpaceService
{
    Task<SpaceModel[]> GetAll(UserInfo requestedBy);

    Task<SpaceModel> GetByKey(string key, UserInfo requestedBy);

    Task<UserPermissionModel> GetPermission(string key, UserInfo requestedBy);

    Task<SpacePermissionModel[]> GetSpacePermissions(string key, UserInfo requestedBy);

    Task<SpaceModel> Create(SpaceCreateModel model, UserInfo createdBy);

    Task Update(string key, SpaceUpdateModel model, UserInfo updatedBy);

    Task Archive(string key, UserInfo archivedBy);

    Task UnArchive(string key, UserInfo unArchivedBy);

    Task Remove(string key, UserInfo removedBy);
}