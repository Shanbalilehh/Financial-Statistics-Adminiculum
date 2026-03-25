using FinancialStatisticsAdminiculum.Application.AI.Services;

namespace FinancialStatisticsAdminiculum.Application.Interfaces
{
    public interface IOrchestratorService
    {
        Task<string> ParseAndExecuteToolsAsync(string rawModelOutput);
    }
}
