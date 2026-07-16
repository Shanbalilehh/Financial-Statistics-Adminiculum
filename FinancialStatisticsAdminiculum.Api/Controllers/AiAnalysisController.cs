using Microsoft.AspNetCore.Mvc;
using FinancialStatisticsAdminiculum.Application.Interfaces;
using FinancialStatisticsAdminiculum.Api.DTOs;
using FinancialStatisticsAdminiculum.Core.Entities;
using FinancialStatisticsAdminiculum.Core.Interfaces;
using FinancialStatisticsAdminiculum.Application.AI;
using Shared.Entities;

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
        public AiAnalysisController(IOrchestratorService orchestratorService, IRepository<AnalysisJob> analysisJobRepository)
        {
            _orchestratorService = orchestratorService;
            _analysisJobRepository = analysisJobRepository;
            
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

        /*[HttpGet("{jobId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAnalysisResultAsync(Guid jobId, CancellationToken ct)
        {
            // 1. Fetch the job using your fast PostgreSQL lookup
            var job = await AnalysisJobRepositoryExtensions.GetByCorrelationIdAsync(_analysisJobRepository, jobId, ct);
            
            if (job == null)
            {
                return NotFound(new { Error = "Job not found." });
            }

            // 2. If the ReAct loop is still ping-ponging, tell the client to keep waiting
            if (job.Status != State.finalResponse)
            {
                return Ok(new 
                { 
                    JobId = jobId, 
                    Status = job.Status.ToString() 
                });
            }

            // 3. If finished, extract the final text from the JSONB state bag
            // Assuming the last message in the history is the model's final output
            var finalMessage = job.History.LastOrDefault(m => m.Role == ChatRole.Model);

            return Ok(new 
            {
                JobId = jobId,
                Status = job.Status.ToString(),
                Result = finalMessage?.Content
            });
        }*/
        [HttpGet("{jobId}/stream")]
        public async Task Get()
        {
            Response.ContentType = "text/event-stream";

            await foreach (var jobEvent in  )

        }
    }
}