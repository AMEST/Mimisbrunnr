using System.Threading.Tasks;
using Mimisbrunnr.Wiki.Contracts;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Wiki.Services;

internal class PageManager : IPageManager, IPageSearcher
{
    private readonly IRepository<Page> _pageRepository;
    private readonly IAttachmentManager _attachmentManager;

    public PageManager(IRepository<Page> pageRepository, IAttachmentManager attachmentManager)
    {
        _pageRepository = pageRepository;
        _attachmentManager = attachmentManager;
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

    public async Task<Page[]> GetAllChilds(Page page)
    {
        var flatChildsList = new List<Page>();
        var childs = await _pageRepository
            .GetAll()
            .Where(x => x.ParentId == page.Id)
            .Select(x => new Page{
                Id = x.Id,
                ParentId = x.ParentId,
                SpaceId = x.SpaceId,
                Name = x.Name
            })
            .ToListAsync();

        flatChildsList.AddRange(childs);
        
        var getChildTasks = new List<Task<Page[]>>();
        foreach (var child in childs)
            getChildTasks.Add(GetAllChilds(child));

        await Task.WhenAll(getChildTasks);

        foreach(var innerChildsTask in getChildTasks)
        {
            var innerChilds = innerChildsTask.Result;
            if(innerChilds != null && innerChilds.Length > 0)
                flatChildsList.AddRange(innerChilds);
        }

        return flatChildsList.ToArray();
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

    public Task Update(Page page, UserInfo updatedBy)
    {
        page.UpdatedBy = updatedBy;
        page.Updated = DateTime.UtcNow;
        return _pageRepository.Update(page);
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
    
    public async Task<Page> Move(Page source, Page destinationParentPage)
    {
        source.SpaceId = destinationParentPage.SpaceId;
        source.ParentId = destinationParentPage.Id;
        await _pageRepository.Update(source);
        return source;
    }

    public Task Remove(Page page, bool deleteChild = false)
    {
        if (deleteChild)
            return RemoveRecuesively(page);
        return RemoveOnlyPage(page);
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
        await _pageRepository.Delete(page);
    }

    private async Task RemoveRecuesively(Page page)
    {
        var allChilds = await GetAllChilds(page);
        foreach (var child in allChilds)
        {
            await RemoveAttachments(child);
            await _pageRepository.Delete(child);
        }

        await RemoveAttachments(page);
        await _pageRepository.Delete(page);
    }

    private async Task RemoveAttachments(Page page)
    {
        var attachments = await _attachmentManager.GetAttachments(page);
        var removeTasks = new List<Task>();
        foreach(var attachment in attachments)
            removeTasks.Add(_attachmentManager.Remove(page, attachment.Name));

        await Task.WhenAll(removeTasks);
    }

    public async Task<IEnumerable<Page>> Search(string text)
    {
        var pages = await _pageRepository.GetAll()
            .Where(x => x.Name.Contains(text)
                || x.Content.Contains(text))
            .Take(100)
            .ToArrayAsync();
        return pages;
    }
}