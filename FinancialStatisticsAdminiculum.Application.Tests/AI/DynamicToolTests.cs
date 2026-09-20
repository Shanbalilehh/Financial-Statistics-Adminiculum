using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using FinancialStatisticsAdminiculum.Application.AI.Factories;
using FinancialStatisticsAdminiculum.Application.AI.Interfaces;
using FinancialStatisticsAdminiculum.Application.AI.Services;
using FinancialStatisticsAdminiculum.Application.AI.Tools;
using Xunit;

namespace FinancialStatisticsAdminiculum.Application.Tests.AI
{
    public class DynamicToolTests
    {
        [Fact]
        public async Task VolatilityToolHandler_ShouldReturnSuccess_WhenTickerProvided()
        {
            var handler = new VolatilityToolHandler();
            var args = new Dictionary<string, string>
            {
                ["ticker"] = "AAPL",
                ["period"] = "20"
            };

            var result = await handler.ExecuteAsync(args);

            result.IsSuccess.Should().BeTrue();
            result.Payload.Should().Contain("VolatilityEstimator");
            result.Payload.Should().Contain("\"ticker\": \"AAPL\"");
            result.Payload.Should().Contain("\"period\": 20");
        }

        [Fact]
        public async Task VolatilityToolHandler_ShouldReturnFailure_WhenTickerMissing()
        {
            var handler = new VolatilityToolHandler();
            var args = new Dictionary<string, string>();

            var result = await handler.ExecuteAsync(args);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("Missing or empty 'ticker'");
        }

        [Fact]
        public async Task SignalTriggerToolHandler_ShouldReturnSuccess_WithCorrectConditionAndThreshold()
        {
            var handler = new SignalTriggerToolHandler();
            var args = new Dictionary<string, string>
            {
                ["condition"] = "GreaterThan",
                ["threshold"] = "35.5"
            };

            var result = await handler.ExecuteAsync(args);

            result.IsSuccess.Should().BeTrue();
            result.Payload.Should().Contain("SignalTrigger");
            result.Payload.Should().Contain("\"condition\": \"GreaterThan\"");
            result.Payload.Should().Contain("\"threshold\": 35.5");
        }

        [Fact]
        public void ToolResolver_ShouldResolveRegisteredTools()
        {
            var services = new ServiceCollection();
            services.AddKeyedScoped<IGemmaTool, VolatilityToolHandler>(VolatilityToolHandler.ToolName);
            services.AddKeyedScoped<IGemmaTool, SignalTriggerToolHandler>(SignalTriggerToolHandler.ToolName);
            services.AddScoped<IToolResolver, ToolResolver>();

            var provider = services.BuildServiceProvider();
            var resolver = provider.GetRequiredService<IToolResolver>();

            var volTool = resolver.Resolve(VolatilityToolHandler.ToolName);
            volTool.Should().NotBeNull();
            volTool!.Name.Should().Be(VolatilityToolHandler.ToolName);

            var triggerTool = resolver.Resolve(SignalTriggerToolHandler.ToolName);
            triggerTool.Should().NotBeNull();
            triggerTool!.Name.Should().Be(SignalTriggerToolHandler.ToolName);

            var unknown = resolver.Resolve("non_existent_tool");
            unknown.Should().BeNull();
        }

        [Fact]
        public async Task NlpCommandService_ShouldSynthesizeVolatilityPipeline_WithTrigger()
        {
            var services = new ServiceCollection();
            services.AddKeyedScoped<IGemmaTool, VolatilityToolHandler>(VolatilityToolHandler.ToolName);
            services.AddKeyedScoped<IGemmaTool, SignalTriggerToolHandler>(SignalTriggerToolHandler.ToolName);
            services.AddScoped<IToolResolver, ToolResolver>();
            var provider = services.BuildServiceProvider();

            var logger = NullLogger<NlpCommandService>.Instance;
            var resolver = provider.GetRequiredService<IToolResolver>();
            var service = new NlpCommandService(logger, resolver);

            var result = await service.ProcessCommandAsync(
                Guid.NewGuid(),
                "Add a 30-day volatility estimator for AAPL and trigger alert when volatility exceeds 25%"
            );

            result.Status.Should().Be("Executed");
            result.ResolvedTool.Should().Be(VolatilityToolHandler.ToolName);
            result.Mutations.Should().HaveCount(5); // PriceStream, Volatility, Conn1, Trigger, Conn2
            result.Explanation.Should().Contain("volatility pipeline");
        }

        [Fact]
        public async Task NlpCommandService_ShouldRejectImpossibleOperations()
        {
            var logger = NullLogger<NlpCommandService>.Instance;
            var service = new NlpCommandService(logger);

            var result = await service.ProcessCommandAsync(
                Guid.NewGuid(),
                "Compute negative variance for MSFT"
            );

            result.Status.Should().Be("Rejected");
            result.Explanation.Should().Contain("Contradiction detected");
            result.Mutations.Should().BeEmpty();
        }

        [Fact]
        public async Task NlpCommandService_ShouldReturnUnresolved_ForAmbiguousInput()
        {
            var logger = NullLogger<NlpCommandService>.Instance;
            var service = new NlpCommandService(logger);

            var result = await service.ProcessCommandAsync(
                Guid.NewGuid(),
                "Hello world what can you do"
            );

            result.Status.Should().Be("Unresolved");
            result.Explanation.Should().Contain("Could not resolve analytical intent");
            result.Mutations.Should().BeEmpty();
        }
    }
}
