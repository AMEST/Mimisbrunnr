using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Wiki.Services;

public interface ICommentManager
{
    Task<Comment[]> GetComments(Page page);

    Task<Comment> GetById(string id);

    Task<Comment> Create(Page page, string message, UserInfo author); 

    Task Remove(Comment comment);

    Task RemoveAll(Page page);
}