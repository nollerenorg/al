using BcIntegration.Api.Services;
using BcIntegration.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<IBcClient, BcClient>()
    .AddPolicyHandler(PollyPolicies.StandardRetry());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<BcOptions>(builder.Configuration.GetSection("BusinessCentral"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => new { status = "ok" });

app.MapBcCustomerEndpoints();
app.MapBcBatchJournalEndpoints();

app.Run();
