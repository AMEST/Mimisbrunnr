using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Repositories;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Wiki.Services;

internal class PageManager : IPageManager
{
    private readonly IPageRepository _pageRepository;
    private readonly IHistoricalPageRepository _historicalPageRepository;
    private readonly IAttachmentManager _attachmentManager;
    private readonly IDraftManager _draftManager;
    private readonly ICommentManager _commentManager;
    private readonly IPluginManager _pluginService;

    public PageManager(IPageRepository pageRepository,
        IHistoricalPageRepository historicalPageRepository,
        IAttachmentManager attachmentManager,
        IDraftManager draftManager,
        ICommentManager commentManager,
        IPluginManager pluginService
    )
    {
        _pageRepository = pageRepository;
        _historicalPageRepository = historicalPageRepository;
        _attachmentManager = attachmentManager;
        _draftManager = draftManager;
        _commentManager = commentManager;
        _pluginService = pluginService;
    }

    public Task<Page[]> GetAllOnSpace(Space space)
    {
        return _pageRepository.GetAll().Where(x => x.SpaceId == space.Id).ToArrayAsync();
    }

    public Task<Page[]> FindByName(string name)
    {
        return _pageRepository.GetAll()
            .Where(x => x.Name.Contains(name)).ToArrayAsync();
    }

    public Task<Page[]> GetAllChilds(Page page, bool lightContract = true)
    {
        return GetAllChilds([page.Id], lightContract);
    }

    public Task<Page> GetById(string id)
    {
        return _pageRepository.GetAll().FirstOrDefaultAsync(x => x.Id == id);
    }

    public Task<Page[]> GetByIds(params string[] ids)
    {
        if (ids is null || ids.Length == 0)
            return Task.FromResult(Array.Empty<Page>());
        return _pageRepository.GetByIds(ids);
    }

    public async Task<Page> Create(string spaceId, string name, string content, UserInfo createdBy, string parentPageId = null)
    {
        var page = new Page()
        {
            SpaceId = spaceId,
            Name = name,
            Content = content,
            Created = DateTime.UtcNow,
            Updated = DateTime.UtcNow,
            CreatedBy = createdBy,
            UpdatedBy = createdBy,
            ParentId = parentPageId
        };
        await _pageRepository.Create(page);
        return page;
    }

    public async Task Update(Page page, string name, string content, UserInfo updatedBy)
    {
        ArgumentNullException.ThrowIfNull(page);

        var expectedVersion = page.Version;
        // The snapshot must be taken before the page is changed: history keeps the previous state.
        var previousVersion = HistoricalPage.Create(page);
        // Saved before the page itself: a rejected (conflicting) update leaves only an accurate snapshot behind.
        await _historicalPageRepository.SaveVersion(previousVersion);

        page.Name = name;
        page.Content = content;
        page.Version = expectedVersion + 1;
        page.UpdatedBy = updatedBy;
        page.Updated = DateTime.UtcNow;

        // Fails when somebody else saved the page after it was read, so no update is silently lost.
        await _pageRepository.ReplaceWithVersionCheck(page, expectedVersion);
        await _draftManager.Remove(page.Id);
    }

    public async Task<Page> Copy(Page source, Page destinationParentPage)
    {
        var destinationPage = source.Clone();
        destinationPage.SpaceId = destinationParentPage.SpaceId;
        destinationPage.ParentId = destinationParentPage.Id;
        destinationPage.Updated = DateTime.UtcNow;
        await _pageRepository.Create(destinationPage);
        return destinationPage;
    }

    public async Task<Page> Move(Page source, Page destinationParentPage, bool withChilds = true)
    {
        var originalParentId = source.ParentId;
        var destinationSpaceId = destinationParentPage.SpaceId;

        var pagesToMove = new List<string>() { source.Id };
        if (withChilds)
            pagesToMove.AddRange((await GetAllChilds(source)).Select(x => x.Id));

        await _pageRepository.MoveToSpace([.. pagesToMove], destinationSpaceId);

        if (!withChilds)
            await _pageRepository.ReparentChildren(source.Id, originalParentId);

        await _pageRepository.Reparent(source.Id, destinationParentPage.Id);

        source.SpaceId = destinationSpaceId;
        source.ParentId = destinationParentPage.Id;
        return source;
    }

    public Task Remove(Page page, bool deleteChild = false)
    {
        if (deleteChild)
            return RemoveRecursively(page);
        return RemoveOnlyPage(page);
    }


    public Task<HistoricalPage[]> GetAllVersions(Page page)
    {
        // Version list is metadata only: page content is loaded by GetVersionByPageId.
        return _historicalPageRepository.GetAll()
            .Where(x => x.PageId == page.Id)
            .OrderByDescending(x => x.Version)
            .Select(x => new HistoricalPage
            {
                Id = x.Id,
                PageId = x.PageId,
                Version = x.Version,
                Name = x.Name,
                Updated = x.Updated,
                UpdatedBy = x.UpdatedBy
            })
            .ToArrayAsync();
    }

    public Task<HistoricalPage> GetVersionByPageId(string id, long version)
    {
        return _historicalPageRepository.GetAll().FirstOrDefaultAsync(x => x.PageId == id && x.Version == version);
    }

    public async Task<Page> RestoreVersion(Page page, long version, UserInfo restoredBy)
    {
        var selectedVersion = await GetVersionByPageId(page.Id, version)
            ?? throw new ArgumentOutOfRangeException(nameof(version), $"Page version '{version}' not found");
        await Update(page, selectedVersion.Name, selectedVersion.Content, restoredBy);
        return page;
    }

    public async Task RemoveVersion(Page page, long version)
    {
        var selectedVersion = await GetVersionByPageId(page.Id, version);
        if (selectedVersion is not null)
            await _historicalPageRepository.Delete(selectedVersion);
    }

    private async Task RemoveOnlyPage(Page page)
    {
        await _pageRepository.ReparentChildren(page.Id, page.ParentId);
        await RemovePageData([page.Id]);
        await _pageRepository.Delete(page);
    }

    private async Task RemoveRecursively(Page page)
    {
        var allChilds = await GetAllChilds(page);
        var pageIds = new List<string> { page.Id };
        pageIds.AddRange(allChilds.Select(x => x.Id));
        var ids = pageIds.ToArray();

        await RemovePageData(ids);
        await _pageRepository.DeleteAll(x => ids.Contains(x.Id));
    }

    /// <summary>
    /// Removes every entity that belongs to the pages with one request per collection.
    /// </summary>
    private async Task RemovePageData(string[] pageIds)
    {
        await _attachmentManager.RemoveAllForPages(pageIds);
        await _historicalPageRepository.DeleteAll(x => pageIds.Contains(x.PageId));
        await _draftManager.RemoveForPages(pageIds);
        await _commentManager.RemoveAllForPages(pageIds);
        await _pluginService.DeleteAllStateInPages(pageIds);
    }

    private async Task<Page[]> GetAllChilds(string[] pageIds, bool lightContract = true)
    {
        var allChilds = new List<Page>();
        var visited = new HashSet<string>(pageIds, StringComparer.Ordinal);
        var currentLevel = pageIds;

        // One request per tree level. Visited ids protect from endless recursion on corrupted parent links.
        while (currentLevel.Length > 0)
        {
            var childsQuery = _pageRepository
                .GetAll()
                .Where(x => currentLevel.Contains(x.ParentId));

            if (lightContract)
                childsQuery = childsQuery.Select(x => new Page
                {
                    Id = x.Id,
                    ParentId = x.ParentId,
                    SpaceId = x.SpaceId,
                    Name = x.Name
                });

            var childs = await childsQuery.ToListAsync();
            childs.RemoveAll(x => string.IsNullOrEmpty(x.Id) || !visited.Add(x.Id));
            if (childs.Count == 0)
                break;

            allChilds.AddRange(childs);
            currentLevel = [.. childs.Select(x => x.Id)];
        }

        return [.. allChilds];
    }

}
