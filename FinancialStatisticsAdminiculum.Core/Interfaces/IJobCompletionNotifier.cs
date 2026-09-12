using FinancialStatisticsAdminiculum.Core.Entities;

namespace FinancialStatisticsAdminiculum.Core.Interfaces
{
    public interface IJobCompletionNotifier
    {
        ValueTask NotifyJobCompletedAsync(JobCompletedEvent jobEvent, CancellationToken ct = default);
        IAsyncEnumerable<JobCompletedEvent> ReadAllEventsAsync(CancellationToken ct = default);
    }
}