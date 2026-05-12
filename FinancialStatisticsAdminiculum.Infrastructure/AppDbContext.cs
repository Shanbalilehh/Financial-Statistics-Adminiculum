using Microsoft.EntityFrameworkCore;
using FinancialStatisticsAdminiculum.Core.Entities;
using FinancialStatisticsAdminiculum.Infrastructure.Messaging.Entities;

namespace FinancialStatisticsAdminiculum.Infrastructure
{
    public class AppDbContext : DbContext
    {
        // Constructor handles the options (connection string) passed by the API
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Asset> Assets { get; set; }
        public DbSet<PricePoint> PricePoints { get; set; }
        public DbSet<AnalysisJob> AnalysisJobs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            base.OnModelCreating(modelBuilder);

            // This line automatically finds and applies the configurations above
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}