using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using FinancialStatisticsAdminiculum.Api.Controllers;
using FinancialStatisticsAdminiculum.Application.AI.Services;
using FinancialStatisticsAdminiculum.Application.DTOs;
using FinancialStatisticsAdminiculum.Application.Interfaces;
using Xunit;

namespace FinancialStatisticsAdminiculum.Api.Tests.Controllers
{
    public class NlpCommandControllerTests
    {
        private readonly Mock<IWorkspaceService> _mockWorkspaceService;
        private readonly Mock<INlpCommandService> _mockNlpCommandService;
        private readonly WorkspacesController _controller;

        public NlpCommandControllerTests()
        {
            _mockWorkspaceService = new Mock<IWorkspaceService>();
            _mockNlpCommandService = new Mock<INlpCommandService>();
            _controller = new WorkspacesController(_mockWorkspaceService.Object, _mockNlpCommandService.Object);
        }

        [Fact]
        public async Task ProcessNlpCommand_ShouldReturnBadRequest_WhenPromptIsEmpty()
        {
            var workspaceId = Guid.NewGuid();
            var request = new NlpPromptDto { Prompt = "   " };

            var result = await _controller.ProcessNlpCommand(workspaceId, request, CancellationToken.None);

            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task ProcessNlpCommand_ShouldReturnBadRequest_WhenRequestIsNull()
        {
            var workspaceId = Guid.NewGuid();

            var result = await _controller.ProcessNlpCommand(workspaceId, null!, CancellationToken.None);

            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task ProcessNlpCommand_ShouldReturnOk_WithResult_WhenPromptIsValid()
        {
            var workspaceId = Guid.NewGuid();
            var prompt = "Add 30-day volatility for AAPL";
            var request = new NlpPromptDto { Prompt = prompt };

            var expectedResult = new NlpCommandResultDto
            {
                CommandId = Guid.NewGuid(),
                Prompt = prompt,
                Status = "Executed",
                ResolvedTool = "calculate_rolling_volatility",
                Mutations = new List<CanvasMutationDto>
                {
                    new() { Action = "ADD_ENTITY", Payload = new { type = "VolatilityEstimator" } }
                },
                Explanation = "Added volatility estimator for AAPL"
            };

            _mockNlpCommandService
                .Setup(s => s.ProcessCommandAsync(workspaceId, prompt, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            var actionResult = await _controller.ProcessNlpCommand(workspaceId, request, CancellationToken.None);

            var okResult = actionResult.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);

            var responseDto = okResult.Value.Should().BeOfType<NlpCommandResultDto>().Subject;
            responseDto.Prompt.Should().Be(prompt);
            responseDto.Status.Should().Be("Executed");
            responseDto.ResolvedTool.Should().Be("calculate_rolling_volatility");
            responseDto.Mutations.Should().HaveCount(1);
        }

        [Fact]
        public async Task ProcessNlpCommand_ShouldPassWorkspaceIdAndPromptToService()
        {
            var workspaceId = Guid.NewGuid();
            var prompt = "Add SMA 20 for MSFT";
            var request = new NlpPromptDto { Prompt = prompt };

            _mockNlpCommandService
                .Setup(s => s.ProcessCommandAsync(workspaceId, prompt, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new NlpCommandResultDto { CommandId = Guid.NewGuid(), Prompt = prompt, Status = "Executed" });

            await _controller.ProcessNlpCommand(workspaceId, request, CancellationToken.None);

            _mockNlpCommandService.Verify(
                s => s.ProcessCommandAsync(workspaceId, prompt, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
    }
}
