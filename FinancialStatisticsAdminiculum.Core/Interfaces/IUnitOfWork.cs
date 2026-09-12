using FinancialStatisticsAdminiculum.Core.Entities;
using FinancialStatisticsAdminiculum.Core.Exceptions;

namespace FinancialStatisticsAdminiculum.Core.Interfaces
{
    [RiskCommunity("PersistenceCommunity")]
    public interface IUnitOfWork : IDisposable
    {
        // Expose your specific repositories here
        // (You can also use a generic accessor, but specific properties are cleaner)
        IRepository<Asset> Assets { get; }
        IRepository<PricePoint> PricePoints { get; }

        // The single "Save" button for the whole transaction
        Task<int> CompleteAsync(CancellationToken ct = default);
    }
}