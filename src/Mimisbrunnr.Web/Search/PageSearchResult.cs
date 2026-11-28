using Mimisbrunnr.Integration.Wiki;

namespace Mimisbrunnr.Web.Search;

/// <summary>
/// A page of search results and the offset needed to continue the search.
/// </summary>
/// <param name="Pages">Matching wiki pages</param>
/// <param name="NextOffset">Offset of the next set of results, if more matches exist</param>
public sealed record PageSearchResult(PageModel[] Pages, int? NextOffset);
