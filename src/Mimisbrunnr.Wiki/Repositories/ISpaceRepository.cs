using Mimisbrunnr.Wiki.Contracts;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Wiki.Repositories;

/// <summary>
/// Space storage operations that cannot be expressed with <see cref="IRepository{TEntity}"/> without loosing efficiency
/// </summary>
public interface ISpaceRepository : IRepository<Space>
{
    /// <summary>
    /// Removes the permission of the group from every space in a single request
    /// </summary>
    /// <returns>Spaces that were affected (identifiers and keys only)</returns>
    Task<Space[]> RemoveGroupPermissions(string groupName);
}
