using ErrorOr;
using SaveYourNote.Application.DTOs;
using SaveYourNote.Application.Errors;
using SaveYourNote.Application.Interfaces;
using SaveYourNote.Domain.Entities;
using SaveYourNote.Domain.Enums;
using SaveYourNote.Domain.ValueObjects;

namespace SaveYourNote.Application.UseCases.ProcessMessage;

/// <summary>
/// Handler for processing message commands
/// Implements the use case with ErrorOr pattern for functional error handling
/// </summary>
public sealed class ProcessMessageHandler : IMessageService
{
    private readonly IMessageLogger _messageLogger;

    public ProcessMessageHandler(IMessageLogger messageLogger)
    {
        _messageLogger = messageLogger ?? throw new ArgumentNullException(nameof(messageLogger));
    }

    /// <summary>
    /// Handles the process message command
    /// </summary>
    public async Task<ErrorOr<MessageDto>> ProcessMessageAsync(
        ProcessMessageCommand command,
        CancellationToken cancellationToken = default)
    {
        // 1. Validate command
        if (command is null)
        {
            return ApplicationErrors.Message.InvalidCommand;
        }

        // 2. Create domain entity from command
        var messageResult = CreateMessageFromCommand(command);
        if (messageResult is null)
        {
            return ApplicationErrors.Message.CreationFailed;
        }

        // 3. Validate message
        if (!messageResult.IsValid())
        {
            return ApplicationErrors.Message.InvalidCommand;
        }

        // 4. Log message to console
        try
        {
            await _messageLogger.LogMessageAsync(messageResult, cancellationToken);
        }
        catch (Exception)
        {
            return ApplicationErrors.Message.LoggingFailed;
        }

        // 5. Map to DTO and return success
        var dto = MapToDto(messageResult);
        return dto;
    }

    private static Message? CreateMessageFromCommand(ProcessMessageCommand command)
    {
        var messageId = MessageId.Create(command.MessageId);
        var senderId = SenderId.Create(command.SenderId);

        if (messageId is null || senderId is null)
        {
            return null;
        }

        return command.Type switch
        {
            MessageType.Text => CreateTextMessage(messageId, senderId, command),
            MessageType.Audio => CreateAudioMessage(messageId, senderId, command),
            _ => null
        };
    }

    private static Message? CreateTextMessage(
        MessageId messageId,
        SenderId senderId,
        ProcessMessageCommand command)
    {
        var textContent = TextContent.Create(command.Content);
        if (textContent is null)
        {
            return null;
        }

        return Message.CreateTextMessage(
            messageId,
            senderId,
            command.Source,
            command.Timestamp,
            textContent);
    }

    private static Message? CreateAudioMessage(
        MessageId messageId,
        SenderId senderId,
        ProcessMessageCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.AudioMimeType))
        {
            return null;
        }

        var audioContent = AudioContent.Create(
            command.Content, // Content is the audio ID for audio messages
            command.AudioMimeType,
            command.AudioSha256);

        if (audioContent is null)
        {
            return null;
        }

        return Message.CreateAudioMessage(
            messageId,
            senderId,
            command.Source,
            command.Timestamp,
            audioContent);
    }

    private static MessageDto MapToDto(Message message)
    {
        return new MessageDto(
            message.Id.Value,
            message.SenderId.Value,
            message.GetContentDisplay(),
            message.Type,
            message.Source,
            message.Timestamp);
    }
}
