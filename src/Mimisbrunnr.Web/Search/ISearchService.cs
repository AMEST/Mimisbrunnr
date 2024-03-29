using Mimisbrunnr.Integration.User;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Search;

public interface ISearchService
{
    Task<IEnumerable<SpaceModel>> SearchSpaces(string text, UserInfo searchBy);

    Task<IEnumerable<PageModel>> SearchPages(string text, UserInfo searchBy);

    Task<IEnumerable<UserModel>> SearchUsers(string text, UserInfo searchBy);
}