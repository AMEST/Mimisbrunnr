using Mimisbrunnr.Integration.Group;
using Mimisbrunnr.Wiki.Contracts;
using Riok.Mapperly.Abstractions;

namespace Mimisbrunnr.Web.Mapping;

[Mapper]
public static partial class GroupMapper
{
    public static partial GroupModel ToModel(this GroupInfo group);

    public static partial GroupModel ToModel(this Users.Group group);

    public static partial GroupInfo ToInfo(this GroupModel model);
}