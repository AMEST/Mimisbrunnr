using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Wiki;

/// <summary>
/// Service for managing page comments
/// </summary>
public interface ICommentService
{
    /// <summary>
    /// Get all comments for a page
    /// </summary>
    /// <param name="pageId">Page identifier</param>
    /// <param name="requestedBy">User requesting the comments</param>
    /// <returns>List of comments</returns>
    Task<IEnumerable<CommentModel>> GetComments(string pageId, UserInfo requestedBy);
    
    /// <summary>
    /// Get a comment by its identifier
    /// </summary>
    /// <param name="pageId">Page identifier</param>
    /// <param name="commentId">Comment identifier</param>
    /// <param name="requestedBy">User requesting the comment</param>
    /// <returns>The requested comment</returns>
    Task<CommentModel> GetById(string pageId, string commentId, UserInfo requestedBy);
    
    /// <summary>
    /// Create a new comment on a page
    /// </summary>
    /// <param name="pageId">Page identifier</param>
    /// <param name="model">Comment creation parameters</param>
    /// <param name="createdBy">User creating the comment</param>
    /// <returns>The created comment</returns>
    Task<CommentModel> Create(string pageId, CommentCreateModel model, UserInfo createdBy);

    /// <summary>
    /// Remove a comment from a page
    /// </summary>
    /// <param name="pageId">Page identifier</param>
    /// <param name="commentId">Comment identifier</param>
    /// <param name="deletedBy">User deleting the comment</param>
    Task Remove(string pageId, string commentId, UserInfo deletedBy);
}