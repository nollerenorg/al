# .NET Wrapper API for Business Central Custom Integration

This ASP.NET Core 8 minimal API wraps the custom Business Central AL APIs (publisher `custom`, group `integration`) and offers simplified DTOs plus resiliency.

## Features
- Customers endpoint (aggregated projection)
- Batch journal line creation passthrough (/journalLines/batch)
- Polly retry for transient failures
- Basic Auth (demo) – swap for OAuth2 in production
- Swagger UI in Development

## Project Structure
```
src/BcIntegration.Api/
  Program.cs
  BcIntegration.Api.csproj
  Infrastructure/
    BcOptions.cs
    PollyPolicies.cs
  Models/
    CustomerDto.cs
    BatchJournalRequest.cs
  Services/
    IBcClient.cs
    BcClient.cs
    Endpoints.cs
```

## Configuration (appsettings.json)
```json
{
  "BusinessCentral": {
    "BaseUrl": "https://bcserver/BC",
    "Company": "CRONUS%20Danmark%20A/S",
    "Username": "BCUSER",
    "Password": "Secret" 
  }
}
```

## Run
```
dotnet run --project src/BcIntegration.Api/BcIntegration.Api.csproj
```
Swagger: https://localhost:5001/swagger

## Calling Examples
GET /customers
POST /journalLines/batch
```json
{
  "lines": [
    {"journalTemplateName":"GENERAL","journalBatchName":"DEFAULT","accountType":"G/L Account","accountNo":"1000","documentNo":"API1","postingDate":"2025-09-04","amount":123.45},
    {"journalTemplateName":"GENERAL","journalBatchName":"DEFAULT","accountType":"G/L Account","accountNo":"1000","documentNo":"API1","postingDate":"2025-09-04","amount":0}
  ]
}
```

## Error Handling
BC validation errors (CODE|JSON) are parsed – batch endpoint surfaces fields:
- ErrorCode, ErrorMessage, CorrelationId, Field

## Next Steps
- Add OAuth (Azure AD) – use On-Behalf-Of or client credentials
- Cache lookups (currencies, dimensions) with MemoryCache
- Add additional projection endpoints (accounts, attachments)
- Typed enum mapping for account types
- Serilog + OpenTelemetry tracing with correlation ids
