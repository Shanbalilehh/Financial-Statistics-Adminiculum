using Microsoft.ML.OnnxRuntimeGenAI;
using FinancialStatisticsAdminiculum.Core.Interfaces;
using FinancialStatisticsAdminiculum.Application.Interfaces;
using FinancialStatisticsAdminiculum.Infrastructure.AI.Entities;
using System.Text;
using Microsoft.Extensions.Logging;
using FinancialStatisticsAdminiculum.Infrastructure.AI.Interfaces;

namespace FinancialStatisticsAdminiculum.Infrastructure.AI.Services
{
    public class GemmaOnnxService : IGemmaOnnxService
    {
        private readonly GemmaModelFactory _factory;
        private readonly IAiSchemaAggregator _schemaAggregator; 
        private readonly IOrchestratorService _orchestratorService;
        private readonly ILogger<GemmaOnnxService> _logger;
        
        // Scoped state for the current conversation history
        private readonly List<ChatMessage> _chatHistory = new();

        public GemmaOnnxService(
            GemmaModelFactory factory, 
            IAiSchemaAggregator schemaAggregator, 
            IOrchestratorService orchestratorService,
            ILogger<GemmaOnnxService> logger)
        {
            _factory = factory;
            _schemaAggregator = schemaAggregator;
            _orchestratorService = orchestratorService;
            _logger = logger;

            // Automatically configure the system prompt with your custom schema format
            InitializeDeveloperPrompt();
        }

        private void InitializeDeveloperPrompt()
        {
            // 1. Get the custom <escape> formatted tools schema from the Aggregator
            string dynamicToolsSchema = _schemaAggregator.BuildCombinedTool();
            _logger.LogDebug("Initialized Developer Schema: {Schema}", dynamicToolsSchema);
            
            string developerContent = $"You are a model that can do function calling with the following functions\n{dynamicToolsSchema}";
            
            _chatHistory.Add(new ChatMessage { Role = ChatRole.Developer, Content = developerContent });
        }

        public async Task<string> ProcessUserPromptAsync(string userPrompt)
        {
            _logger.LogInformation("Processing new user prompt: {PromptLength} chars", userPrompt.Length);
            
            // 2. Add user message to history
            _chatHistory.Add(new ChatMessage { Role = ChatRole.User, Content = userPrompt });

            // 3. Format history and generate initial model response
            string promptString = GemmaPromptFormatter.BuildPrompt(_chatHistory);
            string modelOutput = await GenerateTokensAsync(promptString);

            // 4. Pass the raw output to your Orchestrator for zero-allocation parsing and execution
            string toolResponses = await _orchestratorService.ParseAndExecuteToolsAsync(modelOutput);

            if (!string.IsNullOrEmpty(toolResponses))
            {
                _logger.LogInformation("Tools were executed by Orchestrator. Appending results and generating final response.");
                
                // 5. Add the model's tool call request AND the formatted tool results to history
                _chatHistory.Add(new ChatMessage { Role = ChatRole.Model, Content = modelOutput });
                _chatHistory.Add(new ChatMessage { Role = ChatRole.Tool, Content = toolResponses });

                // 6. Generate the final human-readable text response with the new context
                string finalPromptString = GemmaPromptFormatter.BuildPrompt(_chatHistory);
                string finalOutput = await GenerateTokensAsync(finalPromptString);
                
                _chatHistory.Add(new ChatMessage { Role = ChatRole.Model, Content = finalOutput });
                return finalOutput;
            }

            // Standard conversation (No tools were detected or called)
            _logger.LogInformation("No tools requested. Returning direct text response.");
            _chatHistory.Add(new ChatMessage { Role = ChatRole.Model, Content = modelOutput });
            return modelOutput;
        }

        // Your original ONNX generation logic, extracted for reusability
        private async Task<string> GenerateTokensAsync(string fullPrompt)
        {
            _logger.LogDebug("Starting token generation for prompt length: {Length}", fullPrompt.Length);

            return await Task.Run(() =>
            {
                using var sequences = _factory.Tokenizer.Encode(fullPrompt);
                using var generatorParams = new GeneratorParams(_factory.Model);
                
                // Temperature 0.0 is perfect for deterministic function calling
                generatorParams.SetSearchOption("temperature", 0.0);
                generatorParams.SetSearchOption("max_length", 500);
                generatorParams.SetSearchOption("repetition_penalty", 1.3);

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