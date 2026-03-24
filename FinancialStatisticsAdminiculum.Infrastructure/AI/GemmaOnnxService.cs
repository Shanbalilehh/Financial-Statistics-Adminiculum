using Microsoft.ML.OnnxRuntimeGenAI;
using FinancialStatisticsAdminiculum.Core.Interfaces;
using FinancialStatisticsAdminiculum.Application.Interfaces;
using System.Text;
using Microsoft.Extensions.Logging;

namespace FinancialStatisticsAdminiculum.Infrastructure.AI;

public class GemmaOnnxService : INlpEngine
{
    private readonly GemmaModelFactory _factory;
    private readonly IAiSchemaAggregator _schemaAggregator; 
    private readonly ILogger<GemmaOnnxService> _logger;

    public GemmaOnnxService(
        GemmaModelFactory factory, 
        IAiSchemaAggregator schemaAggregator, 
        ILogger<GemmaOnnxService> logger)
    {
        _factory = factory;
        _schemaAggregator = schemaAggregator;
        _logger = logger;
    }

    public async Task<string> ExtractToolCallAsync(string userPrompt)
    {
        string dynamicToolsJson = _schemaAggregator.BuildCombinedToolJson();
        _logger.LogDebug("DynamicToolsJson: {dynamicToolsJson}", dynamicToolsJson);
        string systemPrompt = $"You have access to the following tools:\n{dynamicToolsJson}\n\n";
        _logger.LogDebug("SystemPrompt: {systemPrompt}", systemPrompt);
        string fullPrompt = $"<start_of_turn>user\n{systemPrompt}{userPrompt}<end_of_turn>\n<start_of_turn>model\n";
        _logger.LogDebug("FullPrompt: {fullPrompt}", fullPrompt);

        _logger.LogInformation("Starting tool extraction");

        // Pure optimistic execution. If this blows up, the Interceptor catches it.
        var result = await Task.Run(() =>
        {
            using var sequences = _factory.Tokenizer.Encode(fullPrompt);
            using var generatorParams = new GeneratorParams(_factory.Model);
            generatorParams.SetSearchOption("temperature", 0.0);
            generatorParams.SetSearchOption("max_length", 500);

            using var generator = new Generator(_factory.Model, generatorParams);
            generator.AppendTokenSequences(sequences);

            using var tokenizerStream = _factory.Tokenizer.CreateStream();
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
        _logger.LogDebug("Extracted tool: {result}", result);
        return result;
    }
}