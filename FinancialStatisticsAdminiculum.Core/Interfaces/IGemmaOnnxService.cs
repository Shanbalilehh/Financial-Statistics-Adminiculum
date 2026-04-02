using FinancialStatisticsAdminiculum.Core.Entities;
using FinancialStatisticsAdminiculum.Core.Exceptions;

namespace FinancialStatisticsAdminiculum.Core.Interfaces
{
    [RiskCommunity("NlpCommunity")]
    public interface IGemmaOnnxService
    {
        Task<string> GenerateAsync(ChatRole role, string content, CancellationToken ct = default);
    }
}