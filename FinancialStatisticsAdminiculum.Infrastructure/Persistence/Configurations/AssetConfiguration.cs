using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinancialStatisticsAdminiculum.Core.Entities;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.ToTable("Assets");

        // Use Ticker as the principal key for relationships
        builder.HasKey(a => a.Ticker);

        // If you want to keep Id in the CLR model but not use it as PK, ignore it in mapping
        builder.Ignore(a => a.Id);

        builder.Property(a => a.Ticker)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(a => a.Name)
            .HasMaxLength(100)
            .IsRequired();
            
        builder.Property(a => a.AssetType)
            .HasMaxLength(20)
            .IsRequired();

        // Note: relationship mapping is defined in PricePointConfiguration to make the FK/principal-key explicit.
    }
}