using System.Threading.Channels;
using FinancialStatisticsAdminiculum.Core.Interfaces;
using FinancialStatisticsAdminiculum.Core.Entities;

namespace FinancialStatisticsAdminiculum.Infrastructure.Messaging.Services
{
    public class JobCompletionNotifier : IJobCompletionNotifier
    {
        // Create an unbounded channel (unlimited capacity in RAM)
        private readonly Channel<JobCompletedEvent> _channel = Channel.CreateUnbounded<JobCompletedEvent>();

        // The Orchestrator calls this to drop an event into the channel
        public async ValueTask NotifyJobCompletedAsync(JobCompletedEvent jobEvent, CancellationToken ct = default)
        {
            await _channel.Writer.WriteAsync(jobEvent, ct);
        }

        // The Controller loops over this to read events as they arrive
        public IAsyncEnumerable<JobCompletedEvent> ReadAllEventsAsync(CancellationToken ct = default)
        {
            return _channel.Reader.ReadAllAsync(ct);
        }
    }
}