using SaveYourNote.Domain.Entities;

namespace SaveYourNote.Application.Interfaces;

/// <summary>
/// Interface for logging messages to console or other outputs
/// </summary>
public interface IMessageLogger
{
    /// <summary>
    /// Logs a message to the output
    /// </summary>
    /// <param name="message">The message to log</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task LogMessageAsync(Message message, CancellationToken cancellationToken = default);
}
