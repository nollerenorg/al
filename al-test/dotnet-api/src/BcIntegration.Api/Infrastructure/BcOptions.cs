namespace BcIntegration.Api.Infrastructure;

public sealed class BcOptions
{
    public string BaseUrl { get; set; } = string.Empty; // https://server:port/BC
    public string Tenant { get; set; } = string.Empty;  // (OnPrem blank)
    public string Environment { get; set; } = string.Empty; // (Sandbox/Production for SaaS)
    public string Company { get; set; } = string.Empty; // Company name URL encoded
    public string Username { get; set; } = string.Empty; // Or use OAuth
    public string Password { get; set; } = string.Empty; // Or an app password
}
