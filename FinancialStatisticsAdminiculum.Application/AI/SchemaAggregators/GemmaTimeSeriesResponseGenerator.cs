using System.Text;
using System.Globalization;
using FinancialStatisticsAdminiculum.Application.DTOs;

namespace FinancialStatisticsAdminiculum.Application.AI.SchemaAggregators
{
    // Objective: Generate Tool Response strings using TimeSeriesDto
    public static class GemmaTimeSeriesResponseGenerator
    {
        public static string GenerateResponse(string toolName, TimeSeriesDto dto)
        {
            var sb = new StringBuilder();
            sb.Append($"<start_function_response>response:{toolName}{{");
            
            // Metadata for the AI
            sb.Append($"ticker:<escape>{dto.Ticker}<escape>,");
            sb.Append($"indicator:<escape>{dto.IndicatorName}<escape>,");
            
            // Wrap the data points in a single escape block for the parser
            sb.Append("points:<escape>[");

            bool isFirst = true;
            foreach (var point in dto.Data)
            {
                // Filter out the SMA padding (NaN) so the model doesn't get confused
                if (double.IsNaN(point.Value)) continue;

                if (!isFirst) sb.Append(",");
                
                // Formatting date as YYYY-MM-DD for token efficiency
                sb.Append($"{point.Time:yyyy-MM-dd}:{point.Value.ToString(CultureInfo.InvariantCulture)}");
                
                isFirst = false;
            }

            sb.Append("]<escape>}<end_function_response>");
            
            return sb.ToString();
        }
    }
}