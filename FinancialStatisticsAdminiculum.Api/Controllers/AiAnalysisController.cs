using Microsoft.AspNetCore.Mvc;
using FinancialStatisticsAdminiculum.Application.AI;

namespace FinancialStatisticsAdminiculum.API.Controllers
{
    // 1. DataAnnotations: ApiController, Route, and JSON response
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    
    // 2. Inherit from base class
    public class AiAnalysisController : ControllerBase
    {
        private readonly OrchestratorService _orchestratorService;

        // 3. Constructor DI
        public AiAnalysisController(OrchestratorService orchestratorService)
        {
            _orchestratorService = orchestratorService;
        }

        // 4. HTTP methods with ProducesResponseType
        [HttpPost("analyze")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        
        // 5. Service methods as tasks
        public async Task<IActionResult> AnalyzeTextAsync([FromBody] PromptRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest(new { Error = "The prompt cannot be empty." });
            }

            try
            {
                // This single line triggers the entire pipeline:
                // Gemma -> JSON -> Tool Strategy -> Database Repo -> Math Extensions
                string result = await _orchestratorService.ExecuteAiCommandAsync(request.Prompt);
                
                return Ok(new { Message = result });
            }
            catch (Exception ex)
            {
                // In production, we would log 'ex' here
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ex.Message });
            }
        }
    }

    // A simple DTO to bind the incoming JSON body
    public class PromptRequest
    {
        public string Prompt { get; set; } = string.Empty;
    }
}