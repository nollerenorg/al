using System.Text.Json.Serialization;

namespace BcIntegration.Api.Models;

public sealed class BatchJournalRequest
{
    [JsonPropertyName("lines")] public List<BatchJournalLine> Lines { get; set; } = new();
}

public sealed class BatchJournalLine
{
    public string JournalTemplateName { get; set; } = string.Empty;
    public string JournalBatchName { get; set; } = string.Empty;
    public string? AccountType { get; set; }
    public string? AccountNo { get; set; }
    public string? DocumentNo { get; set; }
    public DateTime? PostingDate { get; set; }
    public decimal? Amount { get; set; }
    public string? Description { get; set; }
    public string? BalAccountNo { get; set; }
}

public sealed record BatchJournalResponse(List<BatchJournalResult> Lines);

public sealed record BatchJournalResult(
    int Index,
    string Status,
    string? Id = null,
    int? LineNo = null,
    string? ErrorCode = null,
    string? ErrorMessage = null,
    string? CorrelationId = null,
    string? Field = null
);
