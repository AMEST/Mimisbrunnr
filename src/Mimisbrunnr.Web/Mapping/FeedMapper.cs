using Mimisbrunnr.Web.Feed;
using Mimisbrunnr.Wiki.Contracts;
using Riok.Mapperly.Abstractions;

namespace Mimisbrunnr.Web.Mapping;

[Mapper]
public partial class FeedMapper
{
    public partial PageUpdateEventModel ToModel(PageUpdateEvent pageUpdateEvent);
}