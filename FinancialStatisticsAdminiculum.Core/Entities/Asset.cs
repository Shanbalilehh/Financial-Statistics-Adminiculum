using System;
using System.Collections.Generic;

namespace FinancialStatisticsAdminiculum.Core.Entities
{
    public class Asset
    {
        // Constructor for EF Core
        protected Asset() { }

        public Asset(string ticker, string name, string assetType)
        {
            if (string.IsNullOrWhiteSpace(ticker)) 
                throw new ArgumentException("Asset ticker required", nameof(ticker)); // Fixed
            if (string.IsNullOrWhiteSpace(name)) 
                throw new ArgumentException("Asset name required", nameof(name));     // Fixed
            if (string.IsNullOrWhiteSpace(assetType)) 
                throw new ArgumentException("Asset type required", nameof(assetType));

            Ticker = ticker;
            Name = name;
            AssetType = assetType;
        }

        public int Id { get; private set; }

        // Surrogate Key for DB/PK (we will use Ticker as PK via configuration)
        public string Ticker { get; private set; }
        public string Name { get; private set; }
        public string AssetType { get; private set; }

        // Navigation Property (One Asset has many DataPoints)
        private readonly List<PricePoint> _pricePoints = new();
        public IReadOnlyCollection<PricePoint> MarketData => _pricePoints;
    }
}
