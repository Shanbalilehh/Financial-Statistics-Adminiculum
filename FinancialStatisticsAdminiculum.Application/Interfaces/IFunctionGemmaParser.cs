using FinancialStatisticsAdminiculum.Core.Entities;

namespace FinancialStatisticsAdminiculum.Application.Interfaces
{
    public interface IFunctionGemmaParser
    {
        List<GemmaToolCall> ParseToolCalls(string modelOutput);
    }
}