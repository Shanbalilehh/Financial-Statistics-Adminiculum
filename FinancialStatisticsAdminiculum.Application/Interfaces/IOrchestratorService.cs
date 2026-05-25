using FinancialStatisticsAdminiculum.Core.Exceptions;
using Shared.Contracts;

namespace FinancialStatisticsAdminiculum.Application.Interfaces
{
    [RiskCommunity("NlpCommunity")]
    public interface IOrchestratorService
    {
        Task<Guid> HandleUserMessageAsync(string userPrompt, CancellationToken ct = default);
        Task ProcessInferenceResponseAsync(InferenceResponseMessage message, CancellationToken ct);
    }
}
