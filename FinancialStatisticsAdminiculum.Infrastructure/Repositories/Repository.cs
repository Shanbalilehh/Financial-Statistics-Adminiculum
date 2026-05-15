using Microsoft.EntityFrameworkCore;
using FinancialStatisticsAdminiculum.Core.Interfaces;
using System.Linq.Expressions;

namespace FinancialStatisticsAdminiculum.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _dbSet.FindAsync(id, ct);
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        {
            return await _dbSet.Where(predicate).ToListAsync(ct);
        }

        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
        {
            return await _dbSet.ToListAsync(ct);
        }

        public async Task AddAsync(T entity, CancellationToken ct = default)
        {
            await _dbSet.AddAsync(entity, ct);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
        {
            await _dbSet.AddRangeAsync(entities, ct);
        }

        public void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }
    }
}
