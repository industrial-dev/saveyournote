namespace SaveYourNote.Domain.ValueObjects;

/// <summary>
/// Value object representing text message content
/// </summary>
public sealed record TextContent
{
    public string Value { get; }

    private TextContent(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new TextContent from a string value
    /// </summary>
    /// <param name="value">The text content</param>
    /// <returns>A TextContent instance or null if invalid</returns>
    public static TextContent? Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (value.Length > 10000) // Max 10k characters for text messages
        {
            return null;
        }

        // Trim and normalize whitespace
        var normalized = value.Trim();

        return new TextContent(normalized);
    }

    public override string ToString() => Value;
}
