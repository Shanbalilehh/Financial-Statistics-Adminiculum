using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FinancialStatisticsAdminiculum.Core.Entities;

namespace FinancialStatisticsAdminiculum.Infrastructure.Persistence
{
    public class DatabaseSeeder
    {
        private readonly AppDbContext _context;

        public DatabaseSeeder(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            // 1. Automatically apply any pending migrations (Great for MVPs)
            if ((await _context.Database.GetPendingMigrationsAsync()).Any())
            {
                await _context.Database.MigrateAsync();
            }

            // 2. Check if data already exists
            if (await _context.Assets.AnyAsync(a => a.Ticker == "XAU"))
            {
                return; // Database is already seeded!
            }

            // 3. Create the Asset
            var gold = new Asset("XAU", "Gold", "Commodity");

            await _context.Assets.AddAsync(gold);

            // 4. Generate 100 Trading Days of Data (Random Walk)
            var random = new Random(42); // Fixed seed so tests are reproducible
            decimal currentPrice = 2000.00m;
            DateTime currentDate = DateTime.UtcNow.AddDays(-140); // Go back far enough to get 100 weekdays

            int tradingDaysGenerated = 0;

            while (tradingDaysGenerated < 100)
            {
                // Skip weekends to maintain our Trading-Day Window architecture
                if (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    currentDate = currentDate.AddDays(1);
                    continue;
                }

                var pricePoint = new PricePoint("XAU", currentDate, currentPrice);
                await _context.PricePoints.AddAsync(pricePoint);

                // Randomly alter the price between -1.5% and +1.5% daily
                decimal percentageChange = (decimal)(random.NextDouble() * 0.03 - 0.015);
                currentPrice = currentPrice * (1 + percentageChange);
                
                currentDate = currentDate.AddDays(1);
                tradingDaysGenerated++;
            }

            await _context.SaveChangesAsync();
        }
    }
}