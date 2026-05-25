using Shared.Entities;

namespace FinancialStatisticsAdminiculum.Core.Entities
{
    public class AnalysisJob
    {
        public int Id { get; init; }
        public Guid CorrelationId { get; init; } = Guid.NewGuid();
        public required State Status { get; set; }
        public List<ChatMessage> History { get; private set; } = [];

        public void AddChatMessage( ChatMessage message)
        {
            History.Add(message);
        }
        
        public void RemoveChatMessage( ChatMessage message)
        {
            History.Remove(message);
        }
        public string GetMessagesAsString()
        {
            return string.Join("\n", History.Select(m => $"{m.Role}: {m.Content}"));
        }

    }
}