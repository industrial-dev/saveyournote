using System.Text.Json.Serialization;

namespace SaveYourNote.Infrastructure.WhatsApp.DTOs;

/// <summary>
/// WhatsApp webhook payload DTO (infrastructure-specific)
/// Based on WhatsApp Business API documentation
/// </summary>
public sealed record WhatsAppWebhookDto(
    [property: JsonPropertyName("object")] string Object,
    [property: JsonPropertyName("entry")] List<WhatsAppEntryDto> Entry
);

public sealed record WhatsAppEntryDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("changes")] List<WhatsAppChangeDto> Changes
);

public sealed record WhatsAppChangeDto(
    [property: JsonPropertyName("value")] WhatsAppValueDto Value,
    [property: JsonPropertyName("field")] string? Field
);

public sealed record WhatsAppValueDto(
    [property: JsonPropertyName("messaging_product")] string MessagingProduct,
    [property: JsonPropertyName("metadata")] WhatsAppMetadataDto? Metadata,
    [property: JsonPropertyName("contacts")] List<WhatsAppContactDto>? Contacts,
    [property: JsonPropertyName("messages")] List<WhatsAppMessageDto>? Messages
);

public sealed record WhatsAppMetadataDto(
    [property: JsonPropertyName("display_phone_number")] string DisplayPhoneNumber,
    [property: JsonPropertyName("phone_number_id")] string PhoneNumberId
);

public sealed record WhatsAppContactDto(
    [property: JsonPropertyName("profile")] WhatsAppProfileDto Profile,
    [property: JsonPropertyName("wa_id")] string WaId
);

public sealed record WhatsAppProfileDto(
    [property: JsonPropertyName("name")] string Name
);

public sealed record WhatsAppMessageDto(
    [property: JsonPropertyName("from")] string From,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("timestamp")] string Timestamp,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("text")] WhatsAppTextDto? Text,
    [property: JsonPropertyName("audio")] WhatsAppAudioDto? Audio
);

public sealed record WhatsAppTextDto(
    [property: JsonPropertyName("body")] string Body
);

public sealed record WhatsAppAudioDto(
    [property: JsonPropertyName("mime_type")] string MimeType,
    [property: JsonPropertyName("sha256")] string? Sha256,
    [property: JsonPropertyName("id")] string Id
);
