using Mimisbrunnr.PageTemplates.Contracts;
using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.PageTemplates.Services;

public interface IPageTemplateManager
{
    Task<PageTemplate> GetById(string id);
    IQueryable<PageTemplate> GetAll();
    Task<PageTemplate> Create(PageTemplate template);
    Task Update(string id, string name, string description, string content, UserInfo updatedBy);
    Task Delete(string id);
}
