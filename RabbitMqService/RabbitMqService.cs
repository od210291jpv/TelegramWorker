using RabbitMQ.Client;

using System.Text;
using System.Text.Json;

namespace RabbitMqService
{
    public class RabbitMqService : IRabbitMqService
    {
        private IChannel channel;
        private string queueName;

        public RabbitMqService(IChannel channel, string queue)
        {
            this.channel = channel;
            this.queueName = queue;
        }

        public static async Task<RabbitMqService> CreateAsync(string queue, string hostname, string username, string password)
        {
            var factory = new ConnectionFactory { HostName = hostname, UserName = username, Password = password };
            var connection = await factory.CreateConnectionAsync();
            IChannel channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            return new RabbitMqService(channel, queue);
        }

        public async Task SendMessage(object obj, string queue)
        {
            var message = JsonSerializer.Serialize(obj);
            await SendMessage(message, queue);
        }

        public async Task SendMessage(string message, string queue)
        {
            var body = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queue,
                body: body);

            Console.WriteLine($"Message sent. {message}");
        }
    }
}
