using SaveYourNote.Domain.Enums;

namespace SaveYourNote.Application.DTOs;

/// <summary>
/// Data Transfer Object for messages (technology-agnostic)
/// </summary>
public sealed record MessageDto(
    string MessageId,
    string SenderId,
    string Content,
    MessageType Type,
    MessageSource Source,
    DateTime Timestamp
);
