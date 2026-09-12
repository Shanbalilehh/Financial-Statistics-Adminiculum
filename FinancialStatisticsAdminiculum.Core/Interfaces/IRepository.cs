using System.Linq.Expressions;
using FinancialStatisticsAdminiculum.Core.Exceptions;

namespace FinancialStatisticsAdminiculum.Core.Interfaces
{
    [RiskCommunity("PersistenceCommunity")]
    public interface IRepository<T> where T : class
    {
        // 1. Get by ID
        Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
        
        // 2. The OCP-Enabler: Pass any logic here without changing the Repo
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
        
        // 3. Get All (Use carefully with large tables!)
        Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);

        // 4. Atomic Writes
        Task AddAsync(T entity, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
        
        // 5. Deletes
        void Remove(T entity);
    }
}
