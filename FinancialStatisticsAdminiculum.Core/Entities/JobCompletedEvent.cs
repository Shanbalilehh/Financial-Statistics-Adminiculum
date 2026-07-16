namespace FinancialStatisticsAdminiculum.Core.Entities
{
    public record JobCompletedEvent(Guid JobId, string FinalResult);
}