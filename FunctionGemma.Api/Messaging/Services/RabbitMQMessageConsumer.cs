using FunctionGemma.Api.Interfaces;

namespace FunctionGemma.Api.Messaging.Services
{
    public class RabbitMQMessageConsumer(IServiceScopeFactory serviceScopeFactory) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };

            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(exchange: "Inference", type: ExchangeType.Direct);

            var queueDeclareResult = await channel.QueueDeclareAsync();
            string queueName = queueDeclareResult.QueueName;

            await channel.QueueBindAsync(queue: queueName, exchange: "Inference", routingKey: "QueueA");

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += (model, ea) =>
            {
                using IServiceScope scope = serviceScopeFactory.CreateScope();

                IScopedProcessingService scopedProcessingService =
                    scope.ServiceProvider.GetRequiredService<IScopedProcessingService>();

                var body = ea.Body.ToArray();
                var message = JsonSerializer.Deserialize<InferenceRequestMessage>(body) ?? throw new InvalidOperationException("Failed to deserialize message.");
                var routingKey = ea.RoutingKey;
                var response = await scopedProcessingService.GenerateTokensAsync(message, stoppingToken);
                return Task.CompletedTask;
            };
            await channel.BasicConsumeAsync(queueName, autoAck: true, consumer: consumer);
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            await base.StopAsync(stoppingToken);
        }
    }
}