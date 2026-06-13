using Mimisbrunnr.Integration.PageTemplates;
using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.PageTemplates;

public interface IPageTemplateService
{
    Task<PageTemplateModel[]> GetAll(string type, string spaceKey, UserInfo user);
    Task<PageTemplateModel> GetById(string id, UserInfo user);
    Task<PageTemplateModel> Create(PageTemplateCreateModel model, UserInfo user);
    Task Update(string id, PageTemplateUpdateModel model, UserInfo user);
    Task Delete(string id, UserInfo user);
    Task<PageTemplateRenderResponse> Render(string templateId, string spaceKey, UserInfo user);
}
