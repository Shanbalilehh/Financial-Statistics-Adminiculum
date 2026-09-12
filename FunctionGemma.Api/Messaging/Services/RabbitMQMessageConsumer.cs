using FunctionGemma.Api.Interfaces;
using Shared.Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace FunctionGemma.Api.Messaging.Services
{
    public class RabbitMQMessageConsumer(IServiceScopeFactory serviceScopeFactory) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };

            using var connection = await factory.CreateConnectionAsync(stoppingToken);
            using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await channel.ExchangeDeclareAsync(exchange: "Inference", type: ExchangeType.Direct, cancellationToken: stoppingToken);

            await channel.ExchangeDeclareAsync(exchange: "Response", type: ExchangeType.Direct, cancellationToken: stoppingToken);

            // Declare a temporary queue and bind it to the exchange with the routing key "QueueA"
            var queueDeclareResult = await channel.QueueDeclareAsync(cancellationToken: stoppingToken);
            string queueName = queueDeclareResult.QueueName;

            await channel.QueueBindAsync(queue: queueName, exchange: "Inference", routingKey: "QueueA", cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                using IServiceScope scope = serviceScopeFactory.CreateScope();

                IScopedProcessingService scopedProcessingService = scope.ServiceProvider.GetRequiredService<IScopedProcessingService>();

                var requestBody = ea.Body.ToArray();
                var message = JsonSerializer.Deserialize<InferenceRequestMessage>(requestBody) ?? throw new InvalidOperationException("Failed to deserialize message.");
                var response = await scopedProcessingService.GenerateTokensAsync(message, stoppingToken);

                // Publish response to queue "QueueB"
                var responseBody = JsonSerializer.SerializeToUtf8Bytes(response);
                await channel.BasicPublishAsync(exchange: "Response", routingKey: "QueueB", body: responseBody);
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