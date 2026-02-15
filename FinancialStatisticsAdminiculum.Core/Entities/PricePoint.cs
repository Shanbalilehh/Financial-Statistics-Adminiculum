using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialStatisticsAdminiculum.Core.Entities
{
    public class PricePoint
    {
        // EF Core Constructor
        protected PricePoint() { }

        public PricePoint(string assetTicker, DateTime timestamp, decimal value)
        {
            if (value < 0) throw new ArgumentException("Price cannot be negative", nameof(value));
            
            AssetTicker = assetTicker;
            Timestamp = timestamp;
            Value = value;
        }

        public int Id { get; private set; } // PK
        public string AssetTicker { get; private set; } // FK
        public DateTime Timestamp { get; private set; }
        public decimal Value { get; private set; } // The single atomic price
    }
}
