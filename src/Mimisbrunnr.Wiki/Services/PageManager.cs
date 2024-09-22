using Mimisbrunnr.Wiki.Contracts;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Wiki.Services;

internal class PageManager : IPageManager
{
    private readonly IRepository<Page> _pageRepository;
    private readonly IRepository<HistoricalPage> _historicalPageRepository;
    private readonly IAttachmentManager _attachmentManager;
    private readonly IDraftManager _draftManager;
    private readonly ICommentManager _commentManager;

    public PageManager(IRepository<Page> pageRepository,
        IRepository<HistoricalPage> historicalPageRepository,
        IAttachmentManager attachmentManager,
        IDraftManager draftManager,
        ICommentManager commentManager
    )
    {
        _pageRepository = pageRepository;
        _historicalPageRepository = historicalPageRepository;
        _attachmentManager = attachmentManager;
        _draftManager = draftManager;
        _commentManager = commentManager;
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

    public async Task Update(Page page, UserInfo updatedBy)
    {
        await SaveCurrentPageVersion(page.Id);
        page.Version++;
        page.UpdatedBy = updatedBy;
        page.Updated = DateTime.UtcNow;
        await _pageRepository.Update(page);
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

        source.SpaceId = destinationParentPage.SpaceId;
        source.ParentId = destinationParentPage.Id;
        await _pageRepository.Update(source);

        if (withChilds)
        {
            var childs = await GetAllChilds(source, lightContract: false);
            foreach (var child in childs)
            {
                child.SpaceId = destinationParentPage.SpaceId;
                await _pageRepository.Update(child);
            }
            return source;
        }

        var firstLevelChilds = await _pageRepository.GetAll()
            .Where(x => x.ParentId == source.Id)
            .ToArrayAsync();
        foreach (var child in firstLevelChilds)
        {
            child.ParentId = originalParentId;
            await _pageRepository.Update(child);
        }
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
        return _historicalPageRepository.GetAll().Where(x => x.PageId == page.Id).ToArrayAsync();
    }

    public Task<HistoricalPage> GetVersionByPageId(string id, long version)
    {
        return _historicalPageRepository.GetAll().FirstOrDefaultAsync(x => x.PageId == id && x.Version == version);
    }

    public async Task<Page> RestoreVersion(Page page, long version, UserInfo restoredBy)
    {
        var selectedVersion = await GetVersionByPageId(page.Id, version);
        page.Name = selectedVersion.Name;
        page.Content = selectedVersion.Content;
        await Update(page, restoredBy);
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
        var childs = await _pageRepository.GetAll().Where(x => x.ParentId == page.Id).ToListAsync();
        foreach (var child in childs)
        {
            child.ParentId = page.ParentId;
            await _pageRepository.Update(child);
        }

        await RemoveAttachments(page);
        await RemoveAllPageVersions(page);
        await _draftManager.Remove(page.Id);
        await _commentManager.RemoveAll(page);
        await _pageRepository.Delete(page);
    }

    private async Task RemoveRecursively(Page page)
    {
        var allChilds = await GetAllChilds(page);
        foreach (var child in allChilds)
        {
            await RemoveAttachments(child);
            await _draftManager.Remove(child.Id);
            await _commentManager.RemoveAll(page);
            await _pageRepository.Delete(child);
            await RemoveAllPageVersions(child);
        }

        await RemoveAttachments(page);
        await RemoveAllPageVersions(page);
        await _draftManager.Remove(page.Id);
        await _commentManager.RemoveAll(page);
        await _pageRepository.Delete(page);
    }

    private async Task RemoveAttachments(Page page)
    {
        var attachments = await _attachmentManager.GetAttachments(page);
        var removeTasks = new List<Task>();
        foreach (var attachment in attachments)
            removeTasks.Add(_attachmentManager.Remove(page, attachment.Name));

        await Task.WhenAll(removeTasks);
    }

    private async Task SaveCurrentPageVersion(string id)
    {
        var currentPage = await GetById(id);
        await _historicalPageRepository.Create(HistoricalPage.Create(currentPage));
    }

    private async Task RemoveAllPageVersions(Page page)
    {
        var allVersions = await GetAllVersions(page);
        foreach (var version in allVersions)
            await _historicalPageRepository.Delete(version);
    }
    
    private async Task<Page[]> GetAllChilds(string[] pageIds, bool lightContract = true)
    {
        var childsQuery = _pageRepository
            .GetAll()
            .Where(x => pageIds.Contains(x.ParentId));
        if (lightContract)
            childsQuery = childsQuery.Select(x => new Page
            {
                Id = x.Id,
                ParentId = x.ParentId,
                SpaceId = x.SpaceId,
                Name = x.Name
            });

        var childs = await childsQuery.ToListAsync();
        if (childs.Count == 0)
            return Array.Empty<Page>();
        
        var innerChilds = await GetAllChilds(childs.Select(x => x.Id).ToArray(), lightContract);
        if (innerChilds.Length > 0)
            childs.AddRange(innerChilds);

        return childs.ToArray();
    }

}