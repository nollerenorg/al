using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BcIntegration.Api.Infrastructure;
using BcIntegration.Api.Models;
using Microsoft.Extensions.Options;

namespace BcIntegration.Api.Services;

public sealed class BcClient : IBcClient
{
    private readonly HttpClient _http;
    private readonly BcOptions _options;
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    public BcClient(HttpClient http, IOptions<BcOptions> options)
    {
        _http = http;
        _options = options.Value;
        if (!string.IsNullOrWhiteSpace(_options.Username))
        {
            var b = Convert.ToBase64String(Encoding.UTF8.GetBytes(_options.Username + ":" + _options.Password));
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", b);
        }
    }

    private string ApiBase()
        => $"{_options.BaseUrl.TrimEnd('/')}/api/custom/integration/v1.0";

    private string CompanySegment()
        => string.IsNullOrEmpty(_options.Company) ? string.Empty : $"/companies({_options.Company})"; // If using standard API pattern

    public async Task<ServiceResult<IReadOnlyList<CustomerDto>>> GetCustomersAsync(CancellationToken ct = default)
        {
            try
            {
                var url = ApiBase() + "/customers";
                using var resp = await _http.GetAsync(url, ct);
                var content = await resp.Content.ReadAsStringAsync(ct);
                if (!resp.IsSuccessStatusCode)
                    return ServiceResult<IReadOnlyList<CustomerDto>>.Fail(MapHttpFailure("INTHTTP", content));

                using var doc = JsonDocument.Parse(content);
                if (!doc.RootElement.TryGetProperty("value", out var arr))
                    return ServiceResult<IReadOnlyList<CustomerDto>>.Ok(Array.Empty<CustomerDto>());
                var list = new List<CustomerDto>();
                foreach (var el in arr.EnumerateArray())
                {
                    list.Add(new CustomerDto(
                        el.GetProperty("id").GetString()!,
                        el.GetProperty("number").GetString() ?? string.Empty,
                        el.GetProperty("displayName").GetString() ?? string.Empty,
                        el.TryGetProperty("city", out var city) ? city.GetString() : null,
                        el.TryGetProperty("country", out var country) ? country.GetString() : null,
                        el.TryGetProperty("balanceDue", out var balance) ? balance.GetDecimal() : null,
                        el.TryGetProperty("currencyCode", out var ccy) ? ccy.GetString() : null
                    ));
                }
                return ServiceResult<IReadOnlyList<CustomerDto>>.Ok(list);
            }
            catch (Exception ex)
            {
                return ServiceResult<IReadOnlyList<CustomerDto>>.Fail(new BcError("INTEXC", ex.Message));
            }
        }

    public async Task<ServiceResult<BatchJournalResponse>> PostBatchJournalAsync(BatchJournalRequest request, CancellationToken ct = default)
        {
            try
            {
                var url = ApiBase() + "/codeunits/INTBatchJournalService/ProcessJournalLines";
                var json = JsonSerializer.Serialize(new { lines = request.Lines }, _json);
                using var resp = await _http.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"), ct);
                var content = await resp.Content.ReadAsStringAsync(ct);
                if (!resp.IsSuccessStatusCode)
                    return ServiceResult<BatchJournalResponse>.Fail(MapHttpFailure("INTHTTP", content));

                using var doc = JsonDocument.Parse(content);
                var results = new List<BatchJournalResult>();
                if (doc.RootElement.TryGetProperty("lines", out var linesEl))
                {
                    foreach (var line in linesEl.EnumerateArray())
                    {
                        string status = line.GetProperty("status").GetString() ?? "Unknown";
                        string? id = line.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
                        int? lineNo = line.TryGetProperty("lineNo", out var lnEl) ? lnEl.GetInt32() : null;
                        string? errorCode = null; string? errorMessage = null; string? corrId = null; string? field = null;
                        if (status == "Error" && line.TryGetProperty("error", out var errEl))
                        {
                            if (errEl.TryGetProperty("code", out var c)) errorCode = c.GetString();
                            if (errEl.TryGetProperty("json", out var j))
                            {
                                if (j.ValueKind == JsonValueKind.String)
                                {
                                    var embedded = JsonDocument.Parse(j.GetString()!);
                                    ExtractEmbeddedError(embedded.RootElement, ref errorMessage, ref corrId, ref field);
                                }
                                else
                                {
                                    ExtractEmbeddedError(j, ref errorMessage, ref corrId, ref field);
                                }
                            }
                        }
                        results.Add(new BatchJournalResult(
                            line.GetProperty("index").GetInt32(),
                            status,
                            id,
                            lineNo,
                            errorCode,
                            errorMessage,
                            corrId,
                            field
                        ));
                    }
                }
                return ServiceResult<BatchJournalResponse>.Ok(new BatchJournalResponse(results));
            }
            catch (Exception ex)
            {
                return ServiceResult<BatchJournalResponse>.Fail(new BcError("INTEXC", ex.Message));
            }
        }

    public async Task<ServiceResult<IReadOnlyList<JournalDto>>> GetJournalsAsync(CancellationToken ct = default)
        => await GetListAsync("/journals", el => new JournalDto(
            el.GetProperty("id").GetString()!,
            el.GetProperty("code").GetString() ?? string.Empty,
            el.TryGetProperty("description", out var d) ? d.GetString() : null,
            el.TryGetProperty("templateName", out var t) ? t.GetString() : null));

    public async Task<ServiceResult<IReadOnlyList<JournalLineDto>>> GetJournalLinesAsync(string? journalTemplate = null, string? journalBatch = null, CancellationToken ct = default)
    {
        var filterParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(journalTemplate)) filterParts.Add($"journalTemplateName eq '{journalTemplate}'");
        if (!string.IsNullOrWhiteSpace(journalBatch)) filterParts.Add($"journalBatchName eq '{journalBatch}'");
        var query = filterParts.Count > 0 ? $"?$filter={Uri.EscapeDataString(string.Join(" and ", filterParts))}" : string.Empty;
        return await GetListAsync($"/journalLines{query}", el => new JournalLineDto(
            el.GetProperty("id").GetString()!,
            el.GetProperty("journalTemplateName").GetString() ?? string.Empty,
            el.GetProperty("journalBatchName").GetString() ?? string.Empty,
            el.TryGetProperty("lineNo", out var ln) ? ln.GetInt32() : 0,
            el.TryGetProperty("accountType", out var at) ? at.GetString() : null,
            el.TryGetProperty("accountNo", out var an) ? an.GetString() : null,
            el.TryGetProperty("documentNo", out var dn) ? dn.GetString() : null,
            el.TryGetProperty("postingDate", out var pd) ? pd.GetDateTime() : null,
            el.TryGetProperty("amount", out var am) ? am.GetDecimal() : 0m,
            el.TryGetProperty("description", out var desc) ? desc.GetString() : null,
            el.TryGetProperty("balAccountNo", out var bal) ? bal.GetString() : null));
    }

    public async Task<ServiceResult<IReadOnlyList<AccountDto>>> GetAccountsAsync(CancellationToken ct = default)
        => await GetListAsync("/accounts", el => new AccountDto(
            el.GetProperty("id").GetString()!,
            el.GetProperty("number").GetString() ?? string.Empty,
            el.GetProperty("name").GetString() ?? string.Empty,
            el.TryGetProperty("accountType", out var at) ? at.GetString() : null,
            el.TryGetProperty("directPosting", out var dp) && dp.GetBoolean(),
            el.TryGetProperty("blocked", out var b) && b.GetBoolean(),
            el.TryGetProperty("incomeBalance", out var ib) ? ib.GetString() : null));

    public async Task<ServiceResult<IReadOnlyList<CurrencyDto>>> GetCurrenciesAsync(CancellationToken ct = default)
        => await GetListAsync("/currencies", el => new CurrencyDto(
            el.GetProperty("id").GetString()!,
            el.GetProperty("code").GetString() ?? string.Empty,
            el.TryGetProperty("description", out var d) ? d.GetString() : null,
            el.TryGetProperty("amountRoundingPrecision", out var arp) ? arp.GetDecimal() : null,
            el.TryGetProperty("invoiceRoundingPrecision", out var irp) ? irp.GetDecimal() : null));

    public async Task<ServiceResult<IReadOnlyList<BankAccountDto>>> GetBankAccountsAsync(CancellationToken ct = default)
        => await GetListAsync("/bankAccounts", el => new BankAccountDto(
            el.GetProperty("id").GetString()!,
            el.GetProperty("number").GetString() ?? string.Empty,
            el.GetProperty("name").GetString() ?? string.Empty,
            el.TryGetProperty("currencyCode", out var cc) ? cc.GetString() : null,
            el.TryGetProperty("iban", out var ib) ? ib.GetString() : null,
            el.TryGetProperty("swiftCode", out var sw) ? sw.GetString() : null,
            el.TryGetProperty("blocked", out var b) && b.GetBoolean()));

    public async Task<ServiceResult<IReadOnlyList<DimensionDto>>> GetDimensionsAsync(CancellationToken ct = default)
        => await GetListAsync("/dimensions", el => new DimensionDto(
            el.GetProperty("id").GetString()!,
            el.GetProperty("code").GetString() ?? string.Empty,
            el.GetProperty("name").GetString() ?? string.Empty,
            el.TryGetProperty("blocked", out var bl) && bl.GetBoolean()));

    public async Task<ServiceResult<IReadOnlyList<DimensionValueDto>>> GetDimensionValuesAsync(string? dimensionCode = null, CancellationToken ct = default)
    {
        var query = !string.IsNullOrWhiteSpace(dimensionCode) ? $"?$filter=dimensionCode eq '{dimensionCode}'" : string.Empty;
        return await GetListAsync($"/dimensionValues{query}", el => new DimensionValueDto(
            el.GetProperty("id").GetString()!,
            el.GetProperty("dimensionCode").GetString() ?? string.Empty,
            el.GetProperty("code").GetString() ?? string.Empty,
            el.GetProperty("name").GetString() ?? string.Empty,
            el.TryGetProperty("blocked", out var bl) && bl.GetBoolean()));
    }

    public async Task<ServiceResult<IReadOnlyList<TaxGroupDto>>> GetTaxGroupsAsync(CancellationToken ct = default)
        => await GetListAsync("/taxGroups", el => new TaxGroupDto(
            el.GetProperty("id").GetString()!,
            el.GetProperty("code").GetString() ?? string.Empty,
            el.TryGetProperty("description", out var d) ? d.GetString() : null));

    public async Task<ServiceResult<IReadOnlyList<TaxAreaDto>>> GetTaxAreasAsync(CancellationToken ct = default)
        => await GetListAsync("/taxAreas", el => new TaxAreaDto(
            el.GetProperty("id").GetString()!,
            el.GetProperty("code").GetString() ?? string.Empty,
            el.TryGetProperty("description", out var d) ? d.GetString() : null));

    public async Task<ServiceResult<IReadOnlyList<AttachmentDto>>> GetAttachmentsAsync(CancellationToken ct = default)
        => await GetListAsync("/attachments", el => new AttachmentDto(
            el.GetProperty("id").GetString()!,
            el.GetProperty("fileName").GetString() ?? string.Empty,
            el.GetProperty("mimeType").GetString() ?? string.Empty,
            el.TryGetProperty("attachmentContent", out var c) ? c.GetString() : null));

    public async Task<ServiceResult<AttachmentDto>> UploadAttachmentAsync(CreateAttachmentRequest request, CancellationToken ct = default)
    {
        try
        {
            var url = ApiBase() + "/attachments";
            var payload = JsonSerializer.Serialize(new
            {
                fileName = request.FileName,
                mimeType = request.MimeType,
                attachmentContent = request.Base64Content
            }, _json);
            using var resp = await _http.PostAsync(url, new StringContent(payload, Encoding.UTF8, "application/json"), ct);
            var content = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
                return ServiceResult<AttachmentDto>.Fail(MapHttpFailure("INTHTTP", content));

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;
            var dto = new AttachmentDto(
                root.GetProperty("id").GetString()!,
                root.GetProperty("fileName").GetString() ?? string.Empty,
                root.GetProperty("mimeType").GetString() ?? string.Empty,
                root.TryGetProperty("attachmentContent", out var c) ? c.GetString() : null);
            return ServiceResult<AttachmentDto>.Ok(dto);
        }
        catch (Exception ex)
        {
            return ServiceResult<AttachmentDto>.Fail(new BcError("INTEXC", ex.Message));
        }
    }

    public async Task<ServiceResult<CustomerDto>> CreateCustomerAsync(CustomerUpsertRequest request, CancellationToken ct = default)
    {
        try
        {
            var val = BcValidation.ValidateCustomer(request);
            if (val.Count > 0)
                return ServiceResult<CustomerDto>.Fail(new BcError("INTVAL", string.Join("; ", val)));
            var url = ApiBase() + "/customers";
            var payload = BuildCustomerPayload(request);
            using var resp = await _http.PostAsync(url, payload, ct);
            var content = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
                return ServiceResult<CustomerDto>.Fail(MapHttpFailure("INTHTTP", content));
            return ServiceResult<CustomerDto>.Ok(ParseCustomer(content));
        }
        catch (Exception ex)
        {
            return ServiceResult<CustomerDto>.Fail(new BcError("INTEXC", ex.Message));
        }
    }

    public async Task<ServiceResult<CustomerDto>> UpdateCustomerAsync(string id, CustomerUpsertRequest request, CancellationToken ct = default)
    {
        try
        {
            var val = BcValidation.ValidateCustomer(request);
            if (val.Count > 0)
                return ServiceResult<CustomerDto>.Fail(new BcError("INTVAL", string.Join("; ", val)));
            var url = ApiBase() + $"/customers({id})"; // id is SystemId (GUID) - caller responsibility
            var payload = BuildCustomerPayload(request);
            var method = new HttpMethod("PATCH");
            using var req = new HttpRequestMessage(method, url) { Content = payload };
            req.Headers.Add("If-Match", "*"); // optimistic concurrency override
            using var resp = await _http.SendAsync(req, ct);
            var content = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
                return ServiceResult<CustomerDto>.Fail(MapHttpFailure("INTHTTP", content));
            return ServiceResult<CustomerDto>.Ok(ParseCustomer(content));
        }
        catch (Exception ex)
        {
            return ServiceResult<CustomerDto>.Fail(new BcError("INTEXC", ex.Message));
        }
    }

    public async Task<ServiceResult<CustomerWithMeta>> GetCustomerByIdAsync(string id, CancellationToken ct = default)
    {
        try
        {
            var url = ApiBase() + $"/customers({id})";
            using var resp = await _http.GetAsync(url, ct);
            var content = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
                return ServiceResult<CustomerWithMeta>.Fail(MapHttpFailure("INTHTTP", content));
            var dto = ParseCustomer(content);
            var etag = ExtractEtag(resp, content);
            return ServiceResult<CustomerWithMeta>.Ok(new CustomerWithMeta(dto, etag));
        }
        catch (Exception ex)
        {
            return ServiceResult<CustomerWithMeta>.Fail(new BcError("INTEXC", ex.Message));
        }
    }

    public async Task<ServiceResult<CustomerDto>> UpdateCustomerIfMatchAsync(string id, string etag, CustomerUpsertRequest request, CancellationToken ct = default)
    {
        try
        {
            var val = BcValidation.ValidateCustomer(request);
            if (val.Count > 0)
                return ServiceResult<CustomerDto>.Fail(new BcError("INTVAL", string.Join("; ", val)));
            var url = ApiBase() + $"/customers({id})";
            var payload = BuildCustomerPayload(request);
            var method = new HttpMethod("PATCH");
            using var req = new HttpRequestMessage(method, url) { Content = payload };
            req.Headers.Add("If-Match", etag.StartsWith("\"") ? etag : $"\"{etag}\"");
            using var resp = await _http.SendAsync(req, ct);
            var content = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
                return ServiceResult<CustomerDto>.Fail(MapHttpFailure("INTHTTP", content));
            return ServiceResult<CustomerDto>.Ok(ParseCustomer(content));
        }
        catch (Exception ex)
        {
            return ServiceResult<CustomerDto>.Fail(new BcError("INTEXC", ex.Message));
        }
    }

    public async Task<ServiceResult<CustomerWithMeta>> CreateCustomerWithMetaAsync(CustomerUpsertRequest request, CancellationToken ct = default)
    {
        try
        {
            var baseResult = await CreateCustomerAsync(request, ct);
            if (!baseResult.Success)
                return ServiceResult<CustomerWithMeta>.Fail(baseResult.Error!);
            // Need to re-fetch to get reliable ETag (some BC APIs may not return it on POST body) – optional optimization
            var refetch = await GetCustomerByIdAsync(baseResult.Value!.Id, ct);
            return refetch;
        }
        catch (Exception ex)
        {
            return ServiceResult<CustomerWithMeta>.Fail(new BcError("INTEXC", ex.Message));
        }
    }

    public async Task<ServiceResult<CustomerWithMeta>> UpdateCustomerWithMetaAsync(string id, CustomerUpsertRequest request, CancellationToken ct = default)
    {
        var update = await UpdateCustomerAsync(id, request, ct);
        if (!update.Success)
            return ServiceResult<CustomerWithMeta>.Fail(update.Error!);
        return await GetCustomerByIdAsync(id, ct);
    }

    public async Task<ServiceResult<CustomerWithMeta>> UpdateCustomerIfMatchWithMetaAsync(string id, string etag, CustomerUpsertRequest request, CancellationToken ct = default)
    {
        var update = await UpdateCustomerIfMatchAsync(id, etag, request, ct);
        if (!update.Success)
            return ServiceResult<CustomerWithMeta>.Fail(update.Error!);
        return await GetCustomerByIdAsync(id, ct);
    }

    public IReadOnlyList<string> ValidateCustomer(CustomerUpsertRequest request)
        => BcValidation.ValidateCustomer(request);

    private StringContent BuildCustomerPayload(CustomerUpsertRequest r)
    {
        var obj = new Dictionary<string, object?>
        {
            ["number"] = r.Number,
            ["displayName"] = r.DisplayName,
            ["addressLine1"] = r.AddressLine1,
            ["addressLine2"] = r.AddressLine2,
            ["city"] = r.City,
            ["country"] = r.Country,
            ["postalCode"] = r.PostalCode,
            ["phoneNumber"] = r.PhoneNumber,
            ["email"] = r.Email,
            ["salespersonCode"] = r.SalespersonCode,
            ["currencyCode"] = r.CurrencyCode,
            ["paymentTermsId"] = r.PaymentTermsId,
            ["paymentMethodId"] = r.PaymentMethodId,
            ["shipmentMethodId"] = r.ShipmentMethodId,
            ["creditLimit"] = r.CreditLimit,
            ["taxRegistrationNumber"] = r.TaxRegistrationNumber,
            ["blocked"] = r.Blocked
        };
        // remove nulls
        var filtered = obj.Where(kv => kv.Value != null).ToDictionary(kv => kv.Key, kv => kv.Value);
        var json = JsonSerializer.Serialize(filtered, _json);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private CustomerDto ParseCustomer(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var el = doc.RootElement;
        return new CustomerDto(
            el.GetProperty("id").GetString()!,
            el.GetProperty("number").GetString() ?? string.Empty,
            el.GetProperty("displayName").GetString() ?? string.Empty,
            el.TryGetProperty("city", out var city) ? city.GetString() : null,
            el.TryGetProperty("country", out var country) ? country.GetString() : null,
            el.TryGetProperty("balanceDue", out var balance) ? balance.GetDecimal() : null,
            el.TryGetProperty("currencyCode", out var ccy) ? ccy.GetString() : null
        );
    }

    private static string? ExtractEtag(HttpResponseMessage resp, string body)
    {
        var hdr = resp.Headers.ETag?.Tag?.Trim('"');
        if (!string.IsNullOrEmpty(hdr)) return hdr;
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("@odata.etag", out var et))
            {
                var v = et.GetString();
                if (!string.IsNullOrWhiteSpace(v)) return v.Trim('"');
            }
        }
        catch { }
        return null;
    }

    // Generic list fetch helper
    private async Task<ServiceResult<IReadOnlyList<T>>> GetListAsync<T>(string relative, Func<JsonElement, T> projector, CancellationToken ct = default)
    {
        try
        {
            var url = ApiBase() + relative;
            using var resp = await _http.GetAsync(url, ct);
            var content = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
                return ServiceResult<IReadOnlyList<T>>.Fail(MapHttpFailure("INTHTTP", content));
            using var doc = JsonDocument.Parse(content);
            var list = new List<T>();
            if (doc.RootElement.TryGetProperty("value", out var arr))
            {
                foreach (var el in arr.EnumerateArray())
                    list.Add(projector(el));
            }
            else if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var el in doc.RootElement.EnumerateArray())
                    list.Add(projector(el));
            }
            return ServiceResult<IReadOnlyList<T>>.Ok(list);
        }
        catch (Exception ex)
        {
            return ServiceResult<IReadOnlyList<T>>.Fail(new BcError("INTEXC", ex.Message));
        }
    }

    private static void ExtractEmbeddedError(JsonElement el, ref string? message, ref string? corrId, ref string? field)
    {
        if (el.TryGetProperty("message", out var m)) message = m.GetString();
        if (el.TryGetProperty("correlationId", out var c)) corrId = c.GetString();
        if (el.TryGetProperty("field", out var f)) field = f.GetString();
    }

    private static BcError MapHttpFailure(string defaultCode, string body)
    {
        // Attempt parse of CODE|JSON from body if present; fallback to raw
        if (!string.IsNullOrWhiteSpace(body))
        {
            var maybe = body.Trim();
            if (maybe.Contains('|'))
                return BcErrorParser.Parse(maybe);
            // look for {"code":"X" pattern
            try
            {
                using var doc = JsonDocument.Parse(maybe);
                if (doc.RootElement.TryGetProperty("code", out var c))
                {
                    var msg = doc.RootElement.TryGetProperty("message", out var m) ? m.GetString() ?? maybe : maybe;
                    return new BcError(c.GetString() ?? defaultCode, msg);
                }
            }
            catch { }
            return new BcError(defaultCode, maybe);
        }
        return new BcError(defaultCode, "HTTP failure");
    }
}
