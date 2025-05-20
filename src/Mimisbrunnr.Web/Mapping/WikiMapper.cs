using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Web.Wiki;
using Mimisbrunnr.Wiki.Contracts;
using Riok.Mapperly.Abstractions;

namespace Mimisbrunnr.Web.Mapping;

[Mapper]
public static partial class WikiMapper
{
    [MapperIgnoreSource(nameof(Space.Id))]
    [MapperIgnoreSource(nameof(Space.PermissionsFlat))]
    [MapperIgnoreSource(nameof(Space.Permissions))]
    public static partial SpaceModel ToModel(this Space space);

    [MapperIgnoreSource(nameof(Draft.Id))]
    [MapperIgnoreSource(nameof(Draft.OriginalPageId))]
    public static partial DraftModel ToModel(this Draft draft);

    [MapperIgnoreSource(nameof(Attachment.Path))]
    [MapperIgnoreSource(nameof(Attachment.PageId))]
    [MapperIgnoreSource(nameof(Attachment.Id))]
    public static partial AttachmentModel ToModel(this Attachment attachment);

    [MapperIgnoreSource(nameof(Comment.PageId))]
    public static partial CommentModel ToModel(this Comment comment);

    [MapperIgnoreTarget(nameof(HistoricalPageModel.Created))]
    public static partial HistoricalPageModel ToModel(this HistoricalPage historicalPage);

    [MapperIgnoreSource(nameof(page.SpaceId))]
    [MapperIgnoreSource(nameof(page.ParentId))]
    public static partial PageModel ToModelAuto(this Page page, string spaceKey = null);

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