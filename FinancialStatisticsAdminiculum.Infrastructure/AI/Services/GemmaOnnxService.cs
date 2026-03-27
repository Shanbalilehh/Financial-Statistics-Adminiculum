using Microsoft.ML.OnnxRuntimeGenAI;
using FinancialStatisticsAdminiculum.Application.Interfaces;
using FinancialStatisticsAdminiculum.Infrastructure.AI.Entities;
using System.Text;
using Microsoft.Extensions.Logging;
using FinancialStatisticsAdminiculum.Core.Interfaces;
using FinancialStatisticsAdminiculum.Core.Entities;

namespace FinancialStatisticsAdminiculum.Infrastructure.AI.Services
{
    public class GemmaOnnxService : IGemmaOnnxService
    {
        private readonly GemmaModelFactory _factory;
        private readonly IAiSchemaAggregator _schemaAggregator;
        private readonly ILogger<GemmaOnnxService> _logger;

        private readonly List<ChatMessage> _chatHistory = new();

        public GemmaOnnxService(
            GemmaModelFactory factory,
            IAiSchemaAggregator schemaAggregator,
            ILogger<GemmaOnnxService> logger)
        {
            _factory = factory;
            _schemaAggregator = schemaAggregator;
            _logger = logger;

            InitializeDeveloperPrompt();
        }

        private void InitializeDeveloperPrompt()
        {
            string dynamicToolsSchema = _schemaAggregator.BuildCombinedTool();
            _logger.LogDebug("Initialized Developer Schema: {Schema}", dynamicToolsSchema);

            string developerContent = $"You are a model that can do function calling with the following functions\n{dynamicToolsSchema}";
            _chatHistory.Add(new ChatMessage { Role = ChatRole.Developer, Content = developerContent });
        }

        public async Task<string> GenerateAsync(ChatRole role, string content)
        {
            _chatHistory.Add(new ChatMessage { Role = role, Content = content });

            string promptString = GemmaPromptFormatter.BuildPrompt(_chatHistory);
            string modelOutput = await GenerateTokensAsync(promptString);

            _chatHistory.Add(new ChatMessage { Role = ChatRole.Model, Content = modelOutput });
            return modelOutput;
        }

        private async Task<string> GenerateTokensAsync(string fullPrompt)
        {
            _logger.LogDebug("Starting token generation for prompt length: {Length}", fullPrompt.Length);

            return await Task.Run(() =>
            {
                using var sequences = _factory.Tokenizer.Encode(fullPrompt);
                using var generatorParams = new GeneratorParams(_factory.Model);

                generatorParams.SetSearchOption("temperature", 0.0);
                generatorParams.SetSearchOption("max_length", 2048);

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

                string result = sb.ToString();
                _logger.LogDebug("Generation complete. Produced {CharCount} chars.", result.Length);
                return result;
            });
        }
    }
}