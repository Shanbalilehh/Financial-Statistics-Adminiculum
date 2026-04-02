using FinancialStatisticsAdminiculum.Core.Exceptions;

namespace FinancialStatisticsAdminiculum.Application.Interfaces
{
    [RiskCommunity("NlpCommunity")]
    public interface IOrchestratorService
    {
        Task<string> HandleUserMessageAsync(string userPrompt, CancellationToken ct = default);
    }
}
