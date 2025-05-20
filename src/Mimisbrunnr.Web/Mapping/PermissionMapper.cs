using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Wiki.Contracts;
using Riok.Mapperly.Abstractions;

namespace Mimisbrunnr.Web.Mapping;

[Mapper]
public static partial class PermissionMapper
{
#pragma warning disable RMG012 // Source member was not found for target member
    public static partial SpacePermissionModel ToSpacePermissions(this Permission permission);
#pragma warning restore RMG012 // Source member was not found for target member

    [MapperIgnoreSource(nameof(Permission.Group))]
    [MapperIgnoreSource(nameof(Permission.User))]
    public static partial UserPermissionModel ToUserPermissions(this Permission permission);

#pragma warning disable RMG020 // Source member is not mapped to any target member
    public static partial Permission ToEntity(this SpacePermissionModel model);
#pragma warning restore RMG020 // Source member is not mapped to any target member
}