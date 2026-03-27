using Microsoft.Extensions.Logging;
using SerilogTimings;
using Serilog.Events;
using FinancialStatisticsAdminiculum.Application.Interfaces;
using FinancialStatisticsAdminiculum.Application.AI.Interfaces;
using FinancialStatisticsAdminiculum.Core.Interfaces;
using FinancialStatisticsAdminiculum.Core.Entities;

namespace FinancialStatisticsAdminiculum.Application.AI.Services
{
    public class OrchestratorService : IOrchestratorService
    {
        private readonly IToolResolver _resolver;
        private readonly ILogger<OrchestratorService> _logger;
        private readonly IFunctionGemmaParser _parser;
        private readonly IGemmaOnnxService _gemmaService;

        public OrchestratorService(
            IToolResolver resolver,
            ILogger<OrchestratorService> logger,
            IFunctionGemmaParser parser,
            IGemmaOnnxService gemmaService)
        {
            _resolver = resolver;
            _logger = logger;
            _parser = parser;
            _gemmaService = gemmaService;
        }

        public async Task<string> HandleUserMessageAsync(string userPrompt)
        {
            _logger.LogInformation("Handling user message: {PromptLength} chars", userPrompt.Length);

            using var op = Operation.At(LogEventLevel.Information).Begin("Handling user message");

            // First generation pass: user prompt → raw model output
            string modelOutput = await _gemmaService.GenerateAsync(ChatRole.User, userPrompt);

            // Attempt to parse tool calls from the raw output
            var toolCalls = _parser.ParseToolCalls(modelOutput);

            if (toolCalls.Count == 0)
            {
                _logger.LogDebug("No tool calls detected. Returning direct model response.");
                op.Complete();
                return modelOutput;
            }

            // Execute each tool and collect formatted responses
            var toolResults = new List<string>();

            foreach (var call in toolCalls)
            {
                _logger.LogInformation("Executing tool {ToolName} with args {@Args}", call.Name, call.Arguments);

                using var toolOp = Operation.Time($"Executing tool {call.Name}");
                var handler = _resolver.Resolve(call.Name);

                if (handler == null)
                {
                    _logger.LogWarning("Tool '{ToolName}' is not implemented.", call.Name);
                    toolResults.Add($"<start_function_response>response:{call.Name}{{Error: Tool not implemented}}<end_function_response>");
                    continue;
                }

                var toolResult = await handler.ExecuteAsync(call.Arguments);

                if (toolResult.IsSuccess)
                {
                    toolResults.Add($"<start_function_response>response:{call.Name}{{{toolResult}}}<end_function_response>");
                }
                else
                {
                    _logger.LogError("Error executing tool '{ToolName}'", call.Name);
                    toolResults.Add($"<start_function_response>response:{call.Name}{{Error: Internal execution failure}}<end_function_response>");
                }
            }

            // Second generation pass: feed tool results back, get final response
            string combinedToolResults = string.Join("", toolResults);
            _logger.LogInformation("Tools executed. Generating final response with tool context.");

            string finalOutput = await _gemmaService.GenerateAsync(ChatRole.Tool, combinedToolResults);

            op.Complete();
            return finalOutput;
        }

        // Kept for internal/diagnostic use if needed
        public async Task<string> ParseAndExecuteToolsAsync(string rawModelOutput)
        {
            var toolCalls = _parser.ParseToolCalls(rawModelOutput);

            if (toolCalls.Count == 0)
                return string.Empty;

            var results = new List<string>();

            foreach (var call in toolCalls)
            {
                var handler = _resolver.Resolve(call.Name);

                if (handler == null)
                {
                    results.Add($"<start_function_response>response:{call.Name}{{Error: Tool not implemented}}<end_function_response>");
                    continue;
                }

                var toolResult = await handler.ExecuteAsync(call.Arguments);
                results.Add(toolResult.IsSuccess
                    ? $"<start_function_response>response:{call.Name}{{{toolResult}}}<end_function_response>"
                    : $"<start_function_response>response:{call.Name}{{Error: Internal execution failure}}<end_function_response>");
            }

            return string.Join("", results);
        }
    }
}