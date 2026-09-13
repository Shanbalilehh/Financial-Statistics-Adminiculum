using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinancialStatisticsAdminiculum.Application.DTOs;
using FinancialStatisticsAdminiculum.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinancialStatisticsAdminiculum.Api.Controllers
{
    public class NlpPromptDto
    {
        public string Prompt { get; set; } = null!;
    }

    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class WorkspacesController : ControllerBase
    {
        private readonly IWorkspaceService _workspaceService;
        private readonly FinancialStatisticsAdminiculum.Application.AI.Services.INlpCommandService _nlpCommandService;

        public WorkspacesController(
            IWorkspaceService workspaceService,
            FinancialStatisticsAdminiculum.Application.AI.Services.INlpCommandService nlpCommandService)
        {
            _workspaceService = workspaceService;
            _nlpCommandService = nlpCommandService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<WorkspaceSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListWorkspaces(CancellationToken ct)
        {
            var workspaces = await _workspaceService.GetAllWorkspacesAsync(ct);
            return Ok(workspaces);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(WorkspaceDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWorkspaceById(Guid id, CancellationToken ct)
        {
            var workspace = await _workspaceService.GetWorkspaceByIdAsync(id, ct);
            if (workspace == null)
            {
                return NotFound(new { message = $"Workspace {id} was not found." });
            }

            return Ok(workspace);
        }

        [HttpPost]
        [ProducesResponseType(typeof(WorkspaceDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateWorkspace([FromBody] CreateWorkspaceDto request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request?.Name))
            {
                return BadRequest(new { message = "Workspace name cannot be empty." });
            }

            var workspace = await _workspaceService.CreateWorkspaceAsync(request, ct);
            return CreatedAtAction(nameof(GetWorkspaceById), new { id = workspace.Id }, workspace);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(WorkspaceDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateWorkspace(Guid id, [FromBody] UpdateWorkspaceDto request, CancellationToken ct)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { message = "Workspace name cannot be empty." });
            }

            try
            {
                var workspace = await _workspaceService.UpdateWorkspaceAsync(id, request, ct);
                return Ok(workspace);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Workspace {id} was not found." });
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteWorkspace(Guid id, CancellationToken ct)
        {
            var deleted = await _workspaceService.DeleteWorkspaceAsync(id, ct);
            if (!deleted)
            {
                return NotFound(new { message = $"Workspace {id} was not found." });
            }

            return NoContent();
        }

        [HttpPost("{id:guid}/nlp-command")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ProcessNlpCommand(Guid id, [FromBody] NlpPromptDto request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request?.Prompt))
            {
                return BadRequest(new { message = "Prompt cannot be empty." });
            }

            var result = await _nlpCommandService.ProcessCommandAsync(id, request.Prompt, ct);
            return Ok(result);
        }
    }
}
