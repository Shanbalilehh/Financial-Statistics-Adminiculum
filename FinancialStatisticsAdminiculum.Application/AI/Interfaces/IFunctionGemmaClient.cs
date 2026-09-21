using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FinancialStatisticsAdminiculum.Application.AI.Interfaces
{
    public class FunctionGemmaToolCall
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, object> Arguments { get; set; } = new();
    }

    public class FunctionGemmaResponse
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public List<FunctionGemmaToolCall> ToolCalls { get; set; } = new();
        public string? Content { get; set; }
    }

    public interface IFunctionGemmaClient
    {
        Task<FunctionGemmaResponse> CallModelAsync(string prompt, object? toolSchemas = null, CancellationToken ct = default);
        Task<bool> IsAvailableAsync(CancellationToken ct = default);
    }
}
