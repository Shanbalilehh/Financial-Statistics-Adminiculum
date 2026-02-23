using System;
using FinancialStatisticsAdminiculum.Core.Entities;

namespace FinancialStatisticsAdminiculum.Core.Extensions
{
    public static class TrendExtensions
    {
        // Extension method: acts fully on the series
        public static TimeSeries CalculateSMA(this TimeSeries source, int period)
        {
            if (period <= 0 || period > source.Values.Length)
                throw new ArgumentException("Invalid period for SMA.");

            ReadOnlySpan<double> values = source.Values;
            double[] smoothedValues = new double[values.Length];
            
            // 1. Pad the beginning where we don't have enough data
            for (int i = 0; i < period - 1; i++)
            {
                smoothedValues[i] = double.NaN; 
            }

            // 2. Calculate the first full window
            double currentSum = 0;
            for (int i = 0; i < period; i++)
            {
                currentSum += values[i];
            }
            smoothedValues[period - 1] = currentSum / period;

            // 3. Ultra-fast rolling window for the rest of the series (O(n) complexity)
            for (int i = period; i < values.Length; i++)
            {
                currentSum += values[i] - values[i - period];
                smoothedValues[i] = currentSum / period;
            }

            // Return a new TimeSeries using the original timestamps!
            return new TimeSeries(source.Ticker, source.Time.ToArray(), smoothedValues);
        }
    }
}
