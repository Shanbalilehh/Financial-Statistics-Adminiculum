using Shared.Contracts;

namespace FinancialStatisticsAdminiculum.Application.Interfaces
{
    public interface IMessagePublisher
    {
        Task PublishRequest(InferenceRequestMessage message);
    }
}