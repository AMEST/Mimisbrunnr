using Mimisbrunnr.Web.Feed;
using Mimisbrunnr.Wiki.Contracts;
using Riok.Mapperly.Abstractions;

namespace Mimisbrunnr.Web.Mapping;

/// <summary>
/// Maps page update event entities to their web models
/// </summary>
[Mapper]
public static partial class FeedMapper
{
    /// <summary>
    /// Maps a page update event entity to a page update event model
    /// </summary>
    /// <param name="pageUpdateEvent">Page update event entity to map</param>
    /// <returns>The mapped page update event model</returns>
    [MapperIgnoreSource(nameof(PageUpdateEvent.Id))]
    [MapperIgnoreSource(nameof(PageUpdateEvent.SpaceType))]
    public static partial PageUpdateEventModel ToModel(this PageUpdateEvent pageUpdateEvent);
}