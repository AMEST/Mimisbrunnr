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
        return await _repository.GetAll()
            .Where(x => x.PageId == page.Id)
            .OrderBy(x => x.Created)
            .ToArrayAsync();
    }

    public Task Remove(Comment comment)
    {
        return _repository.Delete(comment);
    }

    public Task RemoveAll(Page page)
    {
        return _repository.DeleteAll(x => x.PageId == page.Id);
    }

    public Task RemoveAllForPages(string[] pageIds)
    {
        if (pageIds is null || pageIds.Length == 0)
            return Task.CompletedTask;
        return _repository.DeleteAll(x => pageIds.Contains(x.PageId));
    }
}
