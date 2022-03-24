using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Web.Infrastructure.Contracts;

public class ApplicationConfiguration : IHasId<string>
{
    public string Id { get; set; }
    
    public string Title { get; set; } = "Mimisbrunnr wiki";

    public bool AllowAnonymous { get; set; } = true;

    public bool SwaggerEnabled { get; set; } = true;
}