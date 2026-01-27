using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SaveYourNote.Api.Models;
using SaveYourNote.Application.Interfaces;
using SaveYourNote.Infrastructure.WhatsApp.DTOs;
using SaveYourNote.Infrastructure.WhatsApp.Mappers;
using SaveYourNote.Infrastructure.WhatsApp.Validators;

namespace SaveYourNote.Api.Controllers;

/// <summary>
/// WhatsApp Business API Webhook Controller
/// </summary>
/// <remarks>
/// Handles webhook verification and message reception from WhatsApp Business API.
/// Supports text messages and audio messages with automatic transcription.
/// </remarks>
[ApiController]
[Route("api/whatsapp/webhook")]
[Produces("application/json")]
[Tags("WhatsApp Webhook")]
public class WhatsAppWebhookController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WhatsAppWebhookController> _logger;
    private readonly IWebHostEnvironment _environment;

    public WhatsAppWebhookController(
        IMessageService messageService,
        IConfiguration configuration,
        ILogger<WhatsAppWebhookController> logger,
        IWebHostEnvironment environment
    )
    {
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    /// <summary>
    /// Webhook Verification Endpoint
    /// </summary>
    /// <remarks>
    /// GET endpoint used by WhatsApp to verify the webhook URL during initial setup.
    ///
    /// **How it works:**
    /// 1. WhatsApp sends a GET request with verification parameters
    /// 2. Server validates the verify_token against configured value
    /// 3. If valid, returns the challenge string to complete verification
    ///
    /// **Setup:**
    /// - Configure WhatsApp:VerifyToken in appsettings.json
    /// - Use the same token when configuring webhook in Meta Developer Console
    ///
    /// **Example request:**
    /// ```
    /// GET /api/whatsapp/webhook?hub.mode=subscribe&amp;hub.verify_token=my_token&amp;hub.challenge=challenge_string
    /// ```
    /// </remarks>
    /// <param name="mode">Subscription mode (should be "subscribe")</param>
    /// <param name="verifyToken">Verification token to validate</param>
    /// <param name="challenge">Challenge string to return if verification succeeds</param>
    /// <returns>Plain text challenge string if successful</returns>
    /// <response code="200">Webhook verified successfully - returns challenge string</response>
    /// <response code="403">Verification failed - invalid token or mode</response>
    /// <response code="500">Server configuration error - verify token not configured</response>
    [HttpGet]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK, "text/plain")]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public IActionResult VerifyWebhook(
        [FromQuery(Name = "hub.mode")] string? mode,
        [FromQuery(Name = "hub.verify_token")] string? verifyToken,
        [FromQuery(Name = "hub.challenge")] string? challenge
    )
    {
        _logger.LogInformation("Webhook verification request received");

        var expectedToken = _configuration["WhatsApp:VerifyToken"];

        if (string.IsNullOrWhiteSpace(expectedToken))
        {
            _logger.LogError("WhatsApp verify token is not configured");
            return StatusCode(
                500,
                new ApiErrorResponse
                {
                    Error = "Server configuration error",
                    Details = "WhatsApp verify token is not configured",
                }
            );
        }

        if (mode == "subscribe" && verifyToken == expectedToken)
        {
            _logger.LogInformation("Webhook verified successfully");
            return Content(challenge ?? string.Empty, "text/plain");
        }

        _logger.LogWarning(
            "Webhook verification failed. Mode: {Mode}, Token valid: {TokenValid}",
            mode,
            verifyToken == expectedToken
        );
        return StatusCode(
            403,
            new ApiErrorResponse
            {
                Error = "Verification failed",
                Details = "Invalid verify token or mode",
            }
        );
    }

    /// <summary>
    /// Receive WhatsApp Messages
    /// </summary>
    /// <remarks>
    /// POST endpoint that receives incoming WhatsApp messages (text and audio).
    ///
    /// **Message Flow:**
    /// 1. Validates webhook signature (X-Hub-Signature-256 header)
    /// 2. Deserializes WhatsApp webhook payload
    /// 3. Maps to internal command structure
    /// 4. Processes message (logs and prepares for AI analysis)
    ///
    /// **Supported Message Types:**
    /// - **Text messages**: Direct text content from users
    /// - **Audio messages**: Voice notes (prepared for transcription)
    ///
    /// **Security:**
    /// - Signature validation using HMAC-SHA256
    /// - Configure WhatsApp:AppSecret in appsettings.json
    /// - Signature checked via X-Hub-Signature-256 header
    ///
    /// **Example Text Message Payload:**
    /// ```json
    /// {
    ///   "object": "whatsapp_business_account",
    ///   "entry": [{
    ///     "changes": [{
    ///       "value": {
    ///         "messages": [{
    ///           "from": "1234567890",
    ///           "type": "text",
    ///           "text": { "body": "Remember to buy milk" }
    ///         }]
    ///       }
    ///     }]
    ///   }]
    /// }
    /// ```
    ///
    /// **Example Audio Message Payload:**
    /// ```json
    /// {
    ///   "object": "whatsapp_business_account",
    ///   "entry": [{
    ///     "changes": [{
    ///       "value": {
    ///         "messages": [{
    ///           "from": "1234567890",
    ///           "type": "audio",
    ///           "audio": { "id": "audio_id_123", "mime_type": "audio/ogg" }
    ///         }]
    ///       }
    ///     }]
    ///   }]
    /// }
    /// ```
    /// </remarks>
    /// <returns>Success response with message details or error information</returns>
    /// <response code="200">Message received and processed successfully</response>
    /// <response code="400">Invalid request - bad signature or payload format</response>
    /// <response code="500">Internal server error during message processing</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<MessageReceivedResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ReceiveMessage()
    {
        try
        {
            // Read raw body for signature validation
            Request.EnableBuffering();
            using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
            var rawBody = await reader.ReadToEndAsync();
            Request.Body.Position = 0;

            // Validate signature (required in production, optional in development)
            var signature = Request.Headers["X-Hub-Signature-256"].FirstOrDefault();
            var appSecret = _configuration["WhatsApp:AppSecret"];

            if (_environment.IsDevelopment())
            {
                // In development, skip signature validation if app secret is not configured
                if (string.IsNullOrWhiteSpace(appSecret))
                {
                    _logger.LogWarning(
                        "AppSecret is not configured, skipping signature validation"
                    );
                }
            }
            else
            {
                // In production, ensure app secret is configured
                if (string.IsNullOrWhiteSpace(appSecret))
                {
                    _logger.LogError("WhatsApp app secret is not configured");
                    return StatusCode(
                        500,
                        new ApiErrorResponse
                        {
                            Error = "Server configuration error",
                            Details = "WhatsApp app secret is not configured",
                        }
                    );
                }

                // Validate signature
                var validationResult = WhatsAppSignatureValidator.ValidateSignature(
                    rawBody,
                    signature,
                    appSecret
                );

                if (validationResult.IsError)
                {
                    _logger.LogWarning(
                        "Signature validation failed: {Error}",
                        validationResult.FirstError.Description
                    );
                    return BadRequest(
                        new ApiErrorResponse
                        {
                            Error = "Invalid signature",
                            Details = validationResult.FirstError.Description,
                        }
                    );
                }
            }

            // Deserialize webhook payload
            var webhookDto = JsonSerializer.Deserialize<WhatsAppWebhookDto>(
                rawBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (webhookDto is null)
            {
                _logger.LogWarning("Failed to deserialize webhook payload");
                return BadRequest(
                    new ApiErrorResponse
                    {
                        Error = "Invalid payload format",
                        Details = "Unable to deserialize WhatsApp webhook payload",
                    }
                );
            }

            // Map to command
            var commandResult = WhatsAppMessageMapper.ToCommand(webhookDto);
            if (commandResult.IsError)
            {
                _logger.LogWarning(
                    "Failed to map webhook to command: {Error}",
                    commandResult.FirstError.Description
                );
                return BadRequest(
                    new ApiErrorResponse
                    {
                        Error = "Invalid message format",
                        Details = commandResult.FirstError.Description,
                    }
                );
            }

            // Process message
            var result = await _messageService.ProcessMessageAsync(commandResult.Value);

            if (result.IsError)
            {
                _logger.LogError(
                    "Failed to process message: {Error}",
                    result.FirstError.Description
                );
                return StatusCode(
                    500,
                    new ApiErrorResponse
                    {
                        Error = "Failed to process message",
                        Details = result.FirstError.Description,
                    }
                );
            }

            _logger.LogInformation(
                "Message processed successfully: {MessageId}",
                result.Value.MessageId
            );

            return Ok(
                new ApiResponse<MessageReceivedResponse>
                {
                    Status = "success",
                    Message = "Message received and logged",
                    Data = new MessageReceivedResponse
                    {
                        MessageId = result.Value.MessageId,
                        MessageType = result.Value.Type.ToString().ToLowerInvariant(),
                        SenderId = result.Value.SenderId,
                        Content = result.Value.Content,
                        ReceivedAt = DateTime.UtcNow,
                    },
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing webhook");
            return StatusCode(
                500,
                new ApiErrorResponse
                {
                    Error = "Internal server error",
                    Details = "An unexpected error occurred",
                }
            );
        }
    }
}
