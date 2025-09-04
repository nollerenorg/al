using BcIntegration.Api.Models;

namespace BcIntegration.Api.Services;

public interface IBcClient
{
    Task<ServiceResult<IReadOnlyList<CustomerDto>>> GetCustomersAsync(CancellationToken ct = default);
    Task<ServiceResult<BatchJournalResponse>> PostBatchJournalAsync(BatchJournalRequest request, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<JournalDto>>> GetJournalsAsync(CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<JournalLineDto>>> GetJournalLinesAsync(string? journalTemplate = null, string? journalBatch = null, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<AccountDto>>> GetAccountsAsync(CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<CurrencyDto>>> GetCurrenciesAsync(CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<BankAccountDto>>> GetBankAccountsAsync(CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<DimensionDto>>> GetDimensionsAsync(CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<DimensionValueDto>>> GetDimensionValuesAsync(string? dimensionCode = null, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<TaxGroupDto>>> GetTaxGroupsAsync(CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<TaxAreaDto>>> GetTaxAreasAsync(CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<AttachmentDto>>> GetAttachmentsAsync(CancellationToken ct = default);
    Task<ServiceResult<AttachmentDto>> UploadAttachmentAsync(CreateAttachmentRequest request, CancellationToken ct = default);
    Task<ServiceResult<CustomerDto>> CreateCustomerAsync(CustomerUpsertRequest request, CancellationToken ct = default);
    Task<ServiceResult<CustomerDto>> UpdateCustomerAsync(string id, CustomerUpsertRequest request, CancellationToken ct = default);
    Task<ServiceResult<CustomerWithMeta>> GetCustomerByIdAsync(string id, CancellationToken ct = default);
    Task<ServiceResult<CustomerDto>> UpdateCustomerIfMatchAsync(string id, string etag, CustomerUpsertRequest request, CancellationToken ct = default);
    IReadOnlyList<string> ValidateCustomer(CustomerUpsertRequest request);
    Task<ServiceResult<CustomerWithMeta>> CreateCustomerWithMetaAsync(CustomerUpsertRequest request, CancellationToken ct = default);
    Task<ServiceResult<CustomerWithMeta>> UpdateCustomerWithMetaAsync(string id, CustomerUpsertRequest request, CancellationToken ct = default);
    Task<ServiceResult<CustomerWithMeta>> UpdateCustomerIfMatchWithMetaAsync(string id, string etag, CustomerUpsertRequest request, CancellationToken ct = default);
}

public sealed record ServiceResult<T>(T? Value, bool Success, BcError? Error = null)
{
    public static ServiceResult<T> Ok(T value) => new(value, true, null);
    public static ServiceResult<T> Fail(BcError error) => new(default, false, error);
}

public static class BcIntegrationServiceCollectionExtensions
{
    public static IServiceCollection AddBusinessCentralIntegration(this IServiceCollection services, Action<BcOptions> configure)
    {
        services.Configure(configure);
        services.AddHttpClient<IBcClient, BcClient>()
            .AddPolicyHandler(PollyPolicies.StandardRetry());
        return services;
    }
}
