using System.Text.Json;

namespace FinancialStatisticsAdminiculum.Application.AI
{
    public interface IAiToolHandler
    {
        Task<string> ExecuteAsync(JsonElement arguments);
    }
}