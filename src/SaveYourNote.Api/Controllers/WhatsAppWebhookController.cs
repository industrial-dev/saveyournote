using Microsoft.AspNetCore.Mvc;
using SaveYourNote.Application.Interfaces;
using SaveYourNote.Infrastructure.WhatsApp.DTOs;
using SaveYourNote.Infrastructure.WhatsApp.Mappers;
using SaveYourNote.Infrastructure.WhatsApp.Validators;
using System.Text;
using System.Text.Json;

namespace SaveYourNote.Api.Controllers;

/// <summary>
/// WhatsApp webhook controller
/// Handles webhook verification and message reception from WhatsApp
/// </summary>
[ApiController]
[Route("api/whatsapp/webhook")]
public class WhatsAppWebhookController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WhatsAppWebhookController> _logger;

    public WhatsAppWebhookController(
        IMessageService messageService,
        IConfiguration configuration,
        ILogger<WhatsAppWebhookController> logger)
    {
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// GET endpoint for WhatsApp webhook verification
    /// </summary>
    [HttpGet]
    public IActionResult VerifyWebhook(
        [FromQuery(Name = "hub.mode")] string? mode,
        [FromQuery(Name = "hub.verify_token")] string? verifyToken,
        [FromQuery(Name = "hub.challenge")] string? challenge)
    {
        _logger.LogInformation("Webhook verification request received");

        var expectedToken = _configuration["WhatsApp:VerifyToken"];

        if (string.IsNullOrWhiteSpace(expectedToken))
        {
            _logger.LogError("WhatsApp verify token is not configured");
            return StatusCode(500, new { error = "Server configuration error" });
        }

        if (mode == "subscribe" && verifyToken == expectedToken)
        {
            _logger.LogInformation("Webhook verified successfully");
            return Content(challenge ?? string.Empty, "text/plain");
        }

        _logger.LogWarning("Webhook verification failed");
        return Forbid();
    }

    /// <summary>
    /// POST endpoint for receiving WhatsApp messages
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ReceiveMessage()
    {
        try
        {
            // Read raw body for signature validation
            Request.EnableBuffering();
            using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
            var rawBody = await reader.ReadToEndAsync();
            Request.Body.Position = 0;

            // Validate signature (optional in development, required in production)
            var signature = Request.Headers["X-Hub-Signature-256"].FirstOrDefault();
            var appSecret = _configuration["WhatsApp:AppSecret"];

            if (!string.IsNullOrWhiteSpace(appSecret))
            {
                var validationResult = WhatsAppSignatureValidator.ValidateSignature(
                    rawBody,
                    signature,
                    appSecret);

                if (validationResult.IsError)
                {
                    _logger.LogWarning("Signature validation failed: {Error}",
                        validationResult.FirstError.Description);
                    return BadRequest(new { error = "Invalid signature" });
                }
            }

            // Deserialize webhook payload
            var webhookDto = JsonSerializer.Deserialize<WhatsAppWebhookDto>(
                rawBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (webhookDto is null)
            {
                _logger.LogWarning("Failed to deserialize webhook payload");
                return BadRequest(new { error = "Invalid payload format" });
            }

            // Map to command
            var commandResult = WhatsAppMessageMapper.ToCommand(webhookDto);
            if (commandResult.IsError)
            {
                _logger.LogWarning("Failed to map webhook to command: {Error}",
                    commandResult.FirstError.Description);
                return BadRequest(new { error = commandResult.FirstError.Description });
            }

            // Process message
            var result = await _messageService.ProcessMessageAsync(commandResult.Value);

            if (result.IsError)
            {
                _logger.LogError("Failed to process message: {Error}",
                    result.FirstError.Description);
                return StatusCode(500, new
                {
                    error = "Failed to process message",
                    details = result.FirstError.Description
                });
            }

            _logger.LogInformation("Message processed successfully: {MessageId}",
                result.Value.MessageId);

            return Ok(new
            {
                status = "success",
                message = "Message received and logged",
                data = result.Value
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing webhook");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
