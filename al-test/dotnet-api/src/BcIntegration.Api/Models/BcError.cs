namespace BcIntegration.Api.Models;

/// <summary>
/// Normalized Business Central integration error extracted from CODE|JSON pattern.
/// </summary>
public sealed record BcError(
    string Code,
    string Message,
    string? CorrelationId = null,
    string? Field = null
);

public static class BcErrorParser
{
    /// <summary>
    /// Parse raw error text in the form CODE|{json}. Falls back gracefully if format unexpected.
    /// </summary>
    public static BcError Parse(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return new("INT0000", "Unknown error");
        var sep = raw.IndexOf('|');
        if (sep < 0) return new("INT9999", raw.Trim());
        var code = raw[..sep];
        var json = raw[(sep + 1)..];
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;
            string msg = root.TryGetProperty("message", out var m) ? m.GetString() ?? string.Empty : string.Empty;
            string? corr = root.TryGetProperty("correlationId", out var c) ? c.GetString() : null;
            string? field = root.TryGetProperty("field", out var f) ? f.GetString() : null;
            if (string.IsNullOrWhiteSpace(msg)) msg = json; // fallback
            return new(code, msg, corr, field);
        }
        catch
        {
            return new(code, json);
        }
    }
}
