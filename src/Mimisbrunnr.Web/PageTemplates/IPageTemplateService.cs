using Mimisbrunnr.Integration.PageTemplates;
using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.PageTemplates;

/// <summary>
/// Provides functionality for managing page templates
/// </summary>
public interface IPageTemplateService
{
    /// <summary>
    /// Retrieves page templates filtered by type and space
    /// </summary>
    /// <param name="type">The template type filter</param>
    /// <param name="spaceKey">The key of the space to filter templates by</param>
    /// <param name="user">User information of the user requesting the templates</param>
    /// <returns>Array of page template models</returns>
    Task<PageTemplateModel[]> GetAll(string type, string spaceKey, UserInfo user);

    /// <summary>
    /// Retrieves a page template by its unique identifier
    /// </summary>
    /// <param name="id">Unique identifier of the page template</param>
    /// <param name="user">User information of the user requesting the template</param>
    /// <returns>Page template model</returns>
    Task<PageTemplateModel> GetById(string id, UserInfo user);

    /// <summary>
    /// Creates a new page template
    /// </summary>
    /// <param name="model">Model containing the page template details</param>
    /// <param name="user">User information of the user creating the template</param>
    /// <returns>The created page template model</returns>
    Task<PageTemplateModel> Create(PageTemplateCreateModel model, UserInfo user);

    /// <summary>
    /// Updates an existing page template
    /// </summary>
    /// <param name="id">Unique identifier of the page template to update</param>
    /// <param name="model">Model containing the updated page template details</param>
    /// <param name="user">User information of the user performing the update</param>
    Task Update(string id, PageTemplateUpdateModel model, UserInfo user);

    /// <summary>
    /// Deletes a page template
    /// </summary>
    /// <param name="id">Unique identifier of the page template to delete</param>
    /// <param name="user">User information of the user performing the deletion</param>
    Task Delete(string id, UserInfo user);

    /// <summary>
    /// Renders a page template in the context of a space
    /// </summary>
    /// <param name="templateId">Unique identifier of the page template to render</param>
    /// <param name="spaceKey">The key of the space used as rendering context</param>
    /// <param name="user">User information of the user requesting the render</param>
    /// <returns>Response containing the rendered page template content</returns>
    Task<PageTemplateRenderResponse> Render(string templateId, string spaceKey, UserInfo user);
}
