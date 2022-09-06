using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Wiki.Services;

public interface IDraftManager
{
    Task<Draft> GetByPageId(string pageId);

    Task<Draft> Create(string pageId, string name, string content, UserInfo updatedBy);

    Task Update(Draft draft, UserInfo updatedBy);

    Task Remove(Draft draft);

    Task Remove(string pageId);
}