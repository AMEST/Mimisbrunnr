namespace Mimisbrunnr.Web.Infrastructure;

public interface ITemplateRenderer
{
     Task<string> Render(string template, IDictionary<string, object> parameters);
}