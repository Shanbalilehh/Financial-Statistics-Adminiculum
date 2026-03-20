using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using FinancialStatisticsAdminiculum.Core.Interfaces; 
using FinancialStatisticsAdminiculum.Application.Interfaces;
using Microsoft.Extensions.Logging;
using SerilogTimings;
using Serilog.Events;

namespace FinancialStatisticsAdminiculum.Application.AI
{
    public class OrchestratorService : IOrchestratorService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly INlpEngine _nlpEngine; 
        private readonly ILogger<OrchestratorService> _logger;
        // 2. Inject it via the constructor
        public OrchestratorService(IServiceProvider serviceProvider, INlpEngine nlpEngine, ILogger<OrchestratorService> logger)
        {
            _serviceProvider = serviceProvider;
            _nlpEngine = nlpEngine;
            _logger = logger;
        }

        // 3. Change the parameter name to reflect what it actually is now
        public async Task<string> ExecuteAiCommandAsync(string userPrompt)
        {
            using var op = Operation.At(LogEventLevel.Information).Begin("Executing AI Command");
            string rawJsonFromGemma = await _nlpEngine.ExtractToolCallAsync(userPrompt);

            op.Complete();
            string? cleanJson = ExtractJsonPayload(rawJsonFromGemma);

            if (string.IsNullOrWhiteSpace(cleanJson))
            {
                _logger.LogWarning("User prompt clean Json is null or empty.");
                return $"AI format failure. Raw AI Output: \n\n{rawJsonFromGemma}";
            }

            var toolCall = JsonSerializer.Deserialize<GemaToolCall>(cleanJson);
            
            if (toolCall == null || string.IsNullOrEmpty(toolCall.Name)) 
            {
                _logger.LogWarning("Could not parse AI intent");
                return "Error: Could not parse AI intent.";
            }

            var handler = _serviceProvider.GetKeyedService<IAiToolHandler>(toolCall.Name);

            if (handler == null)
            {
                _logger.LogWarning($"Tool not implemented");
                return $"Error: The tool '{toolCall.Name}' is not implemented.";
            }

            return await handler.ExecuteAsync(toolCall.Arguments);
        }
        private string? ExtractJsonPayload(string input)
        {
            ReadOnlySpan<char> span = input.AsSpan();
            int startIndex = span.IndexOf('{');
            int endIndex = span.LastIndexOf('}');

            if (startIndex != -1 && endIndex != -1 && endIndex > startIndex)
            {
                _logger.LogInformation("User prompt parsed to Json Successfully.");
                return span.Slice(startIndex, endIndex - startIndex + 1).ToString();
            }

            // Return null instead of the raw string to signal a failure
            _logger.LogWarning("User prompt parsing failed.");
            return null; 
        }
    
        
    }
}