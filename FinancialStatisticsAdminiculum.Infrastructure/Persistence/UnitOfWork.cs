using FinancialStatisticsAdminiculum.Core.Entities;
using FinancialStatisticsAdminiculum.Core.Interfaces;
using FinancialStatisticsAdminiculum.Infrastructure.Repositories;

namespace FinancialStatisticsAdminiculum.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        // We cache the repositories so we don't create new instances every time
        private IRepository<Asset>? _assets;
        private IRepository<PricePoint>? _pricePoints;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        // Lazy Loading: Only create the repository instance if someone asks for it
        public IRepository<Asset> Assets => 
            _assets ??= new Repository<Asset>(_context);

        public IRepository<PricePoint> PricePoints => 
            _pricePoints ??= new Repository<PricePoint>(_context);

        public async Task<int> CompleteAsync()
        {
            // This commits ALL changes from ALL repositories in one transaction
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}