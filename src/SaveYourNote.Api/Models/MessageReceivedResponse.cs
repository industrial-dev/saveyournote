namespace SaveYourNote.Api.Models;

/// <summary>
/// Response data after successfully receiving a WhatsApp message
/// </summary>
public class MessageReceivedResponse
{
    /// <summary>
    /// Unique identifier of the processed message
    /// </summary>
    /// <example>wamid.HBgLMTIzNDU2Nzg5MGFkFQIAERgSMTIzNDU2Nzg5MGFiY2RlZg==</example>
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// Type of message received
    /// </summary>
    /// <example>text</example>
    public string MessageType { get; set; } = string.Empty;

    /// <summary>
    /// Sender's WhatsApp ID
    /// </summary>
    /// <example>1234567890</example>
    public string SenderId { get; set; } = string.Empty;

    /// <summary>
    /// Content of the message
    /// </summary>
    /// <example>Remember to buy milk</example>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the message was received
    /// </summary>
    /// <example>2026-01-26T10:30:00Z</example>
    public DateTime ReceivedAt { get; set; }
}

/// <summary>
/// Response data for webhook verification
/// </summary>
public class WebhookVerificationResponse
{
    /// <summary>
    /// Status of the verification
    /// </summary>
    /// <example>verified</example>
    public string Status { get; set; } = "verified";

    /// <summary>
    /// Verification message
    /// </summary>
    /// <example>Webhook verified successfully</example>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Health check response
/// </summary>
public class HealthCheckResponse
{
    /// <summary>
    /// Health status
    /// </summary>
    /// <example>healthy</example>
    public string Status { get; set; } = "healthy";

    /// <summary>
    /// Timestamp of the health check
    /// </summary>
    /// <example>2026-01-26T10:30:00Z</example>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Service name
    /// </summary>
    /// <example>SaveYourNote API</example>
    public string Service { get; set; } = "SaveYourNote API";

    /// <summary>
    /// API version
    /// </summary>
    /// <example>1.0.0</example>
    public string Version { get; set; } = "1.0.0";
}
