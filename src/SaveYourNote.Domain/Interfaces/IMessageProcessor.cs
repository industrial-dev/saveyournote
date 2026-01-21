using SaveYourNote.Domain.Entities;

namespace SaveYourNote.Domain.Interfaces;

/// <summary>
/// Interface for processing messages (to be implemented in future phases)
/// </summary>
public interface IMessageProcessor
{
    /// <summary>
    /// Processes a message (transcription, AI analysis, etc.)
    /// </summary>
    /// <param name="message">The message to process</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task ProcessAsync(Message message, CancellationToken cancellationToken = default);
}
