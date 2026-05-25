using FinancialStatisticsAdminiculum.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using Shared.Contracts;

namespace FinancialStatisticsAdminiculum.Infrastructure.Messaging.Services
{
    public class RabbitMQMessageConsumer(IServiceScopeFactory serviceScopeFactory) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };

            using var connection = await factory.CreateConnectionAsync(stoppingToken);
            using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await channel.ExchangeDeclareAsync(exchange: "Response", type: ExchangeType.Direct, cancellationToken: stoppingToken);

            // Declare a temporary queue and bind it to the exchange with the routing key "QueueA"
            var queueDeclareResult = await channel.QueueDeclareAsync(cancellationToken: stoppingToken);
            string queueName = queueDeclareResult.QueueName;

            await channel.QueueBindAsync(queue: queueName, exchange: "Response", routingKey: "QueueB", cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                using IServiceScope scope = serviceScopeFactory.CreateScope();

                IOrchestratorService orchestratorService = scope.ServiceProvider.GetRequiredService<IOrchestratorService>();

                var requestBody = ea.Body.ToArray();
                var message = JsonSerializer.Deserialize<InferenceResponseMessage>(requestBody) ?? throw new InvalidOperationException("Failed to deserialize message.");
                await orchestratorService.ProcessInferenceResponseAsync(message, stoppingToken);
            };
            await channel.BasicConsumeAsync(queueName, autoAck: true, consumer: consumer, cancellationToken: stoppingToken);

            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            await base.StopAsync(stoppingToken);
        }

    }
}