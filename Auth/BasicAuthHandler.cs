using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

/// <summary>
/// Custom authentication handler used by ASP.NET Core authentication pipeline.
/// 
/// It is intentionally used as a "fake" authentication system
/// to simulate an unauthenticated user (so that authorization returns 401).
/// </summary>

public class BasicAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public BasicAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        TimeProvider timeProvider)
        : base(options, logger, encoder)
    {
        // TimeProvider is now preferred in modern .NET
    }

    /// Core authentication logic.
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        //simulate anonymous user (force 401 path)
        return Task.FromResult(AuthenticateResult.Fail("No credentials"));
    }
}