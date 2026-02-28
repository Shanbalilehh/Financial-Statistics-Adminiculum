namespace FinancialStatisticsAdminiculum.Core.Interfaces
{
    public interface INlpEngine
    {
        Task<string> ExtractToolCallAsync(string userPrompt);
    }
}
