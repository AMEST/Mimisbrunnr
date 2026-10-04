using Mimisbrunnr.Integration.Group;
using Mimisbrunnr.Wiki.Contracts;
using Riok.Mapperly.Abstractions;

namespace Mimisbrunnr.Web.Mapping;

/// <summary>
/// Maps group entities to and from their web models
/// </summary>
[Mapper]
public static partial class GroupMapper
{
    /// <summary>
    /// Maps group info to a group model
    /// </summary>
    /// <param name="group">Group info to map</param>
    /// <returns>The mapped group model</returns>
    [MapperIgnoreTarget(nameof(GroupModel.Description))]
    public static partial GroupModel ToModel(this GroupInfo group);

    /// <summary>
    /// Maps a group entity to a group model
    /// </summary>
    /// <param name="group">Group entity to map</param>
    /// <returns>The mapped group model</returns>
    [MapperIgnoreSource(nameof(Users.Group.Id))]
    [MapperIgnoreSource(nameof(Users.Group.OwnerEmails))]
    public static partial GroupModel ToModel(this Users.Group group);

    /// <summary>
    /// Maps a group model to group info
    /// </summary>
    /// <param name="model">Group model to map</param>
    /// <returns>The mapped group info</returns>
    [MapperIgnoreSource(nameof(GroupModel.Description))]
    public static partial GroupInfo ToInfo(this GroupModel model);
}