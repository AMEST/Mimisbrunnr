using Mimisbrunnr.Wiki.Contracts;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Wiki.Services;

internal class DraftManager : IDraftManager
{
    private readonly IRepository<Draft> _draftRepository;

    public DraftManager(IRepository<Draft> draftRepository)
    {
        _draftRepository = draftRepository;
    }

    public async Task<Draft> Create(string pageId, string name, string content, UserInfo updatedBy)
    {
        var draft = new Draft(){
            OriginalPageId = pageId,
            Name = name,
            Content = content,
            Updated = DateTime.UtcNow,
            UpdatedBy = updatedBy
        };
        await _draftRepository.Create(draft);
        return draft;
    }

    public Task<Draft> GetByPageId(string pageId)
    {
        return _draftRepository.GetAll().FirstOrDefaultAsync(x => x.OriginalPageId == pageId);
    }

    public Task Remove(Draft draft)
    {
        return _draftRepository.Delete(draft);
    }

    public async Task Remove(string pageId)
    {
        var draft = await GetByPageId(pageId);
        if(draft is null)
            return;

        await _draftRepository.Delete(draft);
    }

    public Task Update(Draft draft, UserInfo updatedBy)
    {
        draft.UpdatedBy = updatedBy;
        draft.Updated = DateTime.UtcNow;
        return _draftRepository.Update(draft);
    }
}