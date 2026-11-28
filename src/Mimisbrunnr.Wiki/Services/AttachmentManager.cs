using Mimisbrunnr.Wiki.Contracts;
using Skidbladnir.Repository.Abstractions;
using Skidbladnir.Storage.Abstractions;

namespace Mimisbrunnr.Wiki.Services;

internal class AttachmentManager : IAttachmentManager
{
    private readonly IRepository<Attachment> _attachmentRepository;
    private readonly IStorage _fileStorage;
    private readonly Dictionary<(string PageId, string Name), UploadLock> _locks = new();

    public AttachmentManager(IRepository<Attachment> attachmentRepository, IStorage fileStorage)
    {
        _attachmentRepository = attachmentRepository;
        _fileStorage = fileStorage;
    }

    public Task<Attachment[]> GetAttachments(Page page)
    {
        return _attachmentRepository.GetAll()
            .Where(x => x.PageId == page.Id)
            .OrderBy(x => x.Name)
            .ToArrayAsync();
    }

    public Task<Stream> GetAttachmentContent(Page page, string name)
        => GetAttachmentContentCore(page, name, null);

    public Task<Stream> GetAttachmentContent(Page page, string name, long maxBytes)
        => GetAttachmentContentCore(page, name, maxBytes);

    private async Task<Stream> GetAttachmentContentCore(Page page, string name, long? maxBytes)
    {
        var attachment = await _attachmentRepository.GetAll().FirstOrDefaultAsync(x => x.PageId == page.Id && x.Name == name);
        if (attachment is null)
            return null;

        if (maxBytes.HasValue)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(maxBytes.Value);
            var info = await _fileStorage.GetFileAsync(attachment.Path);
            if (info is null)
                return null;
            if (info.Size > maxBytes.Value)
                throw new AttachmentTooLargeException(maxBytes.Value);
        }

        var downloadResult = await _fileStorage.DownloadFileAsync(attachment.Path);
        if (downloadResult is null)
            return null;

        return downloadResult.Content;
    }

    public async Task Remove(Page page, string name)
    {
        var attachment = await _attachmentRepository.GetAll().FirstOrDefaultAsync(x => x.PageId == page.Id && x.Name == name);
        if (attachment is null)
            return;

        await Remove(attachment);
    }

    public Task RemoveAll(Page page) => RemoveAllForPages([page.Id]);

    public async Task RemoveAllForPages(string[] pageIds)
    {
        if (pageIds is null || pageIds.Length == 0)
            return;

        var attachments = await _attachmentRepository.GetAll()
            .Where(x => pageIds.Contains(x.PageId))
            .ToArrayAsync();
        if (attachments.Length == 0)
            return;

        var deletions = attachments.Select(x => (x.Id, FileDeletion: DeleteFile(x))).ToArray();
        try
        {
            await Task.WhenAll(deletions.Select(x => x.FileDeletion));
        }
        finally
        {
            // Keep metadata for failed deletions so the caller can retry those files.
            var deletedIds = deletions.Where(x => x.FileDeletion.IsCompletedSuccessfully)
                .Select(x => x.Id)
                .ToArray();
            if (deletedIds.Length > 0)
                await _attachmentRepository.DeleteAll(x => deletedIds.Contains(x.Id));
        }
    }

    public async Task Upload(Page page, Stream content, string name, UserInfo uploadedBy)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        if (content == null)
            throw new ArgumentNullException(nameof(content));

        var attachment = new Attachment()
        {
            PageId = page.Id,
            Created = DateTime.UtcNow,
            CreatedBy = uploadedBy,
            Name = name,
            Path = $"attachments/{Guid.NewGuid()}.bin"
        };

        var key = (page.Id, name);
        UploadLock uploadLock;
        lock (_locks)
        {
            if (!_locks.TryGetValue(key, out uploadLock))
            {
                uploadLock = new UploadLock();
                _locks.Add(key, uploadLock);
            }

            // Count each waiter before it can use the semaphore.
            uploadLock.Users++;
        }

        await uploadLock.Semaphore.WaitAsync();
        try
        {
            var existing = await _attachmentRepository.GetAll()
                .FirstOrDefaultAsync(x => x.PageId == page.Id && x.Name == name);
            if (existing is not null)
                await Remove(existing);

            await _fileStorage.UploadFileAsync(content, attachment.Path);

            await _attachmentRepository.Create(attachment);
        }
        finally
        {
            uploadLock.Semaphore.Release();
            lock (_locks)
            {
                if (--uploadLock.Users == 0)
                {
                    _locks.Remove(key);
                    uploadLock.Semaphore.Dispose();
                }
            }
        }
    }

    private async Task Remove(Attachment attachment)
    {
        await DeleteFile(attachment);
        await _attachmentRepository.Delete(attachment);
    }

    private async Task DeleteFile(Attachment attachment)
    {
        try
        {
            await _fileStorage.DeleteAsync(attachment.Path);
        }
        catch (FileNotFoundException) { }
    }

    private sealed class UploadLock
    {
        public readonly SemaphoreSlim Semaphore = new(1, 1);
        public int Users;
    }
}
