namespace FinancialStatisticsAdminiculum.Application.DTOs
{
    // The main wrapper
    public class TimeSeriesDto
    {
        public string Ticker { get; set; } = string.Empty;
        public string IndicatorName { get; set; } = "Raw Price"; // e.g., "SMA-14"
        public List<DataPointDto> Data { get; set; } = new();
    }

    // A memory-efficient struct (no heap allocations per point!)
    public readonly record struct DataPointDto(DateTime Time, double Value);
}
