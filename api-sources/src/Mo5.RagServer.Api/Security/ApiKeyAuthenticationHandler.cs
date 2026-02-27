using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Mo5.RagServer.Api.Security.Models;
using Mo5.RagServer.Api.Security.Services;

namespace Mo5.RagServer.Api.Security;

/// <summary>
/// Authentication handler that validates requests using an API key provided
/// via the HTTP header `X-Api-Key`.
/// Integrates with the anti-brute-force system to track failed attempts.
/// </summary>
public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    /// <summary>Name of the HTTP header that carries the API key.</summary>
    public const string ApiKeyHeaderName = "X-Api-Key";

    private readonly IBlockedIpService _blockedIpService;
    private readonly AntiBruteForceSettings _antiBruteForceSettings;

    #pragma warning disable CS0618
    /// <summary>Constructs the handler.</summary>
    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        Microsoft.AspNetCore.Authentication.ISystemClock clock,
        IBlockedIpService blockedIpService,
        IOptions<AntiBruteForceSettings> antiBruteForceSettings) : base(options, logger, encoder, clock)
    {
        _blockedIpService = blockedIpService;
        _antiBruteForceSettings = antiBruteForceSettings.Value;
    }
    #pragma warning restore CS0618

    /// <summary>
    /// Validates the `X-Api-Key` header against configuration and returns
    /// an authenticated principal when it matches.
    /// Records failed attempts for brute-force protection.
    /// </summary>
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var ipAddress = GetClientIpAddress();

        if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var providedApiKey))
        {
            return AuthenticateResult.NoResult();
        }

        var configuration = Context.RequestServices.GetService(typeof(Microsoft.Extensions.Configuration.IConfiguration)) as Microsoft.Extensions.Configuration.IConfiguration;
        var configuredKey = configuration?.GetValue<string>("ApiKeySettings:Key");

        if (string.IsNullOrEmpty(configuredKey) || !string.Equals(providedApiKey, configuredKey, StringComparison.Ordinal))
        {
            // Record failed attempt for brute-force protection
            if (_antiBruteForceSettings.Enabled && !string.IsNullOrEmpty(ipAddress))
            {
                var failedAttempts = await _blockedIpService.RecordFailedAttemptAsync(ipAddress);

                Logger.LogWarning(
                    "Invalid API key attempt from IP {IpAddress}. Attempt {AttemptNumber}/{MaxAttempts}",
                    ipAddress, failedAttempts, _antiBruteForceSettings.MaxFailedAttempts);

                if (failedAttempts >= _antiBruteForceSettings.MaxFailedAttempts)
                {
                    await _blockedIpService.BlockAsync(ipAddress, "BruteForce");
                    Logger.LogError(
                        "IP {IpAddress} blocked after {FailedAttempts} failed authentication attempts",
                        ipAddress, failedAttempts);
                }
            }

            return AuthenticateResult.Fail("Invalid API Key provided.");
        }

        // Successful authentication - reset failed attempts
        if (!string.IsNullOrEmpty(ipAddress))
        {
            await _blockedIpService.ResetFailedAttemptsAsync(ipAddress);
        }

        var claims = new[] { new Claim(ClaimTypes.Name, "ApiKeyUser") };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }

    private string? GetClientIpAddress()
    {
        // Check for forwarded headers (when behind a reverse proxy)
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return Context.Connection.RemoteIpAddress?.ToString();
    }
}
