using FinancialStatisticsAdminiculum.Application.Services;
using FinancialStatisticsAdminiculum.Application.AI.Interfaces;
using FinancialStatisticsAdminiculum.Application.AI.Entities;

namespace FinancialStatisticsAdminiculum.Application.AI.Tools
{
    public class SmaToolHandler : IGemmaTool
    {
        private readonly TrendAnalysisService _trendService;

        public SmaToolHandler(TrendAnalysisService trendService)
        {
            _trendService = trendService;
        }

        public const string ToolName = "get_moving_average"; 
        public string Name => ToolName;
        public string Description => "Calculates the Simple Moving Average (SMA)";
        public Dictionary<string, GemmaParameter> Parameters => new()
        {
            { 
                "ticker", new GemmaParameter 
                { 
                    Type = "STRING", 
                    Description = "Asset ticker symbol, e.g. XAU for Gold",
                } 
            },
            { 
                "from", new GemmaParameter 
                { 
                    Type = "STRING", 
                    Description = "Start date in ISO 8601 format" 
                } 
            },
            { 
                "to", new GemmaParameter 
                { 
                    Type = "STRING", 
                    Description = "End date in ISO 8601 format" 
                } 
            },
            { 
                "period", new GemmaParameter 
                { 
                    Type = "INTEGER", 
                    Description = "Number of periods for the moving average" 
                } 
            }
        };

        public async Task<string> ExecuteAsync(Dictionary<string, string> arguments)
        {
            if (!arguments.TryGetValue("ticker", out var ticker) || string.IsNullOrWhiteSpace(ticker))
                return "Error: Missing or empty 'ticker' argument.";

            if (!arguments.TryGetValue("from", out var fromRaw) || !DateTime.TryParse(fromRaw, out var fromDate))
                return "Error: Missing or invalid 'from' date.";

            if (!arguments.TryGetValue("to", out var toRaw) || !DateTime.TryParse(toRaw, out var toDate))
                return "Error: Missing or invalid 'to' date.";

            if (!arguments.TryGetValue("period", out var periodRaw) || !int.TryParse(periodRaw, out var period) || period <= 0)
                return "Error: Missing or invalid 'period' argument. Expected a positive integer.";

            if (fromDate >= toDate)
                return "Error: 'from' date must be earlier than 'to' date.";

            var result = await _trendService.GetMovingAverageAsync(ticker, fromDate, toDate, period);

            if (result is null)
                return "Error: No result returned from the moving average service.";

            return $"Successfully calculated {result.IndicatorName} for {result.Ticker}. Points generated: {result.Data.Count}";
        }
    }
}