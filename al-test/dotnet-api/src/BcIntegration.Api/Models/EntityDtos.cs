namespace BcIntegration.Api.Models;

public sealed record JournalDto(string Id, string Code, string? Description, string? TemplateName);
public sealed record JournalLineDto(string Id, string JournalTemplateName, string JournalBatchName, int LineNo, string? AccountType, string? AccountNo, string? DocumentNo, DateTime? PostingDate, decimal? Amount, string? Description, string? BalAccountNo);
public sealed record AccountDto(string Id, string Number, string Name, string? AccountType, bool DirectPosting, bool Blocked, string? IncomeBalance);
public sealed record CurrencyDto(string Id, string Code, string? Description, decimal? AmountRoundingPrecision, decimal? InvoiceRoundingPrecision);
public sealed record BankAccountDto(string Id, string Number, string Name, string? CurrencyCode, string? Iban, string? SwiftCode, bool Blocked);
public sealed record DimensionDto(string Id, string Code, string Name, bool Blocked);
public sealed record DimensionValueDto(string Id, string DimensionCode, string Code, string Name, bool Blocked);
public sealed record TaxGroupDto(string Id, string Code, string? Description);
public sealed record TaxAreaDto(string Id, string Code, string? Description);
public sealed record AttachmentDto(string Id, string FileName, string MimeType, string? Base64Content);

public sealed class CreateAttachmentRequest
{
    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public string Base64Content { get; set; } = string.Empty;
}

public sealed class CustomerUpsertRequest
{
    public string Number { get; set; } = string.Empty; // "No." in BC
    public string DisplayName { get; set; } = string.Empty;
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? SalespersonCode { get; set; }
    public string? CurrencyCode { get; set; }
    public string? PaymentTermsId { get; set; }
    public string? PaymentMethodId { get; set; }
    public string? ShipmentMethodId { get; set; }
    public decimal? CreditLimit { get; set; }
    public string? TaxRegistrationNumber { get; set; }
    public bool? Blocked { get; set; }
}

public sealed record CustomerWithMeta(CustomerDto Customer, string? ETag);

public static class BcValidation
{
    public static IReadOnlyList<string> ValidateCustomer(CustomerUpsertRequest r)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(r.Number)) errors.Add("Number is required");
        if (string.IsNullOrWhiteSpace(r.DisplayName)) errors.Add("DisplayName is required");
        if (r.Number?.Length > 20) errors.Add("Number exceeds max length (20)");
        if (r.DisplayName?.Length > 100) errors.Add("DisplayName exceeds max length (100)");
        // Additional domain checks could be added (e.g. format of email) without server roundtrip.
        return errors;
    }
}
