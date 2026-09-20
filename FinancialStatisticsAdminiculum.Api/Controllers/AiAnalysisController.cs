using Microsoft.AspNetCore.Mvc;
using FinancialStatisticsAdminiculum.Application.Interfaces;
using FinancialStatisticsAdminiculum.Api.DTOs;
using FinancialStatisticsAdminiculum.Core.Entities;
using FinancialStatisticsAdminiculum.Core.Interfaces;
using FinancialStatisticsAdminiculum.Application.AI;
using Shared.Entities;
using System.Text.Json;

namespace FinancialStatisticsAdminiculum.API.Controllers
{
    // 1. DataAnnotations: ApiController, Route, and JSON response
    [ApiController]
    [Route("api/[controller]")]
    
    // 2. Inherit from base class
    public class AiAnalysisController : ControllerBase
    {
        private readonly IOrchestratorService _orchestratorService;
        private readonly IRepository<AnalysisJob> _analysisJobRepository;
        private readonly IJobCompletionNotifier _notifier;

        // 3. Constructor DI
        public AiAnalysisController(
            IOrchestratorService orchestratorService,
            IRepository<AnalysisJob> analysisJobRepository,
            IJobCompletionNotifier notifier)
        {
            _orchestratorService = orchestratorService;
            _analysisJobRepository = analysisJobRepository;
            _notifier = notifier;
        }

        // 4. HTTP methods with ProducesResponseType
        [HttpPost("analyze")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        
        // 5. Service methods as tasks
        public async Task<IActionResult> AnalyzeTextAsync([FromBody] PromptRequest request)
        {
            Response.ContentType = "application/json";
            if (string.IsNullOrWhiteSpace(request.Prompt))
                return BadRequest(new { Error = "The prompt cannot be empty." });
            
            var correlationId = await _orchestratorService.HandleUserMessageAsync(request.Prompt);
            
            return Accepted(new{ JobId = correlationId});
        }

        [HttpGet("{jobId}/stream")]
        public async Task Get(Guid jobId, CancellationToken ct)
        {
            Response.ContentType = "text/event-stream";

            await foreach (var jobEvent in _notifier.ReadAllEventsAsync(ct))
            {
                if (jobEvent.JobId != jobId)
                    continue;

                await Response.WriteAsync($"data: {JsonSerializer.Serialize(jobEvent)}\n\n", ct);
                await Response.Body.FlushAsync(ct);
                break;
            }
        }
    }
}