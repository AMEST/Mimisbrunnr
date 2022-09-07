using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Wiki;

public interface IDraftService
{
    Task<DraftModel> GetByPageId(string pageId, UserInfo requestedBy);

    Task Update(string pageId, DraftUpdateModel updateModel, UserInfo updatedBy);

    Task Delete(string pageId, UserInfo deletedBy);
}