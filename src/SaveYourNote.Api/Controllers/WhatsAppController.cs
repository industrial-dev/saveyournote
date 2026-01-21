using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SaveYourNote.Api.Models.WhatsApp;

namespace SaveYourNote.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WhatsAppController : ControllerBase
{
    private readonly ILogger<WhatsAppController> _logger;
    private readonly IConfiguration _configuration;

    public WhatsAppController(ILogger<WhatsAppController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Verificación del webhook de WhatsApp
    /// WhatsApp enviará una petición GET para verificar el webhook
    /// </summary>
    [HttpGet]
    public IActionResult VerifyWebhook(
        [FromQuery(Name = "hub.mode")] string mode,
        [FromQuery(Name = "hub.verify_token")] string token,
        [FromQuery(Name = "hub.challenge")] string challenge
    )
    {
        _logger.LogInformation(
            "Webhook verification attempt - Mode: {Mode}, Token: {Token}",
            mode,
            token
        );

        var verifyToken = _configuration["WhatsApp:VerifyToken"] ?? "your_verify_token_here";

        if (mode == "subscribe" && token == verifyToken)
        {
            _logger.LogInformation("Webhook verified successfully!");
            return Ok(challenge);
        }

        _logger.LogWarning("Webhook verification failed - Invalid token");
        return Unauthorized();
    }

    /// <summary>
    /// Endpoint principal del webhook que recibe mensajes y eventos de WhatsApp
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ReceiveWebhook([FromBody] WhatsAppWebhook webhook)
    {
        try
        {
            _logger.LogInformation("=== WEBHOOK RECEIVED ===");
            _logger.LogInformation("Object: {Object}", webhook.Object);

            if (webhook.Object != "whatsapp_business_account")
            {
                _logger.LogWarning("Unknown webhook object type: {Object}", webhook.Object);
                return BadRequest("Invalid object type");
            }

            foreach (var entry in webhook.Entry)
            {
                _logger.LogInformation("Entry ID: {EntryId}", entry.Id);

                foreach (var change in entry.Changes)
                {
                    _logger.LogInformation("Change Field: {Field}", change.Field);

                    var value = change.Value;
                    _logger.LogInformation(
                        "Phone Number ID: {PhoneNumberId}",
                        value.Metadata.PhoneNumberId
                    );
                    _logger.LogInformation(
                        "Display Phone: {DisplayPhone}",
                        value.Metadata.DisplayPhoneNumber
                    );

                    // Procesar mensajes recibidos
                    if (value.Messages != null && value.Messages.Any())
                    {
                        foreach (var message in value.Messages)
                        {
                            LogMessage(message, value.Contacts);
                        }
                    }

                    // Procesar estados de mensajes (entregado, leído, etc.)
                    if (value.Statuses != null && value.Statuses.Any())
                    {
                        foreach (var status in value.Statuses)
                        {
                            _logger.LogInformation(
                                "Status Update - Message ID: {MessageId}, Status: {Status}, Recipient: {RecipientId}",
                                status.Id,
                                status.Status,
                                status.RecipientId
                            );
                        }
                    }
                }
            }

            _logger.LogInformation("=== WEBHOOK PROCESSED ===");

            // WhatsApp espera un 200 OK
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook");
            // Aún así retornamos 200 para evitar reenvíos de WhatsApp
            return Ok();
        }
    }

    private void LogMessage(WhatsAppMessage message, List<WhatsAppContact>? contacts)
    {
        var contactName =
            contacts?.FirstOrDefault(c => c.WaId == message.From)?.Profile.Name ?? "Unknown";

        _logger.LogInformation("--- MESSAGE RECEIVED ---");
        _logger.LogInformation("From: {From} ({Name})", message.From, contactName);
        _logger.LogInformation("Message ID: {MessageId}", message.Id);
        _logger.LogInformation("Timestamp: {Timestamp}", message.Timestamp);
        _logger.LogInformation("Type: {Type}", message.Type);

        switch (message.Type.ToLower())
        {
            case "text":
                if (message.Text != null)
                {
                    _logger.LogInformation("📱 TEXT MESSAGE: {Body}", message.Text.Body);
                }
                break;

            case "image":
                if (message.Image != null)
                {
                    _logger.LogInformation(
                        "🖼️ IMAGE: ID={ImageId}, Caption={Caption}",
                        message.Image.Id,
                        message.Image.Caption ?? "(no caption)"
                    );
                }
                break;

            case "audio":
                if (message.Audio != null)
                {
                    _logger.LogInformation(
                        "🎵 AUDIO: ID={AudioId}, Voice={IsVoice}",
                        message.Audio.Id,
                        message.Audio.Voice
                    );
                }
                break;

            case "video":
                if (message.Video != null)
                {
                    _logger.LogInformation(
                        "🎥 VIDEO: ID={VideoId}, Caption={Caption}",
                        message.Video.Id,
                        message.Video.Caption ?? "(no caption)"
                    );
                }
                break;

            case "document":
                if (message.Document != null)
                {
                    _logger.LogInformation(
                        "📄 DOCUMENT: ID={DocumentId}, Filename={Filename}",
                        message.Document.Id,
                        message.Document.Filename
                    );
                }
                break;

            case "location":
                if (message.Location != null)
                {
                    _logger.LogInformation(
                        "📍 LOCATION: Lat={Latitude}, Long={Longitude}, Name={Name}",
                        message.Location.Latitude,
                        message.Location.Longitude,
                        message.Location.Name ?? "(no name)"
                    );
                }
                break;

            default:
                _logger.LogInformation("Unknown message type: {Type}", message.Type);
                break;
        }

        // Si es una respuesta a otro mensaje
        if (message.Context != null)
        {
            _logger.LogInformation("↩️ Reply to Message ID: {ReplyToId}", message.Context.Id);
        }

        _logger.LogInformation("--- END MESSAGE ---");
    }
}
