using FinancialStatisticsAdminiculum.Core.Entities;

namespace FinancialStatisticsAdminiculum.Core.Interfaces
{
    public interface IGemmaOnnxService
    {
        Task<string> GenerateAsync(ChatRole role, string content);
    }
}