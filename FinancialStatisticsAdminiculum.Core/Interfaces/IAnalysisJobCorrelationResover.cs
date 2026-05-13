namespace FinancialStatisticsAdminiculum.Core.Interfaces
{
    public interface IAnalysisJobCorrelationResolver
    {
        Task<int> ResolveAsync(Guid correlationId, CancellationToken ct);
    }
}