using FinancialStatisticsAdminiculum.Application.AI.Entities;

namespace FinancialStatisticsAdminiculum.Application.Interfaces
{
    public interface IFunctionGemmaParser
    {
        List<GemmaToolCall> ParseToolCalls(string modelOutput);
    }
}