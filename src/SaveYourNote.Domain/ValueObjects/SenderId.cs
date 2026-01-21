namespace SaveYourNote.Domain.ValueObjects;

/// <summary>
/// Value object representing a sender identifier (phone number, email, user ID, etc.)
/// </summary>
public sealed record SenderId
{
    public string Value { get; }

    private SenderId(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new SenderId from a string value
    /// </summary>
    /// <param name="value">The sender identifier (phone, email, user ID, etc.)</param>
    /// <returns>A SenderId instance or null if invalid</returns>
    public static SenderId? Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (value.Length > 200) // Reasonable max length
        {
            return null;
        }

        return new SenderId(value.Trim());
    }

    public override string ToString() => Value;
}
