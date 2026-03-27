using FinancialStatisticsAdminiculum.Application.AI.Entities;

namespace FinancialStatisticsAdminiculum.Application.AI.Interfaces
{
    public interface IGemmaTool
    {
        string Name {get;}
        string Description { get; }
        Dictionary<string, GemmaParameter> Parameters { get; } 
        Task<ToolExecutionResult> ExecuteAsync(Dictionary<string, string> args);
    }
    
}