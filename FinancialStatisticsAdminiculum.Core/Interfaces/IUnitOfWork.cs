using FinancialStatisticsAdminiculum.Core.Entities;

namespace FinancialStatisticsAdminiculum.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // Expose your specific repositories here
        // (You can also use a generic accessor, but specific properties are cleaner)
        IRepository<Asset> Assets { get; }
        IRepository<PricePoint> PricePoints { get; }

        // The single "Save" button for the whole transaction
        Task<int> CompleteAsync();
    }
}