using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SerilogTimings;
using Serilog.Events;
using FinancialStatisticsAdminiculum.Application.Interfaces;
using FinancialStatisticsAdminiculum.Application.AI.Interfaces;

namespace FinancialStatisticsAdminiculum.Application.AI.Services
{
    public class OrchestratorService : IOrchestratorService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OrchestratorService> _logger;
        private readonly IFunctionGemmaParser _parser; 

        // 1. We remove INlpEngine from here to prevent circular dependencies, 
        // because GemmaOnnxService will be calling THIS service.
        public OrchestratorService(
            IServiceProvider serviceProvider, 
            ILogger<OrchestratorService> logger,
            IFunctionGemmaParser parser)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _parser = parser;
        }

        // 2. Renamed to reflect that it receives the RAW text from the model
        public async Task<string> ParseAndExecuteToolsAsync(string rawModelOutput)
        {
            using var op = Operation.At(LogEventLevel.Information).Begin("Parsing and Executing AI Tools");

            // 3. Use our high-performance Span parser instead of JSON
            var toolCalls = _parser.ParseToolCalls(rawModelOutput);

            if (toolCalls.Count == 0)
            {
                _logger.LogWarning("Could not parse AI intent from output: {Output}", rawModelOutput);
                return string.Empty; // Return empty to signal no tools were executed
            }

            var results = new List<string>();

            // 4. Handle multiple tool requests if the model asks for them
            foreach (var call in toolCalls)
            {
                var handler = _serviceProvider.GetKeyedService<IGemmaTool>(call.Name);

                if (handler == null)
                {
                    _logger.LogWarning("Tool '{ToolName}' is not implemented.", call.Name);
                    results.Add($"<start_function_response>response:{call.Name}{{Error: Tool not implemented}}<end_function_response>");
                    continue;
                }

                try
                {
                    // Execute the tool using the parsed dictionary
                    string toolResult = await handler.ExecuteAsync(call.Arguments);
                    
                    // Format the result EXACTLY how FunctionGemma expects it back
                    results.Add($"<start_function_response>response:{call.Name}{{{toolResult}}}<end_function_response>");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing tool '{ToolName}'", call.Name);
                    results.Add($"<start_function_response>response:{call.Name}{{Error: Internal execution failure}}<end_function_response>");
                }
            }

            op.Complete();
            
            // 5. Combine all formatted responses into one string for the history
            return string.Join("", results);
        }
    }
}