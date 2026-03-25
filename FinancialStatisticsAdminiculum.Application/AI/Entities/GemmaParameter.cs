namespace FinancialStatisticsAdminiculum.Application.AI.Entities
{
    public class GemmaParameter
    {
        public required string Type { get; set; } 
        public required string Description { get; set; }
        public bool IsRequired { get; set; } = true;
        public string[]? EnumValues { get; set; }
    }
}