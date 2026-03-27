using FinancialStatisticsAdminiculum.Application.AI.Entities;
using FinancialStatisticsAdminiculum.Core.Entities;

namespace FinancialStatisticsAdminiculum.Infrastructure.AI.Entities
{
    public class ChatMessage
    {
        public required ChatRole Role { get; set; }
        public string Content { get; set; } = string.Empty;
        
        public List<GemmaToolCall>? ToolCalls { get; set; }
    }
}