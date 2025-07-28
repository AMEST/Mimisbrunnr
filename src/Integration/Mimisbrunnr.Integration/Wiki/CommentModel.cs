using Mimisbrunnr.Integration.User;

namespace Mimisbrunnr.Integration.Wiki;

/// <summary>
/// Comment model
/// </summary>
public class CommentModel
{
    /// <summary>
    /// Unique comment identifier
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Comment text
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Comment creation date
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Comment author
    /// </summary>
    public UserModel Author { get; set; }
}
