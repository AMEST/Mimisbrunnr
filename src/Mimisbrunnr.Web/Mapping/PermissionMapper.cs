using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Wiki.Contracts;
using Riok.Mapperly.Abstractions;

namespace Mimisbrunnr.Web.Mapping;

[Mapper]
public partial class PermissionMapper
{

    public partial SpacePermissionModel ToSpacePermissions(Permission permission);

    public partial UserPermissionModel ToUserPermissions(Permission permission);

    public partial Permission ToEntity(SpacePermissionModel model);
}