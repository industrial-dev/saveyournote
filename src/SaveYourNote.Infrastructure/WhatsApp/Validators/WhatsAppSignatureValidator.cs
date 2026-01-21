using ErrorOr;
using System.Security.Cryptography;
using System.Text;

namespace SaveYourNote.Infrastructure.WhatsApp.Validators;

/// <summary>
/// Validates WhatsApp webhook signatures using HMAC-SHA256
/// </summary>
public static class WhatsAppSignatureValidator
{
    /// <summary>
    /// Validates the WhatsApp webhook signature
    /// </summary>
    /// <param name="payload">The raw request body</param>
    /// <param name="signature">The X-Hub-Signature-256 header value</param>
    /// <param name="appSecret">The WhatsApp app secret</param>
    /// <returns>ErrorOr Success or validation error</returns>
    public static ErrorOr<Success> ValidateSignature(
        string payload,
        string? signature,
        string appSecret)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return Error.Validation(
                "WhatsApp.EmptyPayload",
                "Request payload is empty");
        }

        if (string.IsNullOrWhiteSpace(signature))
        {
            return Error.Validation(
                "WhatsApp.MissingSignature",
                "X-Hub-Signature-256 header is missing");
        }

        if (string.IsNullOrWhiteSpace(appSecret))
        {
            return Error.Failure(
                "WhatsApp.MissingSecret",
                "WhatsApp app secret is not configured");
        }

        // Remove 'sha256=' prefix if present
        var signatureValue = signature.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase)
            ? signature[7..]
            : signature;

        // Compute HMAC-SHA256
        var expectedSignature = ComputeHmacSha256(payload, appSecret);

        // Compare signatures (constant-time comparison to prevent timing attacks)
        if (!AreEqual(expectedSignature, signatureValue))
        {
            return Error.Validation(
                "WhatsApp.InvalidSignature",
                "Webhook signature validation failed");
        }

        return Result.Success;
    }

    private static string ComputeHmacSha256(string payload, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(payloadBytes);
        
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    private static bool AreEqual(string a, string b)
    {
        if (a.Length != b.Length)
        {
            return false;
        }

        var result = 0;
        for (var i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }

        return result == 0;
    }
}
