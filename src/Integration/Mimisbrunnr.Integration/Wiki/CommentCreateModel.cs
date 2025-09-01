using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Integration.Wiki;

/// <summary>
/// Comment create contract
/// </summary>
public class CommentCreateModel
{
    /// <summary>
    /// Comment message text
    /// </summary>
    [Required]
    public string Message { get; set; }
}
