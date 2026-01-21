using SaveYourNote.Application.Interfaces;
using SaveYourNote.Domain.Entities;
using SaveYourNote.Domain.Enums;

namespace SaveYourNote.Infrastructure.Logging;

/// <summary>
/// Implements IMessageLogger to output messages to console with structured formatting
/// </summary>
public sealed class ConsoleMessageLogger : IMessageLogger
{
    private const string Separator = "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━";

    public Task LogMessageAsync(Message message, CancellationToken cancellationToken = default)
    {
        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"[{timestamp}] Message Received");
        Console.ResetColor();
        
        Console.WriteLine(Separator);
        
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"Message ID: {message.Id}");
        Console.WriteLine($"Source: {GetSourceDisplay(message.Source)}");
        Console.WriteLine($"From: {message.SenderId}");
        Console.WriteLine($"Type: {message.Type}");
        Console.WriteLine($"Timestamp: {message.Timestamp:yyyy-MM-dd HH:mm:ss} UTC");
        
        if (message.Type == MessageType.Text && message.TextContent is not null)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Content: {message.TextContent.Value}");
        }
        else if (message.Type == MessageType.Audio && message.AudioContent is not null)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"Audio ID: {message.AudioContent.AudioId}");
            Console.WriteLine($"MIME Type: {message.AudioContent.MimeType}");
            if (!string.IsNullOrWhiteSpace(message.AudioContent.Sha256))
            {
                Console.WriteLine($"SHA256: {message.AudioContent.Sha256}");
            }
        }
        
        Console.ResetColor();
        Console.WriteLine(Separator);
        Console.WriteLine();

        return Task.CompletedTask;
    }

    private static string GetSourceDisplay(MessageSource source)
    {
        return source switch
        {
            MessageSource.WhatsApp => "📱 WhatsApp",
            MessageSource.WebApp => "🌐 Web App",
            MessageSource.MobileApp => "📲 Mobile App",
            MessageSource.Api => "🔌 API",
            _ => source.ToString()
        };
    }
}
