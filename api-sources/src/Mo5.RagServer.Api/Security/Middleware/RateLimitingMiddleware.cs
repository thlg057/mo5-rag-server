using System.Collections.Concurrent;
using System.Net;
using Microsoft.Extensions.Options;
using Mo5.RagServer.Api.Security.Models;

namespace Mo5.RagServer.Api.Security.Middleware;

/// <summary>
/// Middleware that limits the number of requests per IP address.
/// Uses a sliding window algorithm for rate limiting.
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly ConcurrentDictionary<string, RateLimitEntry> _requestCounts = new();

    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IOptions<RateLimitSettings> settings)
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

        var now = DateTimeOffset.UtcNow;
        var windowStart = now.AddMinutes(-1);

        var entry = _requestCounts.AddOrUpdate(
            ipAddress,
            _ => new RateLimitEntry { Requests = new List<DateTimeOffset> { now } },
            (_, existing) =>
            {
                // Remove requests outside the window
                existing.Requests.RemoveAll(r => r < windowStart);
                existing.Requests.Add(now);
                return existing;
            });

        var requestCount = entry.Requests.Count;

        // Add rate limit headers
        context.Response.Headers["X-RateLimit-Limit"] = config.RequestsPerMinute.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = Math.Max(0, config.RequestsPerMinute - requestCount).ToString();
        context.Response.Headers["X-RateLimit-Reset"] = windowStart.AddMinutes(1).ToUnixTimeSeconds().ToString();

        if (requestCount > config.RequestsPerMinute)
        {
            _logger.LogWarning(
                "Rate limit exceeded for IP {IpAddress}. Requests: {RequestCount}/{Limit} in the last minute. Path: {Path}",
                ipAddress, requestCount, config.RequestsPerMinute, context.Request.Path);

            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.Headers["Retry-After"] = "60";

            await context.Response.WriteAsJsonAsync(new
            {
                error = "Rate limit exceeded",
                limit = config.RequestsPerMinute,
                window = "1 minute",
                retryAfter = 60,
                message = $"You have exceeded the rate limit of {config.RequestsPerMinute} requests per minute. Please wait before making more requests."
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
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return context.Connection.RemoteIpAddress?.ToString();
    }

    private class RateLimitEntry
    {
        public List<DateTimeOffset> Requests { get; set; } = new();
    }
}

