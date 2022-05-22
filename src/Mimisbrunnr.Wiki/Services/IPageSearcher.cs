using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Wiki.Services;

public interface IPageSearcher
{
     Task<IEnumerable<Page>> Search(string text);
}