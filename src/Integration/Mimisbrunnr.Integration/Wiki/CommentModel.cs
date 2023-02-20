using Mimisbrunnr.Integration.User;

namespace Mimisbrunnr.Integration.Wiki;

public class CommentModel
{
    public string Id { get; set; }

    public string Message { get; set; }

    public DateTime Created { get; set; }

    public UserModel Author { get; set; }
}