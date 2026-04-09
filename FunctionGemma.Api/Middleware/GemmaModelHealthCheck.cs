using Microsoft.Extensions.Diagnostics.HealthChecks;
using FunctionGemma.Api.Interfaces;

namespace FunctionGemma.Api.Middleware
{
    public class GemmaModelHealthCheck : IHealthCheck
    {
        private readonly IGemmaOnnxService _service;

        public GemmaModelHealthCheck(IGemmaOnnxService service)
        {
            _service = service;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
        {
            try
            {
                // Test 1: Simple prompt
                string response1 = await _service.GenerateTokensAsync("<start>hello<end>", ct);
                
                // Test 2: Different prompt (or same prompt to check for variety/determinism)
                string response2 = await _service.GenerateTokensAsync("<start>start<end>", ct);

                // Logic: If the model is responsive and providing different outputs
                if (!string.IsNullOrWhiteSpace(response1) && response1 != response2)
                {
                    return HealthCheckResult.Healthy("Model is responsive and producing varied output.");
                }

                return HealthCheckResult.Degraded("Model is responding, but output is suspicious or repetitive.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Model inference failed.", ex);
            }
        }
    }
}