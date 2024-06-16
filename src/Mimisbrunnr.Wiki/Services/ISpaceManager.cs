using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Wiki.Services;

public interface ISpaceManager
{
    Task<Space[]> GetAll(
        int? take = null,
        int? skip = null);

    Task<Space[]> GetAllWithPermissions(UserInfo user = null,
        string[] userGroups = null,
        int? take = null,
        int? skip = null);

    Task<Space[]> GetPublicSpaces(int? take = null, int? skip = null);

    Task<Space> GetById(string id);

    Task<Space> GetByKey(string key);

    Task<Space> FindPersonalSpace(UserInfo user);

    Task<Space[]> FindByName(string name);

    Task<Space> Create(string key, string name, string description, SpaceType type, UserInfo owner);

    Task Update(Space space);

    Task AddPermission(Space space, Permission permission);

    Task UpdatePermission(Space space, Permission permission);

    Task RemovePermission(Space space, Permission permission);

    Task Archive(Space space);

    Task UnArchive(Space space);

    Task Remove(Space space);
}