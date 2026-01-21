using ErrorOr;
using SaveYourNote.Application.DTOs;
using SaveYourNote.Application.UseCases.ProcessMessage;

namespace SaveYourNote.Application.Interfaces;

/// <summary>
/// Interface for generic message operations
/// </summary>
public interface IMessageService
{
    /// <summary>
    /// Processes a message command
    /// </summary>
    /// <param name="command">The message command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>ErrorOr result containing MessageDto or errors</returns>
    Task<ErrorOr<MessageDto>> ProcessMessageAsync(
        ProcessMessageCommand command,
        CancellationToken cancellationToken = default);
}
