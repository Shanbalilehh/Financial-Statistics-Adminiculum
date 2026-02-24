using FinancialStatisticsAdminiculum.Core.Interfaces;
using FinancialStatisticsAdminiculum.Core.Entities;
using FinancialStatisticsAdminiculum.Core.Extensions;
using FinancialStatisticsAdminiculum.Application.DTOs;

namespace FinancialStatisticsAdminiculum.Application.Services
{
    // Organized by responsibility (matches TrendExtensions)
    public class TrendAnalysisService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TrendAnalysisService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // 1. The Async wrapper (Handles I/O and Unit of Work)
        public async Task<TimeSeriesDto> GetMovingAverageAsync(string ticker, DateTime from, DateTime to, int period)
        {
            var rawPoints = await _unitOfWork.PricePoints.FindAsync(p => 
                p.AssetTicker == ticker && 
                p.Timestamp >= from && 
                p.Timestamp <= to);

            // Pass to the synchronous method to safely use Spans
            return ProcessMovingAverage(ticker, rawPoints.ToList(), period);
        }

        // 2. The Sync processor (CPU bound, Spans are fully supported here)
        private TimeSeriesDto ProcessMovingAverage(string ticker, List<PricePoint> rawPoints, int period)
        {
            var times = rawPoints.Select(p => p.Timestamp).ToArray();
            var values = rawPoints.Select(p => (double)p.Value).ToArray(); 

            // Using your updated constructor with Ticker
            var rawSeries = new TimeSeries(ticker, times, values);

            // Apply OCP math
            var smaSeries = rawSeries.CalculateSMA(period);

            var dto = new TimeSeriesDto
            {
                Ticker = ticker,
                IndicatorName = $"SMA ({period})",
                Data = new List<DataPointDto>(smaSeries.GetRawValues().Length)
            };

            // It is now perfectly legal to capture the Span here
            var smaTimes = smaSeries.Time;
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
