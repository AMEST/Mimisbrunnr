using Mimisbrunnr.PageTemplates.Contracts;
using Mimisbrunnr.Wiki.Contracts;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.PageTemplates.Services;

public class PageTemplateManager : IPageTemplateManager
{
    private readonly IRepository<PageTemplate> _repository;

    public PageTemplateManager(IRepository<PageTemplate> repository)
    {
        _repository = repository;
    }

    public async Task<PageTemplate> GetById(string id)
    {
        var template = await _repository.GetAll().FirstOrDefaultAsync(x => x.Id == id);
        if (template == null)
            throw new PageTemplateNotFoundException($"Page template with id '{id}' not found");
        return template;
    }

    public IQueryable<PageTemplate> GetAll() => _repository.GetAll();

    public async Task<PageTemplate> Create(PageTemplate template)
    {
        var now = DateTime.UtcNow;
        template.Id = Guid.NewGuid().ToString();
        template.Created = now;
        template.Updated = now;
        template.CreatedBy = new UserInfo { Email = template.OwnerEmail };
        template.UpdatedBy = new UserInfo { Email = template.OwnerEmail };
        await _repository.Create(template);
        return template;
    }

    public async Task Update(string id, string name, string content, UserInfo updatedBy)
    {
        var template = await GetById(id);
        template.Name = name;
        template.Content = content;
        template.Updated = DateTime.UtcNow;
        template.UpdatedBy = updatedBy;
        await _repository.Update(template);
    }

    public async Task Delete(string id)
    {
        var template = await GetById(id);
        await _repository.Delete(template);
    }
}
