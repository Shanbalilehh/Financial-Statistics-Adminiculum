namespace FinancialStatisticsAdminiculum.Application.AI.Entities
{
    public class GemmaToolCall
    {
        public required string Name { get; set; }
        public Dictionary<string, string> Arguments { get; set; } = new(); 
    }
}