using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace LocalAiWorker
{
    public class Worker(ILogger<Worker> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var factory = new ConnectionFactory { HostName = "192.168.88.252", UserName = "pi", Password = "raspberry" };

                var connection = await factory.CreateConnectionAsync();
                IChannel channel = await connection.CreateChannelAsync();
                var consumer = new AsyncEventingBasicConsumer(channel);

                consumer.ReceivedAsync += async (model, ea) =>
                {
                    logger.LogInformation("Received message: {0}", Encoding.UTF8.GetString(ea.Body.ToArray()));
                    await Task.Yield();
                };

                await channel.BasicConsumeAsync("queue", autoAck: true, consumer: consumer, cancellationToken: stoppingToken);

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
        }
    }
}
