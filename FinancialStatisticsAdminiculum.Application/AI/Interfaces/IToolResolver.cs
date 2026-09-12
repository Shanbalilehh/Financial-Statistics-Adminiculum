namespace FinancialStatisticsAdminiculum.Application.AI.Interfaces
{
    public interface IToolResolver
    {
        IGemmaTool? Resolve(string name);
    }
}