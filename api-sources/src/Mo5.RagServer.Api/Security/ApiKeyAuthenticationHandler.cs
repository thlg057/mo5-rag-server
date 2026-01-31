using System.Security.Claims;
using System.Text.Encodings.Web;
using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Mo5.RagServer.Api.Security;

/// <summary>
/// Authentication handler that validates requests using an API key provided
/// via the HTTP header `X-Api-Key`.
/// </summary>
public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    /// <summary>Name of the HTTP header that carries the API key.</summary>
    public const string ApiKeyHeaderName = "X-Api-Key";

    #pragma warning disable CS0618
    /// <summary>Constructs the handler.</summary>
    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        Microsoft.AspNetCore.Authentication.ISystemClock clock) : base(options, logger, encoder, clock)
    {
    }
    #pragma warning restore CS0618

    /// <summary>
    /// Validates the `X-Api-Key` header against configuration and returns
    /// an authenticated principal when it matches.
    /// </summary>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var providedApiKey))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var configuration = Context.RequestServices.GetService(typeof(Microsoft.Extensions.Configuration.IConfiguration)) as Microsoft.Extensions.Configuration.IConfiguration;
        var configuredKey = configuration?.GetValue<string>("ApiKeySettings:Key");

        if (string.IsNullOrEmpty(configuredKey) || !string.Equals(providedApiKey, configuredKey, StringComparison.Ordinal))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API Key provided."));
        }

        var claims = new[] { new Claim(ClaimTypes.Name, "ApiKeyUser") };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
