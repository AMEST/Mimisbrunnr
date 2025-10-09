using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Wiki.Services;

public interface ISpaceSearcher 
{
     Task<Space[]> Search(string text);
}