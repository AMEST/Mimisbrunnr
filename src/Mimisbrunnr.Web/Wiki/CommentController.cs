using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Web.Filters;
using Mimisbrunnr.Web.Mapping;

namespace Mimisbrunnr.Web.Wiki;

/// <summary>
/// Controller for managing page comments
/// </summary>
[Route("api/Page/{pageId}/comments")]
[ApiController]
[Authorize]
[HandleWikiErrors]
public class CommentController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentController(ICommentService commentService)
    {
        _commentService = commentService;
    }
    
    /// <summary>
    /// Get all comments for a page
    /// </summary>
    /// <param name="pageId">ID of the page</param>
    /// <returns>List of comments</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CommentModel[]), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetAll([FromRoute] string pageId)
    {
        var comments = await _commentService.GetComments(pageId, User?.ToInfo());
        if (comments is null)
            return Ok(Array.Empty<CommentModel>());

        return Ok(comments);
    }
    
    /// <summary>
    /// Get a specific comment by ID
    /// </summary>
    /// <param name="pageId">ID of the page</param>
    /// <param name="commentId">ID of the comment</param>
    /// <returns>The requested comment</returns>
    [HttpGet("{commentId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CommentModel), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Get([FromRoute] string pageId, [FromRoute] string commentId)
    {
        var comment = await _commentService.GetById(pageId, commentId,User?.ToInfo());

        return Ok(comment);
    }
    
    /// <summary>
    /// Create a new comment on a page
    /// </summary>
    /// <param name="pageId">ID of the page</param>
    /// <param name="model">Comment content</param>
    /// <returns>The created comment</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CommentModel), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Create([FromRoute] string pageId, [FromBody] CommentCreateModel model)
    {
        try
        {
            var comment = await _commentService.Create(pageId, model, User?.ToInfo());
            return Ok(comment);
        }
        catch (ArgumentNullException e)
        {
            return BadRequest(e.Message);
        }
    }
    
    /// <summary>
    /// Delete a comment
    /// </summary>
    /// <param name="pageId">ID of the page</param>
    /// <param name="commentId">ID of the comment</param>
    [HttpDelete("{commentId}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Remove([FromRoute] string pageId, [FromRoute] string commentId)
    {
        try
        {
            await _commentService.Remove(pageId, commentId, User?.ToInfo());
            return Ok();
        }
        catch (ArgumentNullException e)
        {
            return BadRequest(e.Message);
        }
    }
}
