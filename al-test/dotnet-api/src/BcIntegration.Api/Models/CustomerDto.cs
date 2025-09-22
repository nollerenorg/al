namespace BcIntegration.Api.Models;

public sealed record CustomerDto(
    string Id,
    string Number,
    string DisplayName,
    string? City,
    string? Country,
    decimal? BalanceDue,
    string? CurrencyCode,
    string? VatBusPostingGroup,
    string? GenBusPostingGroup,
    string? TaxAreaCode,
    bool? TaxLiable,
    string? TaxRegistrationNumber
);
