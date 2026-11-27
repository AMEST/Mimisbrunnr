using System.Collections.Concurrent;
using Mimisbrunnr.Wiki.Contracts;
using Skidbladnir.Repository.Abstractions;
using Skidbladnir.Storage.Abstractions;

namespace Mimisbrunnr.Wiki.Services;

internal class AttachmentManager : IAttachmentManager, IDisposable
{
    private static readonly TimeSpan LockIdleTimeout = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan LockCleanupInterval = TimeSpan.FromMinutes(5);

    private readonly IRepository<Attachment> _attachmentRepository;
    private readonly IStorage _fileStorage;
    private readonly ConcurrentDictionary<string, UploadLock> _locks = new();
    private readonly Timer _lockCleanupTimer;

    public AttachmentManager(IRepository<Attachment> attachmentRepository, IStorage fileStorage)
    {
        _attachmentRepository = attachmentRepository;
        _fileStorage = fileStorage;
        _lockCleanupTimer = new Timer(_ => CleanupLocks(), null, LockCleanupInterval, LockCleanupInterval);
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

        await Task.WhenAll(attachments.Select(DeleteFile));
        await _attachmentRepository.DeleteAll(x => pageIds.Contains(x.PageId));
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

        var mLock = _locks.GetOrAdd($"{page.Id}-{name}", _ => new UploadLock());
        Interlocked.Exchange(ref mLock.LastUsedTicks, DateTime.UtcNow.Ticks);
        try
        {
            await mLock.Semaphore.WaitAsync();

            var existing = await _attachmentRepository.GetAll()
                .FirstOrDefaultAsync(x => x.PageId == page.Id && x.Name == name);
            if (existing is not null)
                await Remove(existing);

            await _fileStorage.UploadFileAsync(content, attachment.Path);

            await _attachmentRepository.Create(attachment);
        }
        finally
        {
            mLock.Semaphore.Release();
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

    private void CleanupLocks()
    {
        try
        {
            var threshold = DateTime.UtcNow - LockIdleTimeout;
            foreach (var pair in _locks)
            {
                if (pair.Value.Semaphore.CurrentCount != 1)
                    continue;

                var lastUsed = new DateTime(Interlocked.Read(ref pair.Value.LastUsedTicks), DateTimeKind.Utc);
                if (lastUsed > threshold)
                    continue;

                ((ICollection<KeyValuePair<string, UploadLock>>)_locks).Remove(pair);
            }
        }
        catch (Exception)
        {
            // Never let the background cleanup take the process down; the next tick will try again.
        }
    }

    public void Dispose()
    {
        _lockCleanupTimer.Dispose();
    }

    private sealed class UploadLock
    {
        public readonly SemaphoreSlim Semaphore = new(1, 1);
        public long LastUsedTicks = DateTime.UtcNow.Ticks;
    }
}
