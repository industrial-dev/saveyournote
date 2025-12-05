using System.Text.Json.Serialization;

namespace SaveYourNote.Api.Models;

public class WhatsAppWebhookPayload
{
    [JsonPropertyName("object")]
    public string? Object { get; set; }
    
    [JsonPropertyName("entry")]
    public List<Entry>? Entry { get; set; }
}

public class Entry
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("changes")]
    public List<Change>? Changes { get; set; }
}

public class Change
{
    [JsonPropertyName("value")]
    public Value? Value { get; set; }
    
    [JsonPropertyName("field")]
    public string? Field { get; set; }
}

public class Value
{
    [JsonPropertyName("messaging_product")]
    public string? MessagingProduct { get; set; }
    
    [JsonPropertyName("metadata")]
    public Metadata? Metadata { get; set; }
    
    [JsonPropertyName("contacts")]
    public List<Contact>? Contacts { get; set; }
    
    [JsonPropertyName("messages")]
    public List<WhatsAppMessage>? Messages { get; set; }
}

public class Metadata
{
    [JsonPropertyName("display_phone_number")]
    public string? DisplayPhoneNumber { get; set; }
    
    [JsonPropertyName("phone_number_id")]
    public string? PhoneNumberId { get; set; }
}

public class Contact
{
    [JsonPropertyName("profile")]
    public Profile? Profile { get; set; }
    
    [JsonPropertyName("wa_id")]
    public string? WaId { get; set; }
}

public class Profile
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class WhatsAppMessage
{
    [JsonPropertyName("from")]
    public string? From { get; set; }
    
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }
    
    [JsonPropertyName("type")]
    public string? Type { get; set; }
    
    [JsonPropertyName("text")]
    public TextContent? Text { get; set; }
    
    [JsonPropertyName("audio")]
    public AudioContent? Audio { get; set; }
    
    [JsonPropertyName("image")]
    public MediaContent? Image { get; set; }
    
    [JsonPropertyName("document")]
    public DocumentContent? Document { get; set; }
    
    [JsonPropertyName("video")]
    public MediaContent? Video { get; set; }
}

public class TextContent
{
    [JsonPropertyName("body")]
    public string? Body { get; set; }
}

public class AudioContent
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("mime_type")]
    public string? MimeType { get; set; }
}

public class MediaContent
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("mime_type")]
    public string? MimeType { get; set; }
    
    [JsonPropertyName("caption")]
    public string? Caption { get; set; }
}

public class DocumentContent
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("mime_type")]
    public string? MimeType { get; set; }
    
    [JsonPropertyName("filename")]
    public string? Filename { get; set; }
}
