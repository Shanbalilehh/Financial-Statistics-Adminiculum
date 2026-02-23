using FinancialStatisticsAdminiculum.Core.Interfaces;
using FinancialStatisticsAdminiculum.Core.Entities; // Using your updated namespace
using FinancialStatisticsAdminiculum.Core.Extensions;
using FinancialStatisticsAdminiculum.Application.DTOs;

namespace FinancialStatisticsAdminiculum.Application.Services
{
    // Clustered Service strictly for Trend calculations
    public class TrendAnalysisService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TrendAnalysisService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<TimeSeriesDto> GetMovingAverageAsync(string ticker, DateTime from, DateTime to, int period)
        {
            // 1. Fetch raw data
            var rawPoints = (await _unitOfWork.PricePoints.FindAsync(p => 
                p.AssetTicker == ticker && 
                p.Timestamp >= from && 
                p.Timestamp <= to))
                .OrderBy(p => p.Timestamp) // Ensure chronological order
                .ToList();

            if (!rawPoints.Any())
                return new TimeSeriesDto { Ticker = ticker, IndicatorName = $"SMA ({period})", Data = new() };

            // 2. Map to your updated high-performance Math Domain Object
            var times = rawPoints.Select(p => p.Timestamp).ToArray();
            var values = rawPoints.Select(p => (double)p.Value).ToArray(); 
            
            // TICKER INCLUDED HERE!
            var rawSeries = new TimeSeries(ticker, times, values); 

            // 3. Apply the OCP Mathematical Transformation
            var smaSeries = rawSeries.CalculateSMA(period);

            // 4. Map back to DTO
            var dto = new TimeSeriesDto
            {
                Ticker = smaSeries.Ticker, // Using the ticker from the series
                IndicatorName = $"SMA ({period})",
                Data = new List<DataPointDto>(smaSeries.Values.Length)
            };

            ReadOnlySpan<DateTime> smaTimes = smaSeries.Time;
            var smaValues = smaSeries.Values;

            for (int i = 0; i < smaValues.Length; i++)
            {
                if (!double.IsNaN(smaValues[i]))
                {
                    dto.Data.Add(new DataPointDto(smaTimes[i], smaValues[i]));
                }
            }

            return dto;
        }
    }
}
