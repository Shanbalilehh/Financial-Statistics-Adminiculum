using System.Text.Json;
using FinancialStatisticsAdminiculum.Application.Services;

namespace FinancialStatisticsAdminiculum.Application.AI.Tools
{
    // The specific arguments for this tool only
    public class SmaArguments
    {
        public string Ticker { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public int Period { get; set; }
    }

    public class SmaToolHandler : IAiToolHandler
    {
        private readonly TrendAnalysisService _trendService;

        public SmaToolHandler(TrendAnalysisService trendService)
        {
            _trendService = trendService;
        }

        public static string ToolName => "get_moving_average";

        public static string GetToolSchema()
        {
            return """
            {
                "type": "function",
                "function": {
                    "name": "get_moving_average",
                    "description": "Calculates the Simple Moving Average (SMA)",
                    "parameters": {
                        "type": "object",
                        "properties": {
                            "ticker": { "type": "string" },
                            "from": { "type": "string" },
                            "to": { "type": "string" },
                            "period": { "type": "integer" }
                        },
                        "required": ["ticker", "from", "to", "period"]
                    }
                }
            }
            """;
        }

        public async Task<string> ExecuteAsync(JsonElement arguments)
        {
            // 1. Deserialize the generic element into our specific arguments
            var args = arguments.Deserialize<SmaArguments>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (args == null) return "Error: Invalid arguments provided.";

            DateTime fromDate = DateTime.Parse(args.From);
            DateTime toDate = DateTime.Parse(args.To);

            // 2. Execute the math
            var result = await _trendService.GetMovingAverageAsync(args.Ticker, fromDate, toDate, args.Period);

            return $"Successfully calculated {result.IndicatorName} for {result.Ticker}. Points generated: {result.Data.Count}";
        }
    }
}