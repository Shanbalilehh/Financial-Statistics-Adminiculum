using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinancialStatisticsAdminiculum.Core.Entities;

namespace FinancialStatisticsAdminiculum.Infrastructure.Persistence.Configurations
{
    public class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>
    {
        public void Configure(EntityTypeBuilder<Workspace> builder)
        {
            builder.ToTable("Workspaces");

            builder.HasKey(w => w.Id);

            builder.Property(w => w.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(w => w.Description)
                .HasMaxLength(500);

            builder.Property(w => w.CreatedAt)
                .IsRequired();

            builder.Property(w => w.UpdatedAt)
                .IsRequired();

            builder.OwnsOne(w => w.Viewport, v =>
            {
                v.ToJson();
            });

            builder.OwnsMany(w => w.Entities, e =>
            {
                e.ToJson();
                e.OwnsOne(entity => entity.Position);
            });

            builder.OwnsMany(w => w.Connections, c =>
            {
                c.ToJson();
            });
        }
    }
}
