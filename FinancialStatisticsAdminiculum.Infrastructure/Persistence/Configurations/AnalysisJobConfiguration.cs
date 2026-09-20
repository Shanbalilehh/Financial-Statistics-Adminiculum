using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinancialStatisticsAdminiculum.Core.Entities;

namespace FinancialStatisticsAdminiculum.Infrastructure.Persistence.configurations
{
    public class AnalysisJobConfiguration : IEntityTypeConfiguration<AnalysisJob>
    {
        public void Configure(EntityTypeBuilder<AnalysisJob> builder)
        {
            builder.ToTable("AnalysisJobs");

            builder.HasKey(j => j.CorrelationId);

            builder.Property(job => job.History)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null!),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<ChatMessage>>(v, (System.Text.Json.JsonSerializerOptions)null!) ?? new List<ChatMessage>());
        }
    }
}