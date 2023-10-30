using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Integration.Wiki;

public class PageCreateModel : IValidatableObject
{
    [Required]
    public string SpaceKey { get; set; }
    
    [Required]
    public string ParentPageId { get; set; }

    [Required]
    public string Name { get; set; }

    public string Content { get; set; }

    public string PlainTextContent {get;set;}

    public PageEditorTypeModel EditorType { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if(!string.IsNullOrEmpty(Content) 
            && Content.Length > 10 // additional check if Content contains only MD/editorjs tokens
            && string.IsNullOrEmpty(PlainTextContent))
            yield return new ValidationResult("PlainTextContent can't be empty when Content not empty", new []{nameof(PlainTextContent)});
    }
}