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

            builder.Property(w => w.Viewport)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null!),
                    v => System.Text.Json.JsonSerializer.Deserialize<ViewportState>(v, (System.Text.Json.JsonSerializerOptions)null!) ?? new ViewportState());

            builder.Property(w => w.Entities)
                .HasField("_entities")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null!),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<AtomicEntity>>(v, (System.Text.Json.JsonSerializerOptions)null!) ?? new List<AtomicEntity>());

            builder.Property(w => w.Connections)
                .HasField("_connections")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null!),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<EntityConnection>>(v, (System.Text.Json.JsonSerializerOptions)null!) ?? new List<EntityConnection>());
        }
    }
}
