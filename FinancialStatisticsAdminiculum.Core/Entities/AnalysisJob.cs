namespace FinancialStatisticsAdminiculum.Core.Entities
{
    public class AnalysisJob
    {
        public int Id { get; init; }
        public Guid CorrelationId { get; init; } = Guid.NewGuid();
        public string Status { get; set; } = "Pending";
        public List<ChatMessage> History { get; private set; } = [];

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