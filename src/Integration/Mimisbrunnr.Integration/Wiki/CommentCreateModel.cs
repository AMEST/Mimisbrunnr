using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Integration.Wiki;

/// <summary>
/// Модель для создания комментария
/// </summary>
public class CommentCreateModel
{
    /// <summary>
    /// Текст комментария (обязательное поле)
    /// </summary>
    [Required]
    public string Message { get; set; }
}
