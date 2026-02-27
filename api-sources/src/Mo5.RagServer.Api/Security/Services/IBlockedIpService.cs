namespace Mo5.RagServer.Api.Security.Services;

/// <summary>
/// Service interface for managing blocked IP addresses.
/// Implements the Strategy pattern to allow different storage backends (Memory, Redis, etc.)
/// </summary>
public interface IBlockedIpService
{
    /// <summary>
    /// Checks if an IP address is currently blocked.
    /// </summary>
    /// <param name="ipAddress">The IP address to check.</param>
    /// <returns>True if the IP is blocked, false otherwise.</returns>
    Task<bool> IsBlockedAsync(string ipAddress);

    /// <summary>
    /// Records a failed authentication attempt for an IP address.
    /// If the number of attempts exceeds the threshold, the IP will be blocked.
    /// </summary>
    /// <param name="ipAddress">The IP address that failed authentication.</param>
    /// <returns>The current number of failed attempts for this IP.</returns>
    Task<int> RecordFailedAttemptAsync(string ipAddress);

    /// <summary>
    /// Blocks an IP address for the configured duration.
    /// </summary>
    /// <param name="ipAddress">The IP address to block.</param>
    /// <param name="reason">The reason for blocking (e.g., "BruteForce", "RateLimit").</param>
    Task BlockAsync(string ipAddress, string reason);

    /// <summary>
    /// Unblocks an IP address manually.
    /// </summary>
    /// <param name="ipAddress">The IP address to unblock.</param>
    Task UnblockAsync(string ipAddress);

    /// <summary>
    /// Resets the failed attempt counter for an IP address.
    /// Called after a successful authentication.
    /// </summary>
    /// <param name="ipAddress">The IP address to reset.</param>
    Task ResetFailedAttemptsAsync(string ipAddress);

    /// <summary>
    /// Gets information about a blocked IP.
    /// </summary>
    /// <param name="ipAddress">The IP address to query.</param>
    /// <returns>Block information or null if not blocked.</returns>
    Task<BlockedIpInfo?> GetBlockInfoAsync(string ipAddress);

    /// <summary>
    /// Gets all currently blocked IP addresses.
    /// </summary>
    /// <returns>List of blocked IP information.</returns>
    Task<IReadOnlyList<BlockedIpInfo>> GetAllBlockedAsync();
}

/// <summary>
/// Information about a blocked IP address.
/// </summary>
public record BlockedIpInfo
{
    /// <summary>
    /// The blocked IP address.
    /// </summary>
    public required string IpAddress { get; init; }

    /// <summary>
    /// The reason for blocking.
    /// </summary>
    public required string Reason { get; init; }

    /// <summary>
    /// When the IP was blocked.
    /// </summary>
    public required DateTimeOffset BlockedAt { get; init; }

    /// <summary>
    /// When the block expires.
    /// </summary>
    public required DateTimeOffset ExpiresAt { get; init; }

    /// <summary>
    /// Number of failed attempts before blocking.
    /// </summary>
    public int FailedAttempts { get; init; }
}

