using FinancialStatisticsAdminiculum.Core.Interfaces;
using FinancialStatisticsAdminiculum.Core.Entities;
using FinancialStatisticsAdminiculum.Core.Extensions;
using FinancialStatisticsAdminiculum.Application.DTOs;
using FinancialStatisticsAdminiculum.Application.Interfaces;
using Microsoft.Extensions.Logging;
using SerilogTimings;
using Serilog.Events;

namespace FinancialStatisticsAdminiculum.Application.Services
{
    // Organized by responsibility (matches TrendExtensions)
    public class TrendAnalysisService : ITrendAnalysisService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TrendAnalysisService> _logger;

        public TrendAnalysisService(IUnitOfWork unitOfWork, ILogger<TrendAnalysisService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // The Async wrapper (Handles I/O and Unit of Work)
        public async Task<TimeSeriesDto> GetMovingAverageAsync(string ticker, DateTime from, DateTime to, int period)
        {
            using var op = Operation.At(LogEventLevel.Information).Begin("Getting/Processing moving average from Sync to Async");
            var rawPoints = await _unitOfWork.PricePoints.FindAsync(p => 
                p.AssetTicker == ticker && 
                p.Timestamp >= from && 
                p.Timestamp <= to);

            op.Complete();
            // Pass to the synchronous method to safely use Spans
            return ProcessMovingAverage(ticker, [.. rawPoints], period);
        }

        // The Sync Span processor
        private TimeSeriesDto ProcessMovingAverage(string ticker, List<PricePoint> rawPoints, int period)
        {
            _logger.LogInformation("Processing Moving Average");
            var times = rawPoints.Select(p => p.Timestamp).ToArray();
            var values = rawPoints.Select(p => (double)p.Value).ToArray(); 

            var rawSeries = new TimeSeries(ticker, times, values);

            var smaSeries = rawSeries.CalculateSMA(period);

            var dto = new TimeSeriesDto
            {
                Ticker = ticker,
                IndicatorName = $"SMA ({period})",
                Data = new List<DataPointDto>(smaSeries.GetRawValues().Length)
            };

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
