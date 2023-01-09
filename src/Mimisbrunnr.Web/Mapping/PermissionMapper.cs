using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Wiki.Contracts;
using Riok.Mapperly.Abstractions;

namespace Mimisbrunnr.Web.Mapping;

[Mapper]
public static partial class PermissionMapper
{

    public static partial SpacePermissionModel ToSpacePermissions(this Permission permission);

    public static partial UserPermissionModel ToUserPermissions(this Permission permission);

    public static partial Permission ToEntity(this SpacePermissionModel model);
}