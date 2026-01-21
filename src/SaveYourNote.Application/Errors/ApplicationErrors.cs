using ErrorOr;

namespace SaveYourNote.Application.Errors;

/// <summary>
/// Contains all application-specific errors
/// </summary>
public static class ApplicationErrors
{
    public static class Message
    {
        public static readonly Error ProcessingFailed = Error.Failure(
            "Message.ProcessingFailed",
            "Failed to process the message");

        public static readonly Error LoggingFailed = Error.Failure(
            "Message.LoggingFailed",
            "Failed to log the message");

        public static readonly Error InvalidCommand = Error.Validation(
            "Message.InvalidCommand",
            "The message command is invalid");

        public static readonly Error CreationFailed = Error.Failure(
            "Message.CreationFailed",
            "Failed to create message entity from command");
    }
}
