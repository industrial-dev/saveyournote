namespace SaveYourNote.Api.Configuration;

public class WhatsAppSettings
{
    public const string SectionName = "WhatsApp";
    
    public string? AccessToken { get; set; }
    public string? PhoneNumberId { get; set; }
    public string? VerifyToken { get; set; }
    public string? ApiVersion { get; set; } = "v18.0";
    public string? BaseUrl { get; set; } = "https://graph.facebook.com";
}

public class StorageSettings
{
    public const string SectionName = "Storage";
    
    public string? BasePath { get; set; } = "./data";
    public string? FilmsFolder { get; set; } = "films";
    public string? PasswordsFile { get; set; } = "passwords.txt";
    public string? TasksFolder { get; set; } = "tasks";
    public string? NotesFolder { get; set; } = "notes";
    public string? LinksFolder { get; set; } = "links";
    public string? AudioFolder { get; set; } = "audio";
}

public class TranscriptionSettings
{
    public const string SectionName = "Transcription";
    
    public string? Provider { get; set; } = "OpenAI";
    public string? ApiKey { get; set; }
    public string? Model { get; set; } = "whisper-1";
}
