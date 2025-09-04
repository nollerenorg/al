using BcIntegration.Api.Models;
using BcIntegration.Api.Services;

namespace Microsoft.AspNetCore.Builder;

public static class Endpoints
{
    public static IEndpointRouteBuilder MapBcCustomerEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/customers", async (IBcClient client, CancellationToken ct) =>
        {
            var items = await client.GetCustomersAsync(ct);
            return Results.Ok(items);
        });
        return app;
    }

    public static IEndpointRouteBuilder MapBcBatchJournalEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/journalLines/batch", async (BatchJournalRequest req, IBcClient client, CancellationToken ct) =>
        {
            if (req.Lines.Count == 0)
                return Results.BadRequest(new { message = "No lines provided" });
            var result = await client.PostBatchJournalAsync(req, ct);
            return Results.Ok(result);
        });
        return app;
    }
}
