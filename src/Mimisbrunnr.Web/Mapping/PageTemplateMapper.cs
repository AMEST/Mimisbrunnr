using Mimisbrunnr.Integration.PageTemplates;
using Mimisbrunnr.PageTemplates.Contracts;
using Riok.Mapperly.Abstractions;

namespace Mimisbrunnr.Web.Mapping;

/// <summary>
/// Maps page template entities to their web models
/// </summary>
[Mapper]
public static partial class PageTemplateMapper
{
    /// <summary>
    /// Maps a page template entity to a page template model
    /// </summary>
    /// <param name="template">Page template entity to map</param>
    /// <returns>The mapped page template model</returns>
    public static partial PageTemplateModel ToModel(this PageTemplate template);
}
