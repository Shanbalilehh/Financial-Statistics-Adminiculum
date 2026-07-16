using Microsoft.Extensions.Logging;
using FinancialStatisticsAdminiculum.Application.Interfaces;
using FinancialStatisticsAdminiculum.Application.AI.Interfaces;
using FinancialStatisticsAdminiculum.Application.AI.SchemaAggregators;
using FinancialStatisticsAdminiculum.Core.Entities;
using Shared.Contracts;
using Shared.Entities;
using FinancialStatisticsAdminiculum.Core.Interfaces;

namespace FinancialStatisticsAdminiculum.Application.AI.Services
{
    public class OrchestratorService : IOrchestratorService
    {
        private readonly IToolResolver _resolver;
        private readonly ILogger<OrchestratorService> _logger;
        private readonly IFunctionGemmaParser _parser;
        private readonly IMessagePublisher _publisher;
        private readonly IAiSchemaAggregator _schemaAggregator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<AnalysisJob> _analysisJobRepository;
        private readonly IJobCompletionNotifier _notifier;

        public OrchestratorService(
            IToolResolver resolver,
            ILogger<OrchestratorService> logger,
            IFunctionGemmaParser parser,
            IMessagePublisher publisher,
            IAiSchemaAggregator schemaAggregator,
            IUnitOfWork unitOfWork,
            IRepository<AnalysisJob> analysisJobRepository,
            IJobCompletionNotifier notifier
            )
        {
            _resolver = resolver;
            _logger = logger;
            _parser = parser;
            _publisher = publisher;
            _schemaAggregator = schemaAggregator;
            _unitOfWork = unitOfWork;
            _analysisJobRepository = analysisJobRepository;
            _notifier = notifier;
        }

        public async Task<Guid> HandleUserMessageAsync(string userPrompt, CancellationToken ct = default)
        {

            // Create a new analysis job for this user prompt
            var job = new AnalysisJob{ Status = State.Pending };

            // Create and save available tools schema as the first message to the model, so it has the context of what tools it can use before it generates any response.
            string dynamicToolsSchema = _schemaAggregator.BuildCombinedTool();
            _logger.LogDebug("Initialized Developer Schema: {Schema}", dynamicToolsSchema);
            string developerContent = $"You are a model that can do function calling with the following functions\n{dynamicToolsSchema}";
            job.AddChatMessage(new ChatMessage { Role = ChatRole.Developer, Content = developerContent });

            // Add the user's message to the job history and save it, so that the prompt builder can include it when constructing the prompt for the model.
            _logger.LogInformation("Handling user message: {PromptLength} chars", userPrompt.Length);
            job.AddChatMessage(new ChatMessage { Role = ChatRole.User, Content = userPrompt});

            // Persist the new job with its initial history to the database
            await _analysisJobRepository.AddAsync(job, ct);
            await _unitOfWork.CompleteAsync(ct);

            // Build the prompt for the model using the chat history and publish the inference request
            string functionCallPromptString = GemmaPromptFormatter.BuildPrompt(job.History);
            var requestMessage = new InferenceRequestMessage(job.CorrelationId, functionCallPromptString);
            await _publisher.PublishRequest(requestMessage);

            return job.CorrelationId;
        }
        public async Task ProcessInferenceResponseAsync(InferenceResponseMessage response, CancellationToken ct)
        {
            var job = await _analysisJobRepository.GetByCorrelationIdAsync(response.CorrelationId, ct);
            
            // Append the model's raw response to the history
            job.AddChatMessage(new ChatMessage { Role = ChatRole.Model, Content = response.GeneratedText });
            
            // Parse the model's text to determine our next action
            var toolCalls = _parser.ParseToolCalls(response.GeneratedText);

            if (toolCalls.Count > 0)
            {
                // === STATE: FUNCTION CALL ===
                var toolResults = new List<string>();
                foreach (var call in toolCalls)
                {
                    ct.ThrowIfCancellationRequested();
                    _logger.LogInformation("Executing tool {ToolName} with args {@Args}", call.Name, call.Arguments);

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
                }

                // Append the tool results to history
                job.AddChatMessage(new ChatMessage { Role = ChatRole.Tool, Content = string.Join("", toolResults) });
                await _unitOfWork.CompleteAsync(ct);

                // Send back to Inference API
                string nextPrompt = GemmaPromptFormatter.BuildPrompt(job.History);
                await _publisher.PublishRequest(new InferenceRequestMessage(job.CorrelationId, nextPrompt));
            }
            else
            {
                // === STATE: FINAL RESPONSE ===
                job.Status = State.finalResponse; 
                await _unitOfWork.CompleteAsync(ct); 

                await _notifier.NotifyJobCompletedAsync(
                new JobCompletedEvent(job.CorrelationId, response.GeneratedText), ct);
                
                _logger.LogInformation("Job {Id} completed successfully.", job.CorrelationId);
            }
        }
    }
}