namespace SaveYourNote.Api.Models;

public enum MessageCategory
{
    Film,
    Password,
    Task,
    Note,
    Link,
    Audio,
    Unknown
}

public class ClassifiedMessage
{
    public string? OriginalContent { get; set; }
    public MessageCategory Category { get; set; }
    public string? ExtractedData { get; set; }
    public DateTime ProcessedAt { get; set; }
    public string? SenderPhone { get; set; }
}
