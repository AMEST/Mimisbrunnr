using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Web.Infrastructure.Contracts;

public class ApplicationConfiguration : IHasId<string>
{
    public string Id { get; set; }

    public string Title { get; set; } = "Mimisbrunnr wiki";

    public bool AllowAnonymous { get; set; }

    public bool UserAutoCreation { get; set; }

    public bool SwaggerEnabled { get; set; }

    public bool AllowHtml { get; set; }

    public string CustomCss { get; set; }

    public bool CustomHomepageEnabled { get; set; }

    public string CustomHomepageSpaceKey { get; set; }
}