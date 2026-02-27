namespace Mo5.RagServer.Api.Security.Models;

/// <summary>
/// Configuration settings for rate limiting.
/// All settings are configurable via environment variables.
/// </summary>
public class RateLimitSettings
{
    /// <summary>
    /// Section name in appsettings.json
    /// </summary>
    public const string SectionName = "RateLimitSettings";

    /// <summary>
    /// Enable or disable rate limiting globally.
    /// Default: true
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Maximum number of requests allowed per minute per IP address.
    /// Default: 30
    /// </summary>
    public int RequestsPerMinute { get; set; } = 30;

    /// <summary>
    /// Maximum number of requests allowed per hour per IP address.
    /// Default: 1000
    /// </summary>
    public int RequestsPerHour { get; set; } = 1000;

    /// <summary>
    /// List of IP addresses that are exempt from rate limiting.
    /// </summary>
    public List<string> WhitelistedIps { get; set; } = new() { "127.0.0.1", "::1" };
}

