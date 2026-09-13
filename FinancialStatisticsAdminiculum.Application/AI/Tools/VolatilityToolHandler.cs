using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinancialStatisticsAdminiculum.Application.AI.Entities;
using FinancialStatisticsAdminiculum.Application.AI.Interfaces;
using FinancialStatisticsAdminiculum.Application.AI.SchemaAggregators;

namespace FinancialStatisticsAdminiculum.Application.AI.Tools
{
    public class VolatilityToolHandler : IGemmaTool
    {
        public const string ToolName = "calculate_rolling_volatility";
        public string Name => ToolName;
        public string Description => "Calculates rolling historical volatility for an asset over a specified window";

        public Dictionary<string, GemmaParameter> Parameters => new()
        {
            {
                "ticker", new GemmaParameter
                {
                    Type = "STRING",
                    Description = "Asset ticker symbol (e.g. AAPL, MSFT, SPY)"
                }
            },
            {
                "period", new GemmaParameter
                {
                    Type = "INTEGER",
                    Description = "Number of days for rolling window (default 30)"
                }
            },
            {
                "annualize", new GemmaParameter
                {
                    Type = "BOOLEAN",
                    Description = "Whether to annualize volatility (sqrt(252))"
                }
            }
        };

        public Task<ToolExecutionResult> ExecuteAsync(Dictionary<string, string> arguments, CancellationToken ct = default)
        {
            if (!arguments.TryGetValue("ticker", out var ticker) || string.IsNullOrWhiteSpace(ticker))
            {
                return Task.FromResult(ToolExecutionResult.Failure("Error: Missing or empty 'ticker' argument."));
            }

            int period = 30;
            if (arguments.TryGetValue("period", out var periodStr) && int.TryParse(periodStr, out var parsedPeriod))
            {
                period = Math.Max(5, parsedPeriod);
            }

            // Return structured tool execution payload with canvas mutation intent
            var jsonResponse = $"{{\"tool\": \"{ToolName}\", \"action\": \"ADD_ENTITY\", \"entityType\": \"VolatilityEstimator\", \"ticker\": \"{ticker.ToUpperInvariant()}\", \"parameters\": {{\"period\": {period}, \"annualizationFactor\": 252}}}}";
            return Task.FromResult(ToolExecutionResult.Success(jsonResponse));
        }
    }
}
