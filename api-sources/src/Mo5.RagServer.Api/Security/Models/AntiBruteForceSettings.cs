namespace Mo5.RagServer.Api.Security.Models;

/// <summary>
/// Configuration settings for anti-brute-force protection.
/// All settings are configurable via environment variables.
/// </summary>
public class AntiBruteForceSettings
{
    /// <summary>
    /// Section name in appsettings.json
    /// </summary>
    public const string SectionName = "AntiBruteForceSettings";

    /// <summary>
    /// Enable or disable anti-brute-force protection globally.
    /// Default: true
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Maximum number of failed authentication attempts before blocking an IP.
    /// Default: 3
    /// </summary>
    public int MaxFailedAttempts { get; set; } = 3;

    /// <summary>
    /// Duration in minutes to block an IP after exceeding max failed attempts.
    /// Default: 30 minutes
    /// </summary>
    public int BlockDurationMinutes { get; set; } = 30;

    /// <summary>
    /// List of IP addresses that are exempt from brute-force protection.
    /// </summary>
    public List<string> WhitelistedIps { get; set; } = new() { "127.0.0.1", "::1" };
}

