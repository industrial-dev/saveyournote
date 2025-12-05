using Microsoft.Extensions.Options;
using SaveYourNote.Api.Configuration;
using SaveYourNote.Api.Models;

namespace SaveYourNote.Api.Services;

public interface IStorageService
{
    Task SaveMessageAsync(ClassifiedMessage message);
    Task SaveAudioAsync(string fileName, byte[] audioData, string? transcription = null);
}

public class StorageService : IStorageService
{
    private readonly StorageSettings _settings;
    private readonly ILogger<StorageService> _logger;
    private readonly string _basePath;

    public StorageService(IOptions<StorageSettings> settings, ILogger<StorageService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _basePath = _settings.BasePath ?? "./data";
        
        // Ensure base directories exist
        EnsureDirectoriesExist();
    }

    private void EnsureDirectoriesExist()
    {
        var directories = new[]
        {
            _basePath,
            Path.Combine(_basePath, _settings.FilmsFolder ?? "films"),
            Path.Combine(_basePath, _settings.TasksFolder ?? "tasks"),
            Path.Combine(_basePath, _settings.NotesFolder ?? "notes"),
            Path.Combine(_basePath, _settings.LinksFolder ?? "links"),
            Path.Combine(_basePath, _settings.AudioFolder ?? "audio")
        };

        foreach (var dir in directories)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                _logger.LogInformation("Created directory: {Directory}", dir);
            }
        }
    }

    public async Task SaveMessageAsync(ClassifiedMessage message)
    {
        var (filePath, content) = message.Category switch
        {
            MessageCategory.Film => GetFilmPath(message),
            MessageCategory.Password => GetPasswordPath(message),
            MessageCategory.Task => GetTaskPath(message),
            MessageCategory.Link => GetLinkPath(message),
            MessageCategory.Note => GetNotePath(message),
            MessageCategory.Audio => GetAudioNotePath(message),
            _ => GetNotePath(message)
        };

        if (message.Category == MessageCategory.Password)
        {
            // Append to passwords file
            await File.AppendAllTextAsync(filePath, content + Environment.NewLine);
        }
        else
        {
            // Create individual files for other types
            await File.WriteAllTextAsync(filePath, content);
        }

        _logger.LogInformation("Saved {Category} message to {Path}", message.Category, filePath);
    }

    public async Task SaveAudioAsync(string fileName, byte[] audioData, string? transcription = null)
    {
        var audioFolder = Path.Combine(_basePath, _settings.AudioFolder ?? "audio");
        var audioPath = Path.Combine(audioFolder, fileName);
        
        await File.WriteAllBytesAsync(audioPath, audioData);
        _logger.LogInformation("Saved audio file: {Path}", audioPath);

        if (!string.IsNullOrEmpty(transcription))
        {
            var transcriptionPath = Path.ChangeExtension(audioPath, ".txt");
            await File.WriteAllTextAsync(transcriptionPath, transcription);
            _logger.LogInformation("Saved transcription: {Path}", transcriptionPath);
        }
    }

    private (string Path, string Content) GetFilmPath(ClassifiedMessage message)
    {
        var folder = Path.Combine(_basePath, _settings.FilmsFolder ?? "films");
        var fileName = $"film_{message.ProcessedAt:yyyyMMdd_HHmmss}.txt";
        var content = FormatMessage(message);
        return (Path.Combine(folder, fileName), content);
    }

    private (string Path, string Content) GetPasswordPath(ClassifiedMessage message)
    {
        var filePath = Path.Combine(_basePath, _settings.PasswordsFile ?? "passwords.txt");
        var content = $"[{message.ProcessedAt:yyyy-MM-dd HH:mm:ss}] From: {message.SenderPhone ?? "Unknown"} | {message.OriginalContent}";
        return (filePath, content);
    }

    private (string Path, string Content) GetTaskPath(ClassifiedMessage message)
    {
        var folder = Path.Combine(_basePath, _settings.TasksFolder ?? "tasks");
        var fileName = $"task_{message.ProcessedAt:yyyyMMdd_HHmmss}.txt";
        var content = FormatMessage(message);
        return (Path.Combine(folder, fileName), content);
    }

    private (string Path, string Content) GetLinkPath(ClassifiedMessage message)
    {
        var folder = Path.Combine(_basePath, _settings.LinksFolder ?? "links");
        var fileName = $"link_{message.ProcessedAt:yyyyMMdd_HHmmss}.txt";
        var content = FormatMessage(message);
        return (Path.Combine(folder, fileName), content);
    }

    private (string Path, string Content) GetNotePath(ClassifiedMessage message)
    {
        var folder = Path.Combine(_basePath, _settings.NotesFolder ?? "notes");
        var fileName = $"note_{message.ProcessedAt:yyyyMMdd_HHmmss}.txt";
        var content = FormatMessage(message);
        return (Path.Combine(folder, fileName), content);
    }

    private (string Path, string Content) GetAudioNotePath(ClassifiedMessage message)
    {
        var folder = Path.Combine(_basePath, _settings.AudioFolder ?? "audio");
        var fileName = $"audio_note_{message.ProcessedAt:yyyyMMdd_HHmmss}.txt";
        var content = FormatMessage(message);
        return (Path.Combine(folder, fileName), content);
    }

    private static string FormatMessage(ClassifiedMessage message)
    {
        return $"""
            Date: {message.ProcessedAt:yyyy-MM-dd HH:mm:ss UTC}
            From: {message.SenderPhone ?? "Unknown"}
            Category: {message.Category}
            ---
            {message.OriginalContent}
            """;
    }
}
