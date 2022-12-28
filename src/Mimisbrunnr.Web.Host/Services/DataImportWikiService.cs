using Mimisbrunnr.DataImport;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Web.Wiki;

namespace Mimisbrunnr.Web.Host.Services
{
    internal class DataImportWikiService : IWikiService
    {
        private readonly ISpaceService _spaceService;
        private readonly IPageService _pageService;
        private readonly IAttachmentService _attachmentService;
        private readonly IHttpContextAccessor _contextAccessor;

        public DataImportWikiService(ISpaceService spaceService,
            IPageService pageService,
            IAttachmentService attachmentService,
            IHttpContextAccessor contextAccessor
        )
        {
            _spaceService = spaceService;
            _pageService = pageService;
            _attachmentService = attachmentService;
            _contextAccessor = contextAccessor;
        }

        public Task<PageModel> CreatePage(PageCreateModel createModel)
        {
            var user = UserMapper.Instance.ToInfo(_contextAccessor.HttpContext.User);
            return _pageService.Create(createModel, user);
        }

        public Task<PageModel> GetPageById(string id)
        {
            var user = UserMapper.Instance.ToInfo(_contextAccessor.HttpContext.User);
            return _pageService.GetById(id, user);
        }

        public Task<SpaceModel> GetSpaceByKey(string key)
        {
            var user = UserMapper.Instance.ToInfo(_contextAccessor.HttpContext.User);
            return _spaceService.GetByKey(key, user);
        }

        public Task UpdatePage(string pageId, PageUpdateModel updateModel)
        {
            var user = UserMapper.Instance.ToInfo(_contextAccessor.HttpContext.User);
            return _pageService.Update(pageId, updateModel, user);
        }

        public Task UploadAttachment(string pageId, Stream content, string name)
        {
            var user = UserMapper.Instance.ToInfo(_contextAccessor.HttpContext.User);
            return _attachmentService.Upload(pageId, content, name, user);
        }
    }
}