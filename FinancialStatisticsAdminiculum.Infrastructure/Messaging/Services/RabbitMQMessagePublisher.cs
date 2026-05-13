using Shared.Contracts;
using RabbitMQ.Client;
using System.Text.Json;
using FinancialStatisticsAdminiculum.Application.Interfaces;

namespace FinancialStatisticsAdminiculum.Infrastructure.Messaging.Services
{
    public class RabbitMQMessagePublisher : IMessagePublisher
    {
        public async Task PublishRequest(InferenceRequestMessage message)
        {

            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(exchange: "Inference", type: ExchangeType.Direct);

            var body = JsonSerializer.SerializeToUtf8Bytes(message);
            await channel.BasicPublishAsync(exchange: "Inference", routingKey: "Request", body: body);

        }
    }

}