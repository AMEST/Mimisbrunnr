namespace Mimisbrunnr.PageTemplates.Contracts;

public class PageTemplateNotFoundException : Exception
{
    public PageTemplateNotFoundException(string message = "Page template not found") : base(message) { }
}
