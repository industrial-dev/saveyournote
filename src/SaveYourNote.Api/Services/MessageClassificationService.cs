using SaveYourNote.Api.Models;
using System.Text.RegularExpressions;

namespace SaveYourNote.Api.Services;

public interface IMessageClassificationService
{
    ClassifiedMessage ClassifyMessage(string content, string? senderPhone = null);
}

public partial class MessageClassificationService : IMessageClassificationService
{
    private readonly ILogger<MessageClassificationService> _logger;
    
    private static readonly string[] FilmKeywords = ["película", "pelicula", "film", "movie", "serie", "series", "watch", "ver", "netflix", "hbo", "disney", "amazon prime", "prime video"];
    private static readonly string[] PasswordKeywords = ["password", "contraseña", "clave", "pass:", "pwd:", "pin:", "código", "codigo", "secret", "credential"];
    private static readonly string[] TaskKeywords = ["todo", "tarea", "task", "hacer", "recordar", "reminder", "pendiente", "buy", "comprar", "call", "llamar"];
    private static readonly string[] LinkPatterns = [@"https?://", @"www\.", @"\.com", @"\.es", @"\.org", @"\.net"];

    public MessageClassificationService(ILogger<MessageClassificationService> logger)
    {
        _logger = logger;
    }

    public ClassifiedMessage ClassifyMessage(string content, string? senderPhone = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        var lowerContent = content.ToLowerInvariant();
        var category = DetermineCategory(lowerContent);
        
        _logger.LogInformation("Message classified as {Category}: {Content}", category, content[..Math.Min(50, content.Length)]);
        
        return new ClassifiedMessage
        {
            OriginalContent = content,
            Category = category,
            ExtractedData = ExtractData(content, category),
            ProcessedAt = DateTime.UtcNow,
            SenderPhone = senderPhone
        };
    }

    private static MessageCategory DetermineCategory(string lowerContent)
    {
        // Check for password patterns first (highest priority for security)
        if (ContainsAnyKeyword(lowerContent, PasswordKeywords) || PasswordPatternRegex().IsMatch(lowerContent))
        {
            return MessageCategory.Password;
        }

        // Check for links
        if (LinkPatterns.Any(pattern => Regex.IsMatch(lowerContent, pattern, RegexOptions.IgnoreCase)))
        {
            return MessageCategory.Link;
        }

        // Check for film/movie references
        if (ContainsAnyKeyword(lowerContent, FilmKeywords))
        {
            return MessageCategory.Film;
        }

        // Check for tasks/todos
        if (ContainsAnyKeyword(lowerContent, TaskKeywords))
        {
            return MessageCategory.Task;
        }

        // Default to Note for unclassified text messages
        return MessageCategory.Note;
    }

    private static bool ContainsAnyKeyword(string content, string[] keywords)
    {
        return keywords.Any(keyword => content.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private static string ExtractData(string content, MessageCategory category)
    {
        return category switch
        {
            MessageCategory.Link => ExtractUrl(content) ?? content,
            MessageCategory.Password => content, // Keep full content for passwords
            _ => content
        };
    }

    private static string? ExtractUrl(string content)
    {
        var match = UrlRegex().Match(content);
        return match.Success ? match.Value : null;
    }

    [GeneratedRegex(@"https?://[^\s]+", RegexOptions.IgnoreCase)]
    private static partial Regex UrlRegex();

    [GeneratedRegex(@"(pass|pwd|pin|clave|contraseña|password|código|codigo)\s*[:=]\s*\S+", RegexOptions.IgnoreCase)]
    private static partial Regex PasswordPatternRegex();
}
