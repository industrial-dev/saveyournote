namespace SaveYourNote.Domain.Errors;

/// <summary>
/// Contains all domain-specific errors
/// </summary>
public static class DomainErrors
{
    public static class Message
    {
        public static readonly Error InvalidMessageId = Error.Validation(
            "Message.InvalidMessageId",
            "The message ID is invalid or empty"
        );

        public static readonly Error InvalidSenderId = Error.Validation(
            "Message.InvalidSenderId",
            "The sender ID is invalid or empty"
        );

        public static readonly Error EmptyContent = Error.Validation(
            "Message.EmptyContent",
            "The message content cannot be empty"
        );

        public static readonly Error InvalidTimestamp = Error.Validation(
            "Message.InvalidTimestamp",
            "The message timestamp is invalid"
        );
    }

    public static class TextContent
    {
        public static readonly Error TooLong = Error.Validation(
            "TextContent.TooLong",
            "The text content exceeds the maximum allowed length"
        );

        public static readonly Error Empty = Error.Validation(
            "TextContent.Empty",
            "The text content cannot be empty"
        );
    }

    public static class AudioContent
    {
        public static readonly Error InvalidMimeType = Error.Validation(
            "AudioContent.InvalidMimeType",
            "The audio MIME type is invalid"
        );

        public static readonly Error InvalidAudioId = Error.Validation(
            "AudioContent.InvalidAudioId",
            "The audio ID is invalid or empty"
        );
    }
}

/// <summary>
/// Represents an error in the domain
/// </summary>
public sealed record Error
{
    public string Code { get; }
    public string Description { get; }
    public ErrorType Type { get; }

    private Error(string code, string description, ErrorType type)
    {
        Code = code;
        Description = description;
        Type = type;
    }

    public static Error Validation(string code, string description) =>
        new(code, description, ErrorType.Validation);

    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);

    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);

    public static Error Failure(string code, string description) =>
        new(code, description, ErrorType.Failure);
}

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Failure,
}
