using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Web.Wiki;
using Mimisbrunnr.Wiki.Contracts;
using Riok.Mapperly.Abstractions;

namespace Mimisbrunnr.Web.Mapping;

/// <summary>
/// Maps wiki entities to and from their web models
/// </summary>
[Mapper]
public static partial class WikiMapper
{
    /// <summary>
    /// Maps a space entity to a space model
    /// </summary>
    /// <param name="space">Space to map</param>
    /// <returns>The mapped space model</returns>
    [MapperIgnoreSource(nameof(Space.Id))]
    [MapperIgnoreSource(nameof(Space.PermissionsFlat))]
    [MapperIgnoreSource(nameof(Space.Permissions))]
    public static partial SpaceModel ToModel(this Space space);

    /// <summary>
    /// Maps a draft entity to a draft model
    /// </summary>
    /// <param name="draft">Draft to map</param>
    /// <returns>The mapped draft model</returns>
    [MapperIgnoreSource(nameof(Draft.Id))]
    [MapperIgnoreSource(nameof(Draft.OriginalPageId))]
    public static partial DraftModel ToModel(this Draft draft);

    /// <summary>
    /// Maps an attachment entity to an attachment model
    /// </summary>
    /// <param name="attachment">Attachment to map</param>
    /// <returns>The mapped attachment model</returns>
    [MapperIgnoreSource(nameof(Attachment.Path))]
    [MapperIgnoreSource(nameof(Attachment.PageId))]
    [MapperIgnoreSource(nameof(Attachment.Id))]
    public static partial AttachmentModel ToModel(this Attachment attachment);

    /// <summary>
    /// Maps a comment entity to a comment model
    /// </summary>
    /// <param name="comment">Comment to map</param>
    /// <returns>The mapped comment model</returns>
    [MapperIgnoreSource(nameof(Comment.PageId))]
    public static partial CommentModel ToModel(this Comment comment);

    /// <summary>
    /// Maps a historical page entity to a historical page model
    /// </summary>
    /// <param name="historicalPage">Historical page to map</param>
    /// <returns>The mapped historical page model</returns>
    [MapperIgnoreTarget(nameof(HistoricalPageModel.Created))]
    public static partial HistoricalPageModel ToModel(this HistoricalPage historicalPage);

    /// <summary>
    /// Maps a page entity to a page model using the specified space key
    /// </summary>
    /// <param name="page">Page to map</param>
    /// <param name="spaceKey">Key of the space containing the page</param>
    /// <returns>The mapped page model</returns>
    [MapperIgnoreSource(nameof(page.SpaceId))]
    [MapperIgnoreSource(nameof(page.ParentId))]
    public static partial PageModel ToModelAuto(this Page page, string spaceKey = null);

    /// <summary>
    /// Maps a page entity to a page model using the specified space key
    /// </summary>
    /// <param name="page">Page to map</param>
    /// <param name="spaceKey">Key of the space containing the page</param>
    /// <returns>The mapped page model</returns>
    public static PageModel ToModel(this Page page, string spaceKey = null)
    {
        return new PageModel()
        {
            Id = page.Id,
            SpaceKey = spaceKey,
            Name = page.Name,
            Content = page.Content,
            Created = page.Created,
            CreatedBy = page.CreatedBy.ToModel(),
            Updated = page.Updated,
            UpdatedBy = page.CreatedBy.ToModel()
        };
    }

    /// <summary>
    /// Maps a collection of child pages into a page tree model
    /// </summary>
    /// <param name="childs">Child pages to map</param>
    /// <param name="rootPage">Root page of the tree</param>
    /// <param name="space">Space containing the pages</param>
    /// <returns>The mapped page tree model</returns>
    public static PageTreeModel ToModel(this IEnumerable<Page> childs, Page rootPage, Space space = null)
    {
        var pageTree = new PageTreeModel
        {
            Page = ToModel(rootPage, space.Key)
        };
        var childsPages = childs.Where(x => x.ParentId == rootPage?.Id).Select(x => ToModel(childs, x, space)).ToList();
        pageTree.Childs = childsPages;
        return pageTree;
    }
}