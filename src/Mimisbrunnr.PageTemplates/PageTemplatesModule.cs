using Microsoft.Extensions.DependencyInjection;
using Mimisbrunnr.PageTemplates.Services;
using Skidbladnir.Modules;

namespace Mimisbrunnr.PageTemplates;

public class PageTemplatesModule : Module
{
    public override Type[] DependsModules => [];

    public override void Configure(IServiceCollection services)
    {
        services.AddSingleton<IPageTemplateManager, PageTemplateManager>();
    }
}
