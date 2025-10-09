using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Wiki.Services;

public interface IPageSearcher
{
     Task<Page[]> Search(string text);
}