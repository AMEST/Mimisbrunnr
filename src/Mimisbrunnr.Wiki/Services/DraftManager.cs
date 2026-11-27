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
        // A page has a single draft (unique index on OriginalPageId), so creation is an upsert.
        for (var attempt = 0; ; attempt++)
        {
            var existing = await GetByPageId(pageId);
            if (existing is not null)
                return await Apply(existing, name, content, updatedBy);

            var draft = new Draft()
            {
                OriginalPageId = pageId,
                Name = name,
                Content = content,
                Updated = DateTime.UtcNow,
                UpdatedBy = updatedBy
            };

            try
            {
                await _draftRepository.Create(draft);
                return draft;
            }
            catch (Exception) when (attempt == 0)
            {
                // Another request created the draft between the read and the insert - retry as an update.
            }
        }
    }

    public Task<Draft> GetByPageId(string pageId)
    {
        return _draftRepository.GetAll().FirstOrDefaultAsync(x => x.OriginalPageId == pageId);
    }

    public Task Remove(Draft draft)
    {
        return _draftRepository.Delete(draft);
    }

    public Task Remove(string pageId)
    {
        return _draftRepository.DeleteAll(x => x.OriginalPageId == pageId);
    }

    public Task RemoveForPages(string[] pageIds)
    {
        if (pageIds is null || pageIds.Length == 0)
            return Task.CompletedTask;
        return _draftRepository.DeleteAll(x => pageIds.Contains(x.OriginalPageId));
    }

    public Task Update(Draft draft, UserInfo updatedBy)
    {
        draft.UpdatedBy = updatedBy;
        draft.Updated = DateTime.UtcNow;
        return _draftRepository.Update(draft);
    }

    private async Task<Draft> Apply(Draft draft, string name, string content, UserInfo updatedBy)
    {
        draft.Name = name;
        draft.Content = content;
        draft.Updated = DateTime.UtcNow;
        draft.UpdatedBy = updatedBy;
        await _draftRepository.Update(draft);
        return draft;
    }
}
