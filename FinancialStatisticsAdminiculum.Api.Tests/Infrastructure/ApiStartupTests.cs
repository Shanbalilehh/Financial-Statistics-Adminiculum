using System;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Castle.DynamicProxy;
using FinancialStatisticsAdminiculum.Application.AI.Factories;
using FinancialStatisticsAdminiculum.Application.AI.Interfaces;
using FinancialStatisticsAdminiculum.Application.AI.Parsers;
using FinancialStatisticsAdminiculum.Application.AI.SchemaAggregators;
using FinancialStatisticsAdminiculum.Application.AI.Services;
using FinancialStatisticsAdminiculum.Application.AI.Tools;
using FinancialStatisticsAdminiculum.Application.ExceptionHandling;
using FinancialStatisticsAdminiculum.Application.Interfaces;
using FinancialStatisticsAdminiculum.Application.Services;
using FinancialStatisticsAdminiculum.Core.Entities;
using FinancialStatisticsAdminiculum.Core.Interfaces;
using FinancialStatisticsAdminiculum.Infrastructure;
using FinancialStatisticsAdminiculum.Infrastructure.Messaging.Services;
using FinancialStatisticsAdminiculum.Infrastructure.Persistence;
using FinancialStatisticsAdminiculum.Infrastructure.Repositories;
using Xunit;

namespace FinancialStatisticsAdminiculum.Api.Tests.Infrastructure
{
    public class ApiStartupTests
    {
        [Fact]
        public void DependencyInjection_ShouldResolveRequiredServices_IncludingMessagePublisher()
        {
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new System.Collections.Generic.KeyValuePair<string, string?>("ConnectionStrings:LocalConnection", "Host=localhost;Database=TestDb;Username=test;Password=test"),
                    new System.Collections.Generic.KeyValuePair<string, string?>("RabbitMQ:Host", "localhost")
                })
                .Build();

            services.AddSingleton<IConfiguration>(configuration);
            services.AddLogging(builder => builder.AddConsole());

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("LocalConnection")));

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddSingleton<IJobCompletionNotifier, JobCompletionNotifier>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddSingleton<IFunctionGemmaParser, FunctionGemmaParser>();
            services.AddScoped<IToolResolver, ToolResolver>();
            services.AddScoped<IAiSchemaAggregator, AiSchemaAggregator>();
            services.AddScoped<DatabaseSeeder>();

            // The fix: IMessagePublisher is registered
            services.AddScoped<IMessagePublisher, RabbitMQMessagePublisher>();
            services.AddHostedService<RabbitMQMessageConsumer>();

            services.AddSingleton<ProxyGenerator>();
            services.AddTransient<SecurityExceptionInterceptor>();
            services.AddScoped<IWorkspaceService, WorkspaceService>();
            services.AddScoped<INlpCommandService, NlpCommandService>();

            services.AddScoped<OrchestratorService>();
            services.AddScoped<IOrchestratorService>(provider =>
            {
                var proxyGenerator = provider.GetRequiredService<ProxyGenerator>();
                var interceptor = provider.GetRequiredService<SecurityExceptionInterceptor>();
                var implementation = provider.GetRequiredService<OrchestratorService>();
                return proxyGenerator.CreateInterfaceProxyWithTarget<IOrchestratorService>(implementation, interceptor);
            });

            using var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });

            using var scope = serviceProvider.CreateScope();

            // Verify OrchestratorService and its IMessagePublisher dependency resolve cleanly
            var orchestrator = scope.ServiceProvider.GetService<IOrchestratorService>();
            orchestrator.Should().NotBeNull();

            var publisher = scope.ServiceProvider.GetService<IMessagePublisher>();
            publisher.Should().NotBeNull();
            publisher.Should().BeOfType<RabbitMQMessagePublisher>();
        }
    }
}
