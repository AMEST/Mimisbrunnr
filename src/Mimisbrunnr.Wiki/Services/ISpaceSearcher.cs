using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Wiki.Services;

public interface ISpaceSearcher 
{
     Task<IEnumerable<Space>> Search(string text);
}