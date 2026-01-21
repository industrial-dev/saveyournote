namespace SaveYourNote.Domain.ValueObjects;

/// <summary>
/// Value object representing audio message metadata
/// </summary>
public sealed record AudioContent
{
    public string AudioId { get; }
    public string MimeType { get; }
    public string? Sha256 { get; }

    private AudioContent(string audioId, string mimeType, string? sha256)
    {
        AudioId = audioId;
        MimeType = mimeType;
        Sha256 = sha256;
    }

    /// <summary>
    /// Creates a new AudioContent instance
    /// </summary>
    /// <param name="audioId">The audio file identifier</param>
    /// <param name="mimeType">The MIME type of the audio</param>
    /// <param name="sha256">Optional SHA256 hash of the audio file</param>
    /// <returns>An AudioContent instance or null if invalid</returns>
    public static AudioContent? Create(string audioId, string mimeType, string? sha256 = null)
    {
        if (string.IsNullOrWhiteSpace(audioId))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(mimeType))
        {
            return null;
        }

        // Validate MIME type format (basic validation)
        if (!mimeType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return new AudioContent(audioId.Trim(), mimeType.Trim(), sha256?.Trim());
    }

    public override string ToString() => $"Audio: {AudioId} ({MimeType})";
}
