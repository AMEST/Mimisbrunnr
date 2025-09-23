using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Wiki.Contracts;

public class MacroState : IHasId<string>
{
    public string Id { get; set; }
    public string MacroIdentifier { get; set; }
    public string PageId { get; set; }
    public string MacroIdentifierOnPage { get; set; }
    public IDictionary<string, string> Params { get; set; } = new Dictionary<string, string>();
}