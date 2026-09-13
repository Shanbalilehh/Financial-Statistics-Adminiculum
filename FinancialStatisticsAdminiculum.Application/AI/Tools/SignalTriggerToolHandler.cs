using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinancialStatisticsAdminiculum.Application.AI.Entities;
using FinancialStatisticsAdminiculum.Application.AI.Interfaces;
using FinancialStatisticsAdminiculum.Application.AI.SchemaAggregators;

namespace FinancialStatisticsAdminiculum.Application.AI.Tools
{
    public class SignalTriggerToolHandler : IGemmaTool
    {
        public const string ToolName = "create_signal_trigger";
        public string Name => ToolName;
        public string Description => "Creates a conditional trigger alert when a financial or statistical metric crosses a threshold";

        public Dictionary<string, GemmaParameter> Parameters => new()
        {
            {
                "condition", new GemmaParameter
                {
                    Type = "STRING",
                    Description = "Condition operator: 'GreaterThan' or 'LessThan'"
                }
            },
            {
                "threshold", new GemmaParameter
                {
                    Type = "FLOAT",
                    Description = "Numerical cutoff value for trigger condition"
                }
            }
        };

        public Task<ToolExecutionResult> ExecuteAsync(Dictionary<string, string> arguments, CancellationToken ct = default)
        {
            string condition = "GreaterThan";
            if (arguments.TryGetValue("condition", out var cond) && !string.IsNullOrWhiteSpace(cond))
            {
                condition = cond.Contains("less", StringComparison.OrdinalIgnoreCase) ? "LessThan" : "GreaterThan";
            }

            double threshold = 25.0;
            if (arguments.TryGetValue("threshold", out var threshStr) && double.TryParse(threshStr, out var thresh))
            {
                threshold = thresh;
            }

            var jsonResponse = $"{{\"tool\": \"{ToolName}\", \"action\": \"ADD_ENTITY\", \"entityType\": \"SignalTrigger\", \"parameters\": {{\"condition\": \"{condition}\", \"threshold\": {threshold}}}}}";
            return Task.FromResult(ToolExecutionResult.Success(jsonResponse));
        }
    }
}
