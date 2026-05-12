using FinancialStatisticsAdminiculum.Application.Interfaces;
using FinancialStatisticsAdminiculum.Application.AI.Interfaces;
using System.Text;
using Microsoft.Extensions.Logging;

namespace FinancialStatisticsAdminiculum.Application.AI.SchemaAggregators
{
    //Objective: Tools Schemas ready for passing to model (developer)
    public class AiSchemaAggregator : IAiSchemaAggregator
    {
        private readonly IEnumerable<IGemmaTool> _availableTools;
        private readonly ILogger<AiSchemaAggregator> _logger;

        public AiSchemaAggregator(IEnumerable<IGemmaTool> availableTools, ILogger<AiSchemaAggregator> logger)
        {
            _availableTools = availableTools;
            _logger = logger;
        }

        public string BuildCombinedTool()
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are a model that can do function calling with the following functions");
            _logger.LogDebug("AiSchemaAggregator has {Count} tools: {Names}",
                _availableTools.Count(),
                string.Join(", ", _availableTools.Select(t => t.GetType().Name)));
            
            foreach (var tool in _availableTools)
            {
                // FunctionGemma specific custom formatter.
                sb.Append(GemmaSchemaGenerator.GenerateDeclaration(tool));
                _logger.LogInformation("Generated Schema: {Schema}", GemmaSchemaGenerator.GenerateDeclaration(tool));
            }
            
            return sb.ToString();
        }
    }
}