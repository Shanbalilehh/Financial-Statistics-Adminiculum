using FinancialStatisticsAdminiculum.Application.AI.Interfaces;
using FinancialStatisticsAdminiculum.Application.AI.Entities;
using FinancialStatisticsAdminiculum.Application.AI.SchemaAggregators;
using FinancialStatisticsAdminiculum.Application.Interfaces;

namespace FinancialStatisticsAdminiculum.Application.AI.Tools
{
    public class SmaToolHandler : IGemmaTool
    {
        private readonly ITrendAnalysisService _trendService;

        public SmaToolHandler(ITrendAnalysisService trendService)
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

        public async Task<ToolExecutionResult> ExecuteAsync(Dictionary<string, string> arguments, CancellationToken ct = default)
        {
            if (!arguments.TryGetValue("ticker", out var ticker) || string.IsNullOrWhiteSpace(ticker))
                return ToolExecutionResult.Failure("Error: Missing or empty 'ticker' argument.");

            // 1. Parse 'from' and specify UTC kind
            if (!arguments.TryGetValue("from", out var fromRaw) || !DateTime.TryParse(fromRaw, out var fromParsed))
                return ToolExecutionResult.Failure("Error: Missing or invalid 'from' date.");
            
            var fromDate = DateTime.SpecifyKind(fromParsed, DateTimeKind.Utc);

            // 2. Parse 'to' and specify UTC kind
            if (!arguments.TryGetValue("to", out var toRaw) || !DateTime.TryParse(toRaw, out var toParsed))
                return ToolExecutionResult.Failure("Error: Missing or invalid 'to' date.");
            
            var toDate = DateTime.SpecifyKind(toParsed, DateTimeKind.Utc);

            if (!arguments.TryGetValue("period", out var periodRaw) || !int.TryParse(periodRaw, out var period) || period <= 0)
                return ToolExecutionResult.Failure("Error: Missing or invalid 'period' argument.");

            if (fromDate >= toDate)
                return ToolExecutionResult.Failure("Error: 'from' date must be earlier than 'to' date.");

            var result = await _trendService.GetMovingAverageAsync(ticker, fromDate, toDate, period);
            string formattedResult = GemmaTimeSeriesResponseGenerator.GenerateResponse(ToolName, result);

            if (result is null)
                return ToolExecutionResult.Failure("Error: No result returned from the moving average service.");

            return ToolExecutionResult.Success(formattedResult);
        }
    }
}