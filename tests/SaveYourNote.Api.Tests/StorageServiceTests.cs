using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SaveYourNote.Api.Configuration;
using SaveYourNote.Api.Models;
using SaveYourNote.Api.Services;

namespace SaveYourNote.Api.Tests;

public class StorageServiceTests : IDisposable
{
    private readonly StorageService _service;
    private readonly string _testBasePath;
    private readonly Mock<ILogger<StorageService>> _loggerMock;

    public StorageServiceTests()
    {
        _testBasePath = Path.Combine(Path.GetTempPath(), $"saveyournote_tests_{Guid.NewGuid()}");
        
        var settings = Options.Create(new StorageSettings
        {
            BasePath = _testBasePath,
            FilmsFolder = "films",
            PasswordsFile = "passwords.txt",
            TasksFolder = "tasks",
            NotesFolder = "notes",
            LinksFolder = "links",
            AudioFolder = "audio"
        });
        
        _loggerMock = new Mock<ILogger<StorageService>>();
        _service = new StorageService(settings, _loggerMock.Object);
    }

    [Fact]
    public async Task SaveMessageAsync_ShouldCreateFilmFile_WhenCategoryIsFilm()
    {
        // Arrange
        var message = new ClassifiedMessage
        {
            OriginalContent = "Watch Inception tonight",
            Category = MessageCategory.Film,
            ProcessedAt = DateTime.UtcNow,
            SenderPhone = "+1234567890"
        };

        // Act
        await _service.SaveMessageAsync(message);

        // Assert
        var filmsFolder = Path.Combine(_testBasePath, "films");
        Assert.True(Directory.Exists(filmsFolder));
        var files = Directory.GetFiles(filmsFolder, "film_*.txt");
        Assert.Single(files);
        var content = await File.ReadAllTextAsync(files[0]);
        Assert.Contains("Watch Inception tonight", content);
    }

    [Fact]
    public async Task SaveMessageAsync_ShouldAppendToPasswordsFile_WhenCategoryIsPassword()
    {
        // Arrange
        var message1 = new ClassifiedMessage
        {
            OriginalContent = "password: secret1",
            Category = MessageCategory.Password,
            ProcessedAt = DateTime.UtcNow,
            SenderPhone = "+1234567890"
        };
        var message2 = new ClassifiedMessage
        {
            OriginalContent = "password: secret2",
            Category = MessageCategory.Password,
            ProcessedAt = DateTime.UtcNow,
            SenderPhone = "+0987654321"
        };

        // Act
        await _service.SaveMessageAsync(message1);
        await _service.SaveMessageAsync(message2);

        // Assert
        var passwordsFile = Path.Combine(_testBasePath, "passwords.txt");
        Assert.True(File.Exists(passwordsFile));
        var lines = await File.ReadAllLinesAsync(passwordsFile);
        Assert.Equal(2, lines.Length);
        Assert.Contains("secret1", lines[0]);
        Assert.Contains("secret2", lines[1]);
    }

    [Fact]
    public async Task SaveMessageAsync_ShouldCreateTaskFile_WhenCategoryIsTask()
    {
        // Arrange
        var message = new ClassifiedMessage
        {
            OriginalContent = "Buy groceries",
            Category = MessageCategory.Task,
            ProcessedAt = DateTime.UtcNow,
            SenderPhone = "+1234567890"
        };

        // Act
        await _service.SaveMessageAsync(message);

        // Assert
        var tasksFolder = Path.Combine(_testBasePath, "tasks");
        Assert.True(Directory.Exists(tasksFolder));
        var files = Directory.GetFiles(tasksFolder, "task_*.txt");
        Assert.Single(files);
    }

    [Fact]
    public async Task SaveMessageAsync_ShouldCreateNoteFile_WhenCategoryIsNote()
    {
        // Arrange
        var message = new ClassifiedMessage
        {
            OriginalContent = "Random thought",
            Category = MessageCategory.Note,
            ProcessedAt = DateTime.UtcNow,
            SenderPhone = "+1234567890"
        };

        // Act
        await _service.SaveMessageAsync(message);

        // Assert
        var notesFolder = Path.Combine(_testBasePath, "notes");
        Assert.True(Directory.Exists(notesFolder));
        var files = Directory.GetFiles(notesFolder, "note_*.txt");
        Assert.Single(files);
    }

    [Fact]
    public async Task SaveAudioAsync_ShouldSaveAudioFile_WithTranscription()
    {
        // Arrange
        var audioData = new byte[] { 0x00, 0x01, 0x02, 0x03 };
        var transcription = "This is the transcribed text";
        var fileName = "test_audio.ogg";

        // Act
        await _service.SaveAudioAsync(fileName, audioData, transcription);

        // Assert
        var audioFolder = Path.Combine(_testBasePath, "audio");
        var audioPath = Path.Combine(audioFolder, fileName);
        var transcriptionPath = Path.ChangeExtension(audioPath, ".txt");
        
        Assert.True(File.Exists(audioPath));
        Assert.True(File.Exists(transcriptionPath));
        
        var savedAudio = await File.ReadAllBytesAsync(audioPath);
        Assert.Equal(audioData, savedAudio);
        
        var savedTranscription = await File.ReadAllTextAsync(transcriptionPath);
        Assert.Equal(transcription, savedTranscription);
    }

    [Fact]
    public async Task SaveAudioAsync_ShouldSaveOnlyAudioFile_WhenNoTranscription()
    {
        // Arrange
        var audioData = new byte[] { 0x00, 0x01, 0x02, 0x03 };
        var fileName = "test_audio_no_transcription.ogg";

        // Act
        await _service.SaveAudioAsync(fileName, audioData);

        // Assert
        var audioFolder = Path.Combine(_testBasePath, "audio");
        var audioPath = Path.Combine(audioFolder, fileName);
        var transcriptionPath = Path.ChangeExtension(audioPath, ".txt");
        
        Assert.True(File.Exists(audioPath));
        Assert.False(File.Exists(transcriptionPath));
    }

    public void Dispose()
    {
        // Cleanup test directory
        if (Directory.Exists(_testBasePath))
        {
            Directory.Delete(_testBasePath, true);
        }
    }
}
