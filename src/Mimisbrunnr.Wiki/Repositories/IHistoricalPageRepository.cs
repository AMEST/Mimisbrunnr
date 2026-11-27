using Mimisbrunnr.Wiki.Contracts;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Wiki.Repositories;

/// <summary>
/// Page history storage operations that cannot be expressed with <see cref="IRepository{TEntity}"/>
/// </summary>
public interface IHistoricalPageRepository : IRepository<HistoricalPage>
{
    /// <summary>
    /// Stores the snapshot of the page version. A snapshot of the same version is overwritten,
    /// so the operation can be safely retried.
    /// </summary>
    Task SaveVersion(HistoricalPage historicalPage);
}
