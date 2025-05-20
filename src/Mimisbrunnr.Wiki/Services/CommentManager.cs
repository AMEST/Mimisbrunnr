using System.Web;
using Mimisbrunnr.Wiki.Contracts;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Wiki.Services;

internal class CommentManager : ICommentManager
{
    private readonly IRepository<Comment> _repository;

    public CommentManager(IRepository<Comment> repository)
    {
        _repository = repository;
    }

    public async Task<Comment> Create(Page page, string message, UserInfo author)
    {
        var comment = new Comment()
        {
            PageId = page.Id,
            Author = author,
            Message = HttpUtility.HtmlEncode(message),
            Created = DateTime.UtcNow
        };
        await _repository.Create(comment);
        return comment;
    }

    public Task<Comment> GetById(string id)
    {
        return _repository.GetAll().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Comment[]> GetComments(Page page)
    {
        return await _repository.GetAll().Where(x => x.PageId == page.Id).ToArrayAsync();
    }

    public Task Remove(Comment comment)
    {
        return _repository.Delete(comment);
    }

    public async Task RemoveAll(Page page)
    {
        await _repository.DeleteAll( x => x.PageId == page.Id);
    }
}