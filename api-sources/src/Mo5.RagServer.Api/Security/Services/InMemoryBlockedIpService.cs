using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using Mo5.RagServer.Api.Security.Models;

namespace Mo5.RagServer.Api.Security.Services;

/// <summary>
/// In-memory implementation of <see cref="IBlockedIpService"/>.
/// Uses ConcurrentDictionary for thread-safe operations.
/// Note: Data is lost on application restart.
/// </summary>
public class InMemoryBlockedIpService : IBlockedIpService
{
    private readonly ConcurrentDictionary<string, FailedAttemptEntry> _failedAttempts = new();
    private readonly ConcurrentDictionary<string, BlockedIpInfo> _blockedIps = new();
    private readonly AntiBruteForceSettings _settings;
    private readonly ILogger<InMemoryBlockedIpService> _logger;

    public InMemoryBlockedIpService(
        IOptions<AntiBruteForceSettings> settings,
        ILogger<InMemoryBlockedIpService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public Task<bool> IsBlockedAsync(string ipAddress)
    {
        if (_blockedIps.TryGetValue(ipAddress, out var blockInfo))
        {
            if (DateTimeOffset.UtcNow < blockInfo.ExpiresAt)
            {
                return Task.FromResult(true);
            }

            // Block has expired, remove it
            _blockedIps.TryRemove(ipAddress, out _);
            _logger.LogInformation("IP {IpAddress} block expired, automatically unblocked", ipAddress);
        }

        return Task.FromResult(false);
    }

    public Task<int> RecordFailedAttemptAsync(string ipAddress)
    {
        var now = DateTimeOffset.UtcNow;
        var windowStart = now.AddMinutes(-_settings.BlockDurationMinutes);

        var entry = _failedAttempts.AddOrUpdate(
            ipAddress,
            _ => new FailedAttemptEntry { Attempts = 1, FirstAttempt = now, LastAttempt = now },
            (_, existing) =>
            {
                // Reset if outside the window
                if (existing.FirstAttempt < windowStart)
                {
                    return new FailedAttemptEntry { Attempts = 1, FirstAttempt = now, LastAttempt = now };
                }

                existing.Attempts++;
                existing.LastAttempt = now;
                return existing;
            });

        _logger.LogWarning(
            "Failed authentication attempt {AttemptNumber}/{MaxAttempts} from IP {IpAddress}",
            entry.Attempts, _settings.MaxFailedAttempts, ipAddress);

        return Task.FromResult(entry.Attempts);
    }

    public Task BlockAsync(string ipAddress, string reason)
    {
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddMinutes(_settings.BlockDurationMinutes);

        _failedAttempts.TryGetValue(ipAddress, out var attempts);

        var blockInfo = new BlockedIpInfo
        {
            IpAddress = ipAddress,
            Reason = reason,
            BlockedAt = now,
            ExpiresAt = expiresAt,
            FailedAttempts = attempts?.Attempts ?? 0
        };

        _blockedIps[ipAddress] = blockInfo;

        _logger.LogError(
            "IP {IpAddress} blocked for {Duration} minutes. Reason: {Reason}. Failed attempts: {FailedAttempts}",
            ipAddress, _settings.BlockDurationMinutes, reason, blockInfo.FailedAttempts);

        return Task.CompletedTask;
    }

    public Task UnblockAsync(string ipAddress)
    {
        _blockedIps.TryRemove(ipAddress, out _);
        _failedAttempts.TryRemove(ipAddress, out _);

        _logger.LogInformation("IP {IpAddress} manually unblocked", ipAddress);

        return Task.CompletedTask;
    }

    public Task ResetFailedAttemptsAsync(string ipAddress)
    {
        _failedAttempts.TryRemove(ipAddress, out _);
        return Task.CompletedTask;
    }

    public Task<BlockedIpInfo?> GetBlockInfoAsync(string ipAddress)
    {
        if (_blockedIps.TryGetValue(ipAddress, out var blockInfo))
        {
            if (DateTimeOffset.UtcNow < blockInfo.ExpiresAt)
            {
                return Task.FromResult<BlockedIpInfo?>(blockInfo);
            }

            // Expired
            _blockedIps.TryRemove(ipAddress, out _);
        }

        return Task.FromResult<BlockedIpInfo?>(null);
    }

    public Task<IReadOnlyList<BlockedIpInfo>> GetAllBlockedAsync()
    {
        var now = DateTimeOffset.UtcNow;
        var activeBlocks = _blockedIps.Values
            .Where(b => b.ExpiresAt > now)
            .ToList();

        // Clean up expired entries
        foreach (var expired in _blockedIps.Where(kvp => kvp.Value.ExpiresAt <= now).ToList())
        {
            _blockedIps.TryRemove(expired.Key, out _);
        }

        return Task.FromResult<IReadOnlyList<BlockedIpInfo>>(activeBlocks);
    }

    private class FailedAttemptEntry
    {
        public int Attempts { get; set; }
        public DateTimeOffset FirstAttempt { get; set; }
        public DateTimeOffset LastAttempt { get; set; }
    }
}

