using System.Net;
using Microsoft.Extensions.Options;
using Mo5.RagServer.Api.Security.Models;
using Mo5.RagServer.Api.Security.Services;

namespace Mo5.RagServer.Api.Security.Middleware;

/// <summary>
/// Middleware that blocks IP addresses that have exceeded the maximum number of failed authentication attempts.
/// </summary>
public class AntiBruteForceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AntiBruteForceMiddleware> _logger;

    public AntiBruteForceMiddleware(
        RequestDelegate next,
        ILogger<AntiBruteForceMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IBlockedIpService blockedIpService,
        IOptions<AntiBruteForceSettings> settings)
    {
        var config = settings.Value;
        
        if (!config.Enabled)
        {
            await _next(context);
            return;
        }

        var ipAddress = GetClientIpAddress(context);

        if (string.IsNullOrEmpty(ipAddress))
        {
            await _next(context);
            return;
        }

        // Check if IP is whitelisted
        if (config.WhitelistedIps.Contains(ipAddress))
        {
            await _next(context);
            return;
        }

        // Check if IP is blocked
        if (await blockedIpService.IsBlockedAsync(ipAddress))
        {
            var blockInfo = await blockedIpService.GetBlockInfoAsync(ipAddress);
            
            _logger.LogWarning(
                "Blocked IP {IpAddress} attempted to access {Path}. Block expires at {ExpiresAt}",
                ipAddress, context.Request.Path, blockInfo?.ExpiresAt);

            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            context.Response.Headers["X-Blocked-Reason"] = blockInfo?.Reason ?? "BruteForce";
            context.Response.Headers["X-Blocked-Until"] = blockInfo?.ExpiresAt.ToString("o") ?? "";
            
            if (blockInfo != null)
            {
                var retryAfterSeconds = (int)(blockInfo.ExpiresAt - DateTimeOffset.UtcNow).TotalSeconds;
                context.Response.Headers["Retry-After"] = Math.Max(0, retryAfterSeconds).ToString();
            }

            await context.Response.WriteAsJsonAsync(new
            {
                error = "IP address is temporarily blocked",
                reason = blockInfo?.Reason ?? "TooManyFailedAttempts",
                blockedAt = blockInfo?.BlockedAt,
                expiresAt = blockInfo?.ExpiresAt,
                message = $"Your IP has been blocked due to too many failed authentication attempts. Please try again later."
            });

            return;
        }

        await _next(context);
    }

    private static string? GetClientIpAddress(HttpContext context)
    {
        // Check for forwarded headers (when behind a reverse proxy)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // X-Forwarded-For can contain multiple IPs, take the first one (client IP)
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return context.Connection.RemoteIpAddress?.ToString();
    }
}

