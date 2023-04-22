using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Integration.Wiki;

public class CommentCreateModel
{
    [Required]
    public string Message { get; set; }
}