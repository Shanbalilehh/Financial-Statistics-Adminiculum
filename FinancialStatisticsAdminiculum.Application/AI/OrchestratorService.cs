using System;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using FinancialStatisticsAdminiculum.Core.Interfaces; // Add this

namespace FinancialStatisticsAdminiculum.Application.AI
{
    public class OrchestratorService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly INlpEngine _nlpEngine; // 1. Add the NLP Engine

        // 2. Inject it via the constructor
        public OrchestratorService(IServiceProvider serviceProvider, INlpEngine nlpEngine)
        {
            _serviceProvider = serviceProvider;
            _nlpEngine = nlpEngine;
        }

        // 3. Change the parameter name to reflect what it actually is now
        public async Task<string> ExecuteAiCommandAsync(string userPrompt)
        {
            try
            {
                string rawJsonFromGemma = await _nlpEngine.ExtractToolCallAsync(userPrompt);

                string? cleanJson = ExtractJsonPayload(rawJsonFromGemma);

                // Fail fast if no JSON was found, and SHOW the raw text!
                if (string.IsNullOrWhiteSpace(cleanJson))
                {
                    return $"AI format failure. Raw AI Output: \n\n{rawJsonFromGemma}";
                }

                var toolCall = JsonSerializer.Deserialize<GemaToolCall>(cleanJson);
                
                if (toolCall == null || string.IsNullOrEmpty(toolCall.Name)) 
                    return "Error: Could not parse AI intent.";

                var handler = _serviceProvider.GetKeyedService<IAiToolHandler>(toolCall.Name);

                if (handler == null)
                    return $"Error: The tool '{toolCall.Name}' is not implemented.";

                return await handler.ExecuteAsync(toolCall.Arguments);
            }
            catch (JsonException ex)
            {
                return $"JSON Parsing Error: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"Execution failed: {ex.Message}";
            }
        }
        private string? ExtractJsonPayload(string input)
        {
            ReadOnlySpan<char> span = input.AsSpan();
            int startIndex = span.IndexOf('{');
            int endIndex = span.LastIndexOf('}');

            if (startIndex != -1 && endIndex != -1 && endIndex > startIndex)
            {
                return span.Slice(startIndex, endIndex - startIndex + 1).ToString();
            }

            // Return null instead of the raw string to signal a failure
            return null; 
        }
    
        
    }
}