using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SaveYourNote.Api.Configuration;
using SaveYourNote.Api.Models;
using SaveYourNote.Api.Services;

namespace SaveYourNote.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly IMessageProcessorService _messageProcessor;
    private readonly WhatsAppSettings _settings;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(
        IMessageProcessorService messageProcessor,
        IOptions<WhatsAppSettings> settings,
        ILogger<WebhookController> logger)
    {
        _messageProcessor = messageProcessor;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Webhook verification endpoint for WhatsApp
    /// </summary>
    [HttpGet]
    public IActionResult Verify(
        [FromQuery(Name = "hub.mode")] string? mode,
        [FromQuery(Name = "hub.verify_token")] string? token,
        [FromQuery(Name = "hub.challenge")] string? challenge)
    {
        _logger.LogInformation("Webhook verification request received. Mode: {Mode}", mode);

        if (mode == "subscribe" && token == _settings.VerifyToken)
        {
            _logger.LogInformation("Webhook verified successfully");
            return Ok(challenge);
        }

        _logger.LogWarning("Webhook verification failed. Invalid token or mode.");
        return Forbid();
    }

    /// <summary>
    /// Webhook endpoint to receive WhatsApp messages
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ReceiveMessage([FromBody] WhatsAppWebhookPayload payload)
    {
        _logger.LogInformation("Webhook message received");

        try
        {
            await _messageProcessor.ProcessWebhookAsync(payload);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook");
            // Return 200 to avoid WhatsApp retries
            return Ok();
        }
    }
}
