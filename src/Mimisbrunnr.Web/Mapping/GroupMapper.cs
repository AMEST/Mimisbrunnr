using Mimisbrunnr.Integration.Group;
using Mimisbrunnr.Wiki.Contracts;
using Riok.Mapperly.Abstractions;

namespace Mimisbrunnr.Web.Mapping;

[Mapper]
public static partial class GroupMapper
{
    [MapperIgnoreTarget(nameof(GroupModel.Description))]
    public static partial GroupModel ToModel(this GroupInfo group);

    [MapperIgnoreSource(nameof(Users.Group.Id))]
    [MapperIgnoreSource(nameof(Users.Group.OwnerEmails))]
    public static partial GroupModel ToModel(this Users.Group group);

    [MapperIgnoreSource(nameof(GroupModel.Description))]
    public static partial GroupInfo ToInfo(this GroupModel model);
}