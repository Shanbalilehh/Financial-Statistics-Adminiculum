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
        private readonly IAiSchemaAggregator _schemaAggregator;
        private readonly ILogger<GemmaOnnxService> _logger;
        private readonly List<ChatMessage> _chatHistory = [];
        private readonly HttpClient _client;
        public GemmaOnnxService(
            IAiSchemaAggregator schemaAggregator,
            ILogger<GemmaOnnxService> logger,
            HttpClient client
            )
        {
            _schemaAggregator = schemaAggregator;
            _logger = logger;
            _client = client;

            InitializeDeveloperPrompt();
        }

        private void InitializeDeveloperPrompt()
        {
            string dynamicToolsSchema = _schemaAggregator.BuildCombinedTool();
            _logger.LogDebug("Initialized Developer Schema: {Schema}", dynamicToolsSchema);

            string developerContent = $"You are a model that can do function calling with the following functions\n{dynamicToolsSchema}";
            _chatHistory.Add(new ChatMessage { Role = ChatRole.Developer, Content = developerContent });
        }

        public async Task<string> GenerateAsync(ChatRole role, string content, CancellationToken ct = default)
        {
            _chatHistory.Add(new ChatMessage { Role = role, Content = content });

            string promptString = GemmaPromptFormatter.BuildPrompt(_chatHistory);
            string modelOutput = await GenerateTokensAsync(promptString, ct);

            _chatHistory.Add(new ChatMessage { Role = ChatRole.Model, Content = modelOutput });
            return modelOutput;
        }

        private async Task<string> GenerateTokensAsync(string fullPrompt, CancellationToken ct = default)
        {
            var response = await _client.PostAsync("/Inference", new StringContent(fullPrompt, Encoding.UTF8, "text/plain"), ct);
            
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync(ct);

            return body!;
        }
    }
}