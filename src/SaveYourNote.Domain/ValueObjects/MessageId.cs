namespace SaveYourNote.Domain.ValueObjects;

/// <summary>
/// Value object representing a unique message identifier
/// </summary>
public sealed record MessageId
{
    public string Value { get; }

    private MessageId(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new MessageId from a string value
    /// </summary>
    /// <param name="value">The message ID string</param>
    /// <returns>A MessageId instance or null if invalid</returns>
    public static MessageId? Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (value.Length > 500) // Reasonable max length for message IDs
        {
            return null;
        }

        return new MessageId(value.Trim());
    }

    public override string ToString() => Value;
}
