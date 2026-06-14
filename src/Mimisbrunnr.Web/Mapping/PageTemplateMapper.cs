using Mimisbrunnr.Integration.PageTemplates;
using Mimisbrunnr.PageTemplates.Contracts;
using Riok.Mapperly.Abstractions;

namespace Mimisbrunnr.Web.Mapping;

[Mapper]
public static partial class PageTemplateMapper
{
    public static partial PageTemplateModel ToModel(this PageTemplate template);
}
