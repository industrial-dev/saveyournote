namespace SaveYourNote.Domain.Enums;

/// <summary>
/// Defines the source/origin of a message
/// </summary>
public enum MessageSource
{
    /// <summary>
    /// Message received from WhatsApp
    /// </summary>
    WhatsApp,

    /// <summary>
    /// Message received from a web application
    /// </summary>
    WebApp,

    /// <summary>
    /// Message received from a mobile application
    /// </summary>
    MobileApp,

    /// <summary>
    /// Message received directly via API
    /// </summary>
    Api,
}
