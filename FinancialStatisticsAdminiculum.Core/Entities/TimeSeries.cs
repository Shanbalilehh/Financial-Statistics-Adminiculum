using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialStatisticsAdminiculum.Core.Entities
{
    public class TimeSeries
    {

        public string Ticker { get; }
        private readonly DateTime[] _time;
        private readonly double[] _values;

        public ReadOnlySpan<DateTime> Time => _time.AsSpan();
        public ReadOnlySpan<double> Values => _values.AsSpan();

        public double[] GetRawValues() => _values;

        public TimeSeries(string ticker, DateTime[] time, double[] values)
        {
            if (time.Length != values.Length)
                throw new ArgumentException("Times and values must have the same length.");
            _time = time;
            _values = values;
            Ticker = ticker;

        }

        public IEnumerable<PricePoint> Points
        {
            get
            {
                for (int i = 0; i < _time.Length; i++)
                {
                    yield return new PricePoint(Ticker, _time[i], (decimal)_values[i]);
                }
            }
        }
    }
}
