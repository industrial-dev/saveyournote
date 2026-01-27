namespace SaveYourNote.Domain.Errors;

/// <summary>
/// Contains all domain-specific errors
/// </summary>
public static class DomainErrors
{
    public static class Message
    {
        public static readonly DomainError InvalidMessageId = DomainError.Validation(
            "Message.InvalidMessageId",
            "The message ID is invalid or empty"
        );

        public static readonly DomainError InvalidSenderId = DomainError.Validation(
            "Message.InvalidSenderId",
            "The sender ID is invalid or empty"
        );

        public static readonly DomainError EmptyContent = DomainError.Validation(
            "Message.EmptyContent",
            "The message content cannot be empty"
        );

        public static readonly DomainError InvalidTimestamp = DomainError.Validation(
            "Message.InvalidTimestamp",
            "The message timestamp is invalid"
        );
    }

    public static class TextContent
    {
        public static readonly DomainError TooLong = DomainError.Validation(
            "TextContent.TooLong",
            "The text content exceeds the maximum allowed length"
        );

        public static readonly DomainError Empty = DomainError.Validation(
            "TextContent.Empty",
            "The text content cannot be empty"
        );
    }

    public static class AudioContent
    {
        public static readonly DomainError InvalidMimeType = DomainError.Validation(
            "AudioContent.InvalidMimeType",
            "The audio MIME type is invalid"
        );

        public static readonly DomainError InvalidAudioId = DomainError.Validation(
            "AudioContent.InvalidAudioId",
            "The audio ID is invalid or empty"
        );
    }
}

/// <summary>
/// Represents an error in the domain
/// </summary>
public sealed record DomainError
{
    public string Code { get; }
    public string Description { get; }
    public ErrorType Type { get; }

    private DomainError(string code, string description, ErrorType type)
    {
        Code = code;
        Description = description;
        Type = type;
    }

    public static DomainError Validation(string code, string description) =>
        new(code, description, ErrorType.Validation);

    public static DomainError NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);

    public static DomainError Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);

    public static DomainError Failure(string code, string description) =>
        new(code, description, ErrorType.Failure);
}

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Failure,
}
