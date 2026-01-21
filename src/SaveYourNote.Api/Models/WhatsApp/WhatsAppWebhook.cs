namespace SaveYourNote.Api.Models.WhatsApp;

/// <summary>
/// Webhook payload de WhatsApp Business API
/// Documentación: https://developers.facebook.com/docs/whatsapp/cloud-api/webhooks/payload-examples
/// </summary>
public class WhatsAppWebhook
{
    public string Object { get; set; } = string.Empty;
    public List<WhatsAppEntry> Entry { get; set; } = new();
}

public class WhatsAppEntry
{
    public string Id { get; set; } = string.Empty;
    public List<WhatsAppChange> Changes { get; set; } = new();
}

public class WhatsAppChange
{
    public WhatsAppValue Value { get; set; } = new();
    public string Field { get; set; } = string.Empty;
}

public class WhatsAppValue
{
    public string MessagingProduct { get; set; } = string.Empty;
    public WhatsAppMetadata Metadata { get; set; } = new();
    public List<WhatsAppContact>? Contacts { get; set; }
    public List<WhatsAppMessage>? Messages { get; set; }
    public List<WhatsAppStatus>? Statuses { get; set; }
}

public class WhatsAppMetadata
{
    public string DisplayPhoneNumber { get; set; } = string.Empty;
    public string PhoneNumberId { get; set; } = string.Empty;
}

public class WhatsAppContact
{
    public WhatsAppProfile Profile { get; set; } = new();
    public string WaId { get; set; } = string.Empty;
}

public class WhatsAppProfile
{
    public string Name { get; set; } = string.Empty;
}

public class WhatsAppMessage
{
    public string From { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;

    // Tipos de contenido
    public WhatsAppTextMessage? Text { get; set; }
    public WhatsAppImageMessage? Image { get; set; }
    public WhatsAppAudioMessage? Audio { get; set; }
    public WhatsAppVideoMessage? Video { get; set; }
    public WhatsAppDocumentMessage? Document { get; set; }
    public WhatsAppLocationMessage? Location { get; set; }
    public WhatsAppContextMessage? Context { get; set; }
}

public class WhatsAppTextMessage
{
    public string Body { get; set; } = string.Empty;
}

public class WhatsAppImageMessage
{
    public string? Caption { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public string Sha256 { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
}

public class WhatsAppAudioMessage
{
    public string MimeType { get; set; } = string.Empty;
    public string Sha256 { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public bool Voice { get; set; }
}

public class WhatsAppVideoMessage
{
    public string? Caption { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public string Sha256 { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
}

public class WhatsAppDocumentMessage
{
    public string? Caption { get; set; }
    public string Filename { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public string Sha256 { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
}

public class WhatsAppLocationMessage
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Name { get; set; }
    public string? Address { get; set; }
}

public class WhatsAppContextMessage
{
    public string From { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
}

public class WhatsAppStatus
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
    public string RecipientId { get; set; } = string.Empty;
}
