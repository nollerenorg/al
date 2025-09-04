using Polly;
using Polly.Extensions.Http;
using System.Net;

namespace BcIntegration.Api.Infrastructure;

public static class PollyPolicies
{
    public static IAsyncPolicy<HttpResponseMessage> StandardRetry()
        => HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(r => r.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt)));
}
