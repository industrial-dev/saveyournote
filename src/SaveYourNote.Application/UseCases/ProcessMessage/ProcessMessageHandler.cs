using ErrorOr;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<ProcessMessageHandler> _logger;

    public ProcessMessageHandler(ILogger<ProcessMessageHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the process message command
    /// </summary>
    public async Task<ErrorOr<MessageDto>> ProcessMessageAsync(
        ProcessMessageCommand command,
        CancellationToken cancellationToken = default
    )
    {
        if (command is null)
        {
            return ApplicationErrors.Message.InvalidCommand;
        }

        var messageResult = CreateMessageFromCommand(command);
        if (messageResult is null)
        {
            return ApplicationErrors.Message.CreationFailed;
        }

        if (!messageResult.IsValid())
        {
            return ApplicationErrors.Message.InvalidCommand;
        }

        LogMessageReceived(messageResult);

        var dto = MapToDto(messageResult);
        return dto;
    }

    private void LogMessageReceived(Message message)
    {
        var contentDisplay =
            message.Type == MessageType.Text
                ? message.TextContent?.Value ?? "[Empty]"
                : $"Audio: {message.AudioContent?.AudioId}";

        _logger.LogInformation(
            "📨 Message Received - ID: {MessageId}, Source: {Source}, From: {SenderId}, Type: {MessageType}, Content: {Content}, Timestamp: {Timestamp:yyyy-MM-dd HH:mm:ss}",
            message.Id.Value,
            message.Source,
            message.SenderId.Value,
            message.Type,
            contentDisplay,
            message.Timestamp
        );

        if (message.Type == MessageType.Audio && message.AudioContent is not null)
        {
            _logger.LogDebug(
                "🎵 Audio Details - MimeType: {MimeType}, SHA256: {Sha256}",
                message.AudioContent.MimeType,
                message.AudioContent.Sha256
            );
        }
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
            _ => null,
        };
    }

    private static Message? CreateTextMessage(
        MessageId messageId,
        SenderId senderId,
        ProcessMessageCommand command
    )
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
            textContent
        );
    }

    private static Message? CreateAudioMessage(
        MessageId messageId,
        SenderId senderId,
        ProcessMessageCommand command
    )
    {
        if (string.IsNullOrWhiteSpace(command.AudioMimeType))
        {
            return null;
        }

        var audioContent = AudioContent.Create(
            command.Content,
            command.AudioMimeType,
            command.AudioSha256
        );

        if (audioContent is null)
        {
            return null;
        }

        return Message.CreateAudioMessage(
            messageId,
            senderId,
            command.Source,
            command.Timestamp,
            audioContent
        );
    }

    private static MessageDto MapToDto(Message message)
    {
        return new MessageDto(
            message.Id.Value,
            message.SenderId.Value,
            message.GetContentDisplay(),
            message.Type,
            message.Source,
            message.Timestamp
        );
    }
}
