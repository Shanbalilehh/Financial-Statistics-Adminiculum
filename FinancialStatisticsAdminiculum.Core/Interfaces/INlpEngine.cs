namespace FinancialStatisticsAdminiculum.Core.Interfaces
{
    public interface INlpEngine
    {
        // Takes the user's text and returns the JSON tool call
        Task<string> ExtractToolCallAsync(string userPrompt);
    }
}
