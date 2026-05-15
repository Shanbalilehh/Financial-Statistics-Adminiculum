using FinancialStatisticsAdminiculum.Core.Entities;
using FinancialStatisticsAdminiculum.Core.Interfaces;

namespace FinancialStatisticsAdminiculum.Application.AI
{
    public static class HistoryConstructor
    {
        public static async Task<AnalysisJob> GetByCorrelationIdAsync(
            this IRepository<AnalysisJob> repository,
            Guid correlationId,
            CancellationToken ct = default)
        {
            return (await repository.FindAsync(j => j.CorrelationId == correlationId, ct))
                .SingleOrDefault()
                ?? throw new Exception($"AnalysisJob with CorrelationId {correlationId} not found.");
        }

        public static async Task AppendChatMessageAsync(
            this IRepository<AnalysisJob> repository,
            Guid correlationId,
            ChatMessage message,
            IUnitOfWork unitOfWork,
            CancellationToken ct = default)
        {
            var job = await repository.GetByCorrelationIdAsync(correlationId, ct);
            job.AddChatMessage(message);
            await unitOfWork.CompleteAsync(ct);
        }
    }
}