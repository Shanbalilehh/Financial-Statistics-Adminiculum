using Microsoft.Extensions.Logging;
using SerilogTimings;
using FinancialStatisticsAdminiculum.Application.Interfaces;
using FinancialStatisticsAdminiculum.Application.AI.Interfaces;
using FinancialStatisticsAdminiculum.Application.AI.SchemaAggregators;
using FinancialStatisticsAdminiculum.Core.Entities;
using Shared.Contracts;
using FinancialStatisticsAdminiculum.Core.Interfaces;

namespace FinancialStatisticsAdminiculum.Application.AI.Services
{
    public class OrchestratorService : IOrchestratorService
    {
        private readonly IToolResolver _resolver;
        private readonly ILogger<OrchestratorService> _logger;
        private readonly IFunctionGemmaParser _parser;
        private readonly List<ChatMessage> _chatHistory = [];
        private readonly IMessagePublisher _publisher;
        private readonly IAiSchemaAggregator _schemaAggregator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<AnalysisJob> _analysisJobRepository;

        public OrchestratorService(
            IToolResolver resolver,
            ILogger<OrchestratorService> logger,
            IFunctionGemmaParser parser,
            List<ChatMessage> chatHistory,
            IMessagePublisher publisher,
            IAiSchemaAggregator schemaAggregator,
            IUnitOfWork unitOfWork,
            IRepository<AnalysisJob> analysisJobRepository
            )
        {
            _resolver = resolver;
            _logger = logger;
            _parser = parser;
            _chatHistory = chatHistory;
            _publisher = publisher;
            _schemaAggregator = schemaAggregator;
            _unitOfWork = unitOfWork;
            _analysisJobRepository = analysisJobRepository;
        }

        public async Task<Guid> HandleUserMessageAsync(string userPrompt, CancellationToken ct = default)
        {

            var job = new AnalysisJob();

            string dynamicToolsSchema = _schemaAggregator.BuildCombinedTool();
            _logger.LogDebug("Initialized Developer Schema: {Schema}", dynamicToolsSchema);

            string developerContent = $"You are a model that can do function calling with the following functions\n{dynamicToolsSchema}";
            await HistoryConstructor.AppendChatMessageAsync(_analysisJobRepository, job.CorrelationId, new ChatMessage { Role = ChatRole.Developer, Content = developerContent }, _unitOfWork, ct);

            _logger.LogInformation("Handling user message: {PromptLength} chars", userPrompt.Length);

            //using var op = Operation.At(LogEventLevel.Information).Begin("Handling user message");

            // First generation pass: user prompt → raw model output
            //string modelOutput = await _gemmaService.GenerateAsync(ChatRole.User, userPrompt, ct);
            

            await HistoryConstructor.AppendChatMessageAsync(_analysisJobRepository, job.CorrelationId, new ChatMessage { Role = ChatRole.User, Content = userPrompt }, _unitOfWork, ct);

            string functionCallPromptString = GemmaPromptFormatter.BuildPrompt(job.History);
            var requestMessage = new InferenceRequestMessage(job.CorrelationId, functionCallPromptString);
            await _publisher.PublishRequest(requestMessage);

            ////////////////
            //_chatHistory.Add(new ChatMessage { Role = ChatRole.Model, Content = modelOutput });
            ////////////////
            _logger.LogInformation("First model generation.");
            /*_logger.LogDebug("First model generation: {output}", modelOutput);

            // Attempt to parse tool calls from the raw output
            var toolCalls = _parser.ParseToolCalls(modelOutput);

            if (toolCalls.Count == 0)
            {
                _logger.LogInformation("No tool calls detected. Returning direct model response.");
                op.Complete();
                return modelOutput;
            }

            // Execute each tool and collect formatted responses
            var toolResults = new List<string>();

            foreach (var call in toolCalls)
            {
                ct.ThrowIfCancellationRequested();
                _logger.LogInformation("Executing tool {ToolName} with args {@Args}", call.Name, call.Arguments);

                using var toolOp = Operation.Time($"Executing tool {call.Name}");
                var handler = _resolver.Resolve(call.Name);

                if (handler == null)
                {
                    _logger.LogWarning("Tool '{ToolName}' is not implemented.", call.Name);
                    toolResults.Add($"<start_function_response>response:{call.Name}{{Error: Tool not implemented}}<end_function_response>");
                    continue;
                }

                var toolExecutionResult = await handler.ExecuteAsync(call.Arguments, ct);
                var toolResult = toolExecutionResult.Payload;
                _logger.LogInformation("Tool Executed: {toolName}", call.Name);
                _logger.LogDebug("Tool Result: {output}", toolResult);

                if (toolExecutionResult.IsSuccess)
                {
                    toolResults.Add(toolResult);
                }
                else
                {
                    _logger.LogError("Error executing tool '{ToolName}'", call.Name);
                    toolResults.Add($"<start_function_response>response:{call.Name}{{Error: Internal execution failure}}<end_function_response>");
                }
            }*/

            // Second generation pass: feed tool results back, get final response
            //string combinedToolResults = string.Join("", toolResults);
            //_logger.LogDebug("Second model generation: {output}", combinedToolResults);
            //_logger.LogInformation("Tools executed. Generating final response with tool context.");

            //_chatHistory.Add(new ChatMessage { Role = ChatRole.Tool, Content = combinedToolResults });

            string functionResponsePromptString = GemmaPromptFormatter.BuildPrompt(_chatHistory);
            //var functionResponseRequestMessage = new InferenceRequestMessage(correlationId, functionCallPromptString);
            //_publisher.PublishRequest(functionResponseRequestMessage);
            //_logger.LogDebug("Final output: {output}", finalOutput);

            //op.Complete();
            //return finalOutput;
        }
    }
}