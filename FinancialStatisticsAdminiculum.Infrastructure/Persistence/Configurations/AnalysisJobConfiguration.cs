using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinancialStatisticsAdminiculum.Infrastructure.Messaging.Entities;

namespace FinancialStatisticsAdminiculum.Infrastructure.Persistence.configurations
{
    public class AnalysisJobConfiguration : IEntityTypeConfiguration<AnalysisJob>
    {
        public void Configure(EntityTypeBuilder<AnalysisJob> builder)
        {
            builder.ToTable("AnalysisJobs");

            builder.HasKey(j => j.CorrelationId);

            builder.OwnsMany(job => job.History, builder =>
            {
                builder.ToJson();
            });
        }
    }
}