using Mimisbrunnr.Wiki.Contracts;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Wiki.Services
{
    internal class FeedManager : IFeedManager
    {
        private readonly IRepository<PageUpdateEvent> _pageUpdatesRepository;

        public FeedManager(IRepository<PageUpdateEvent> pageUpdatesRepository)
        {
            _pageUpdatesRepository = pageUpdatesRepository;
        }

        public Task<PageUpdateEvent[]> GetAllPageUpdates()
        {
            return _pageUpdatesRepository.GetAll().OrderByDescending(x => x.Updated).Take(15).ToArrayAsync();
        }

        public Task<PageUpdateEvent[]> GetPageUpdates(UserInfo requestedBy, IEnumerable<Space> userSpaces = null, UserInfo updatedBy = null)
        {
            var query = _pageUpdatesRepository.GetAll().OrderByDescending(x => x.Updated);
            if (requestedBy is null)
                return query.Where(x => x.SpaceType == SpaceType.Public).Take(15).ToArrayAsync();

            if (userSpaces is null)
                throw new ArgumentNullException(nameof(userSpaces));

            if (updatedBy is null)
            {
                var userSpacesKeys = userSpaces.Select(x=>x.Key).ToArray();
                return query.Where(x => userSpacesKeys.Contains(x.SpaceKey))
                .Take(15)
                .ToArrayAsync();
            }

            if (requestedBy.Email == updatedBy.Email)
                return query.Where(x => x.UpdatedBy.Email == updatedBy.Email).Take(15).ToArrayAsync();

            return query.Where(x => x.SpaceType == SpaceType.Public && x.UpdatedBy.Email == updatedBy.Email).Take(15).ToArrayAsync();
        }

        public Task AddPageUpdate(Space space, Page page, UserInfo updatedBy)
        {
            return _pageUpdatesRepository.Create(new PageUpdateEvent
            {
                SpaceKey = space.Key,
                SpaceType = space.Type,
                PageId = page.Id,
                PageTitle = page.Name,
                Updated = DateTime.UtcNow,
                UpdatedBy = updatedBy
            });
        }
    }
}