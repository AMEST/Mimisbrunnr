namespace Mimisbrunnr.Wiki.Contracts;

/// <summary>
/// Thrown when a page was modified by someone else after it was read.
/// </summary>
public class PageConcurrencyException : Exception
{
    public PageConcurrencyException(string pageId, Exception innerException = null)
        : base($"Page '{pageId}' was modified by another user. Reload the page and try again.", innerException)
    {
        PageId = pageId;
    }

    /// <summary>
    /// Identifier of the page that was modified concurrently
    /// </summary>
    public string PageId { get; }
}
