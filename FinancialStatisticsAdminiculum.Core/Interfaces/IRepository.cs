using System.Linq.Expressions;

namespace FinancialStatisticsAdminiculum.Core.Interfaces
{
    public interface IRepository<T> where T : class
    {
        // 1. Get by ID
        Task<T?> GetByIdAsync(int id);
        
        // 2. The OCP-Enabler: Pass any logic here without changing the Repo
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        
        // 3. Get All (Use carefully with large tables!)
        Task<IEnumerable<T>> GetAllAsync();

        // 4. Atomic Writes
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        
        // 5. Deletes
        void Remove(T entity);
        
        // 6. Save Changes (Unit of Work pattern usually handles this, but good to have)
        Task SaveChangesAsync();
    }
}
