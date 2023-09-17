using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Wiki;

public interface ICommentService
{
    Task<IEnumerable<CommentModel>> GetComments(string pageId, UserInfo requestedBy);
    
    Task<CommentModel> GetById(string pageId, string commentId, UserInfo requestedBy);
    
    Task<CommentModel> Create(string pageId, CommentCreateModel model, UserInfo createdBy);

    Task Remove(string pageId, string commentId, UserInfo deletedBy);
}