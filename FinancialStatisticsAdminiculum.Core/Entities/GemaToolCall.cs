namespace FinancialStatisticsAdminiculum.Core.Entities
{
    public class GemmaToolCall
    {
        public required string Name { get; set; }
        public Dictionary<string, string> Arguments { get; set; } = new(); 
    }
}