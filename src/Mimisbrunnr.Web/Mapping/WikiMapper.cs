using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Web.Wiki;
using Mimisbrunnr.Wiki.Contracts;
using Riok.Mapperly.Abstractions;

namespace Mimisbrunnr.Web.Mapping;

[Mapper]
public static partial class WikiMapper
{
    public static partial SpaceModel ToModel(this Space space);

    public static partial DraftModel ToModel(this Draft draft);

    public static partial AttachmentModel ToModel(this Attachment attachment);

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