using Mimisbrunnr.Integration.Group;
using Mimisbrunnr.Wiki.Contracts;
using Riok.Mapperly.Abstractions;

namespace Mimisbrunnr.Web.Mapping;

[Mapper]
public partial class GroupMapper
{
    public static GroupMapper Instance { get; } = new GroupMapper();

    public partial GroupModel ToModel(GroupInfo group);

    public partial GroupModel ToModel(Users.Group group);

    public partial GroupInfo ToInfo(GroupModel model);
}