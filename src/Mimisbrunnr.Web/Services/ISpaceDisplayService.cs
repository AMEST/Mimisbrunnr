using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Services;

public interface ISpaceDisplayService
{
    Task<IEnumerable<Space>> FindUserVisibleSpaces(UserInfo userInfo, int? take = null, int? skip = null);
}