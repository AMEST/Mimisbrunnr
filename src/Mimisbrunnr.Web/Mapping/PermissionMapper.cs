using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Wiki.Contracts;
using Riok.Mapperly.Abstractions;

namespace Mimisbrunnr.Web.Mapping;

/// <summary>
/// Maps permission entities to and from their web models
/// </summary>
[Mapper]
public static partial class PermissionMapper
{
#pragma warning disable RMG012 // Source member was not found for target member
    /// <summary>
    /// Maps a permission to a space permission model
    /// </summary>
    /// <param name="permission">Permission to map</param>
    /// <returns>The mapped space permission model</returns>
    public static partial SpacePermissionModel ToSpacePermissions(this Permission permission);
#pragma warning restore RMG012 // Source member was not found for target member

    /// <summary>
    /// Maps a permission to a user permission model
    /// </summary>
    /// <param name="permission">Permission to map</param>
    /// <returns>The mapped user permission model</returns>
    [MapperIgnoreSource(nameof(Permission.Group))]
    [MapperIgnoreSource(nameof(Permission.User))]
    public static partial UserPermissionModel ToUserPermissions(this Permission permission);

#pragma warning disable RMG020 // Source member is not mapped to any target member
    /// <summary>
    /// Maps a space permission model to a permission entity
    /// </summary>
    /// <param name="model">Space permission model to map</param>
    /// <returns>The mapped permission entity</returns>
    public static partial Permission ToEntity(this SpacePermissionModel model);
#pragma warning restore RMG020 // Source member is not mapped to any target member
}