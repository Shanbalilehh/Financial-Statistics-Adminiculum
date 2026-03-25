using FinancialStatisticsAdminiculum.Application.AI.Entities;

namespace FinancialStatisticsAdminiculum.Infrastructure.AI.Entities
{
    public enum ChatRole
    {
        Developer,
        User,
        Model,
        Tool
    }

    public class ChatMessage
    {
        public required ChatRole Role { get; set; }
        public string Content { get; set; } = string.Empty;
        
        public List<GemmaToolCall>? ToolCalls { get; set; }
    }
}