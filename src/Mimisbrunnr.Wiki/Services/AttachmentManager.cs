using Mimisbrunnr.Wiki.Contracts;
using Skidbladnir.Repository.Abstractions;
using Skidbladnir.Storage.Abstractions;

namespace Mimisbrunnr.Wiki.Services;

internal class AttachmentManager : IAttachmentManager
{
    private readonly IRepository<Attachment> _attachmentRepository;
    private readonly IStorage _fileStorage;

    public AttachmentManager(IRepository<Attachment> attachmentRepository, IStorage fileStorage)
    {
        _attachmentRepository = attachmentRepository;
        _fileStorage = fileStorage;
    }

    public Task<Attachment[]> GetAttachments(Page page)
    {
        return _attachmentRepository.GetAll().Where(x => x.PageId == page.Id).ToArrayAsync();
    }

    public async Task<Stream> GetAttachmentContent(Page page, string name)
    {
        var attachment = await _attachmentRepository.GetAll().FirstOrDefaultAsync(x => x.Name == name);
        if(attachment is null)
            return null;
        
        var downloadResult = await _fileStorage.DownloadFileAsync(attachment.Path);
        if(downloadResult is null)
            return null;

        return downloadResult.Content;
    }

    public async Task Remove(Page page, string name)
    {
        var attachment = await _attachmentRepository.GetAll().FirstOrDefaultAsync(x => x.Name == name);
        if(attachment is null)
            return;
        
        await _fileStorage.DeleteAsync(attachment.Path);

        await _attachmentRepository.Delete(attachment);
    }

    public async Task Upload(Page page, Stream content, string name, UserInfo uploadedBy)
    {
        if(string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        if(content == null)
            throw new ArgumentNullException(nameof(content));

        var attachment = new Attachment(){
            PageId = page.Id,
            Created = DateTime.UtcNow,
            CreatedBy = uploadedBy,
            Name = name,
            Path = $"attachments/{Guid.NewGuid()}.bin"
        };

        await _fileStorage.UploadFileAsync(content, attachment.Path);
        
        await _attachmentRepository.Create(attachment);
    }
}
