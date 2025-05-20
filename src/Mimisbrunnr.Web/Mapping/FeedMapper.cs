using Mimisbrunnr.Web.Feed;
using Mimisbrunnr.Wiki.Contracts;
using Riok.Mapperly.Abstractions;

namespace Mimisbrunnr.Web.Mapping;

[Mapper]
public static partial class FeedMapper
{
    [MapperIgnoreSource(nameof(PageUpdateEvent.Id))]
    [MapperIgnoreSource(nameof(PageUpdateEvent.SpaceType))]
    public static partial PageUpdateEventModel ToModel(this PageUpdateEvent pageUpdateEvent);
}