using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinancialStatisticsAdminiculum.Core.Entities;

public class PricePointConfiguration : IEntityTypeConfiguration<PricePoint>
{
    public void Configure(EntityTypeBuilder<PricePoint> builder)
    {
        builder.ToTable("PricePoints");

        // Surrogate Key (Auto-Increment)
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();

        // Precision for Financials
        builder.Property(p => p.Value)
            .HasColumnType("decimal(18,8)")
            .IsRequired();

        // Index for Speed
        builder.HasIndex(p => new { p.AssetTicker, p.Timestamp });

        // Explicit relationship:
        // Use Asset.Ticker (string) as principal key, and PricePoint.AssetTicker (string) as FK.
        builder.HasOne<Asset>()
               .WithMany(a => a.MarketData)
               .HasForeignKey(p => p.AssetTicker)
               .HasPrincipalKey(a => a.Ticker)
               .OnDelete(DeleteBehavior.Cascade);
    }
}