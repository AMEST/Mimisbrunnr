namespace Mimisbrunnr.Wiki.Contracts;

/// <summary>
/// The attachment exceeds the maximum size permitted for the requested operation.
/// </summary>
public class AttachmentTooLargeException : Exception
{
    /// <summary>
    /// Creates an error describing the maximum permitted attachment size.
    /// </summary>
    public AttachmentTooLargeException(long maxBytes)
        : base($"Attachment exceeds the {maxBytes} bytes limit.")
    {
    }
}
