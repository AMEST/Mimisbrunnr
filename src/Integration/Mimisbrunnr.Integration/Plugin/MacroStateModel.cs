using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Integration.Plugin;

public class MacroStateModel
{
    public string Id { get; set; }
    [Required]
    [MaxLength(255)]
    public string MacroIdentifier { get; set; }
    [Required]
    public string PageId { get; set; }
    [Required]
    public string MacroIdentifierOnPage { get; set; }
    public IDictionary<string, string> Params { get; set; } = new Dictionary<string, string>();
}