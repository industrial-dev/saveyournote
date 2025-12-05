using Microsoft.Extensions.Logging;
using Moq;
using SaveYourNote.Api.Models;
using SaveYourNote.Api.Services;

namespace SaveYourNote.Api.Tests;

public class MessageClassificationServiceTests
{
    private readonly MessageClassificationService _service;
    private readonly Mock<ILogger<MessageClassificationService>> _loggerMock;

    public MessageClassificationServiceTests()
    {
        _loggerMock = new Mock<ILogger<MessageClassificationService>>();
        _service = new MessageClassificationService(_loggerMock.Object);
    }

    [Theory]
    [InlineData("Watch this movie tonight: Inception")]
    [InlineData("Have you seen the new Netflix series?")]
    [InlineData("Quiero ver esta película")]
    [InlineData("Check out this film on HBO")]
    [InlineData("Disney+ has a new series")]
    public void ClassifyMessage_ShouldReturnFilm_WhenMessageContainsFilmKeywords(string content)
    {
        // Act
        var result = _service.ClassifyMessage(content);

        // Assert
        Assert.Equal(MessageCategory.Film, result.Category);
        Assert.Equal(content, result.OriginalContent);
    }

    [Theory]
    [InlineData("password: mySecretPass123")]
    [InlineData("La contraseña es: abc123")]
    [InlineData("PIN: 1234")]
    [InlineData("clave: secreto")]
    [InlineData("My pwd: test123")]
    public void ClassifyMessage_ShouldReturnPassword_WhenMessageContainsPasswordKeywords(string content)
    {
        // Act
        var result = _service.ClassifyMessage(content);

        // Assert
        Assert.Equal(MessageCategory.Password, result.Category);
    }

    [Theory]
    [InlineData("Remember to buy milk")]
    [InlineData("TODO: finish the report")]
    [InlineData("Tarea: estudiar para el examen")]
    [InlineData("Llamar a mamá")]
    [InlineData("Comprar pan mañana")]
    public void ClassifyMessage_ShouldReturnTask_WhenMessageContainsTaskKeywords(string content)
    {
        // Act
        var result = _service.ClassifyMessage(content);

        // Assert
        Assert.Equal(MessageCategory.Task, result.Category);
    }

    [Theory]
    [InlineData("Check this out https://example.com")]
    [InlineData("Visit www.google.com")]
    [InlineData("Go to example.com for more info")]
    public void ClassifyMessage_ShouldReturnLink_WhenMessageContainsUrls(string content)
    {
        // Act
        var result = _service.ClassifyMessage(content);

        // Assert
        Assert.Equal(MessageCategory.Link, result.Category);
    }

    [Theory]
    [InlineData("Hello, how are you?")]
    [InlineData("Just a random thought")]
    [InlineData("Testing 123")]
    public void ClassifyMessage_ShouldReturnNote_WhenMessageDoesNotMatchAnyCategory(string content)
    {
        // Act
        var result = _service.ClassifyMessage(content);

        // Assert
        Assert.Equal(MessageCategory.Note, result.Category);
    }

    [Fact]
    public void ClassifyMessage_ShouldSetProcessedAt()
    {
        // Arrange
        var beforeTest = DateTime.UtcNow;

        // Act
        var result = _service.ClassifyMessage("Test message");

        // Assert
        Assert.True(result.ProcessedAt >= beforeTest);
        Assert.True(result.ProcessedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void ClassifyMessage_ShouldSetSenderPhone_WhenProvided()
    {
        // Arrange
        var phone = "+1234567890";

        // Act
        var result = _service.ClassifyMessage("Test message", phone);

        // Assert
        Assert.Equal(phone, result.SenderPhone);
    }

    [Fact]
    public void ClassifyMessage_ShouldThrowArgumentNullException_WhenContentIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.ClassifyMessage(null!));
    }

    [Fact]
    public void ClassifyMessage_ShouldThrowArgumentException_WhenContentIsEmpty()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.ClassifyMessage(""));
    }

    [Theory]
    [InlineData("https://example.com/path?query=test")]
    [InlineData("Check this: http://test.org/page")]
    public void ClassifyMessage_ShouldExtractUrl_WhenCategoryIsLink(string content)
    {
        // Act
        var result = _service.ClassifyMessage(content);

        // Assert
        Assert.Equal(MessageCategory.Link, result.Category);
        Assert.Contains("http", result.ExtractedData);
    }

    [Fact]
    public void ClassifyMessage_PasswordShouldTakePriority_OverOtherCategories()
    {
        // A message with both password and film keywords should be classified as password
        var content = "Watch movie with password: secret123";

        // Act
        var result = _service.ClassifyMessage(content);

        // Assert
        Assert.Equal(MessageCategory.Password, result.Category);
    }
}
