using SaveYourNote.Domain.Enums;

namespace SaveYourNote.Application.UseCases.ProcessMessage;

/// <summary>
/// Command for processing a message (generic, not platform-specific)
/// </summary>
public sealed record ProcessMessageCommand(
    string MessageId,
    string SenderId,
    string Content,
    MessageType Type,
    MessageSource Source,
    DateTime Timestamp,
    string? AudioMimeType = null,
    string? AudioSha256 = null
);
