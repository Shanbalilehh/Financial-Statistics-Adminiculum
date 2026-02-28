using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialStatisticsAdminiculum.Application.AI
{
    public class OrchestratorService
    {
        private readonly IServiceProvider _serviceProvider;

        public OrchestratorService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<string> ExecuteAiCommandAsync(string rawJsonFromGemma)
        {
            try
            {
                var toolCall = JsonSerializer.Deserialize<GemaToolCall>(rawJsonFromGemma);
                if (toolCall == null || string.IsNullOrEmpty(toolCall.Name)) 
                    return "Error: Could not parse AI intent.";

                // OCP MAGIC: Ask the DI container for the handler matching the string name!
                var handler = _serviceProvider.GetKeyedService<IAiToolHandler>(toolCall.Name);

                if (handler == null)
                    return $"Error: The tool '{toolCall.Name}' is not implemented.";

                // Execute the tool without knowing what it actually does
                return await handler.ExecuteAsync(toolCall.Arguments);
            }
            catch (Exception ex)
            {
                return $"Execution failed: {ex.Message}";
            }
        }
    }
}