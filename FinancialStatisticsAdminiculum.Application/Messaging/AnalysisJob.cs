using FinancialStatisticsAdminiculum.Application.AI.Entities;

namespace FinancialStatisticsAdminiculum.Application.Messaging
{
    public class AnalysisJob
    {
        public Guid CorrelationId { get; init; }
        public string Status { get; set; } = "Pending";
        public List<ChatMessage> History { get; set; } = [];

        public void AddChatMessage( ChatMessage message)
        {
            History.Add(message);
        }
        
        public void RemoveChatMessage( ChatMessage message)
        {
            History.Remove(message);
        }

    }
}