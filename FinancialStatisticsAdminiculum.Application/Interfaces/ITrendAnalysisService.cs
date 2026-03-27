using FinancialStatisticsAdminiculum.Application.DTOs;

namespace FinancialStatisticsAdminiculum.Application.Interfaces
{
    public interface ITrendAnalysisService
    {
        Task<TimeSeriesDto> GetMovingAverageAsync(string ticker, DateTime from, DateTime to, int period);
    }
}
