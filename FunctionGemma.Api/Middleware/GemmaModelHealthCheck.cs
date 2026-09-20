using Microsoft.Extensions.Diagnostics.HealthChecks;
using FunctionGemma.Api.Services;

namespace FunctionGemma.Api.Middleware
{
    public class GemmaModelHealthCheck : IHealthCheck
    {
        private readonly GemmaModelFactory _factory;

        public GemmaModelHealthCheck(GemmaModelFactory factory)
        {
            _factory = factory;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
        {
            var result = _factory.Model is not null && _factory.Tokenizer is not null
                ? HealthCheckResult.Healthy("Model and tokenizer are loaded.")
                : HealthCheckResult.Unhealthy("Model or tokenizer is not loaded.");

            return Task.FromResult(result);
        }
    }
}