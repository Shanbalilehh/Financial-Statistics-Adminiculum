using Microsoft.ML.OnnxRuntimeGenAI;
using FinancialStatisticsAdminiculum.Core.Interfaces;
using System.Text;
using Microsoft.Extensions.Logging;

namespace FinancialStatisticsAdminiculum.Infrastructure.AI
{
    public class GemmaOnnxService : INlpEngine, IDisposable
{
    private readonly Model _model;
    private readonly Tokenizer _tokenizer;
    private readonly string _systemPrompt;
    private readonly ILogger<GemmaOnnxService> _logger;

    public GemmaOnnxService(string modelPath, string dynamicToolsJson, ILogger<GemmaOnnxService> logger)
    {
        _logger = logger;
        _model = new Model(modelPath);
        _tokenizer = new Tokenizer(_model);
        _systemPrompt = BuildSystemPrompt(dynamicToolsJson);
    }

    private static string BuildSystemPrompt(string dynamicToolsJson) =>
        $"You have access to the following tools:\n{dynamicToolsJson}\n\n";

    public async Task<string> ExtractToolCallAsync(string userPrompt)
    {
        _logger.LogInformation("Starting tool extraction.");
        
        var result = await Task.Run(() =>
        {
            string fullPrompt =
                $"<start_of_turn>user\n{_systemPrompt}{userPrompt}<end_of_turn>\n<start_of_turn>model\n";

            using var sequences = _tokenizer.Encode(fullPrompt);

            using var generatorParams = new GeneratorParams(_model);
            generatorParams.SetSearchOption("temperature", 0.0);
            generatorParams.SetSearchOption("max_length", 500);

            using var generator = new Generator(_model, generatorParams);
            generator.AppendTokenSequences(sequences);

            using var tokenizerStream = _tokenizer.CreateStream();
            var sb = new StringBuilder();

            while (!generator.IsDone())
            {
                generator.GenerateNextToken();
                var newTokenId = generator.GetSequence(0)[^1];
                sb.Append(tokenizerStream.Decode(newTokenId));
            }

            return sb.ToString();
        });

        _logger.LogInformation("Tool extraction complete. Generated {CharCount} chars.", result.Length);
        return result;
    }

    public void Dispose()
    {
        _logger.LogInformation("Disposing GemmaOnnxService resources.");
        _tokenizer?.Dispose();
        _model?.Dispose();
        GC.SuppressFinalize(this);
    }
}
}